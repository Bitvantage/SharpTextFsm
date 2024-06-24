/*
   Bitvantage.SharpTextFSM
   Copyright (C) 2024 Michael Crino
   
   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU Affero General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.
   
   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU Affero General Public License for more details.
   
   You should have received a copy of the GNU Affero General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Bitvantage.SharpTextFSM.Exceptions;

namespace Bitvantage.SharpTextFSM.TemplateHelpers;

public enum LineAction
{
    Next,
    Continue
}

public enum RecordAction
{
    NoRecord,
    Record,
    Clear,
    ClearAll
}

public abstract record RuleAction
{
}

public record ErrorAction : RuleAction
{
    public readonly string? Message;

    public ErrorAction()
    {
    }

    public ErrorAction(string? message)
    {
        Message = message;
    }
}

public record ChangeStateActionDefinition(string StateName) : RuleActionDefinition
{
}

public abstract record RuleActionDefinition
{
}

public record ErrorActionDefinition : RuleActionDefinition
{
    public readonly string? Message;

    public ErrorActionDefinition()
    {
    }

    public ErrorActionDefinition(string message)
    {
        Message = message;
    }
}

public record ChangeStateAction(TemplateState NewState) : RuleAction
{
}

public record StateFilter
{
    public StateFilter(IEnumerable<TemplateState>? states, bool invert, ImmutableHashSet<TemplateState> allStates)
    {
        Invert = invert;

        // if states is null, then use every state
        States = states?.ToImmutableHashSet() ?? allStates;

        // calculate the effective state, such that if the invert bit is set then use all states not specified
        EffectiveStates = invert ? allStates.Except(States).ToImmutableHashSet() : States;
    }

    public ImmutableHashSet<TemplateState> EffectiveStates { get; }
    public bool Invert { get; }
    public ImmutableHashSet<TemplateState> States { get; }
}

public record Rule
{
    internal Rule(Template template, TemplateState state, string pattern, StateFilter stateFilterFilter, LineAction lineAction, RecordAction recordAction, RuleAction? action, ValueDescriptorCollection valueDescriptorCollection)
    {
        Template = template;
        State = state;
        StateFilter = stateFilterFilter;
        LineAction = lineAction;
        RecordAction = recordAction;
        Action = action;
        Pattern = pattern;

        // expand the TextFSM ${variable} or $variable to a normal regex group with a prefix of 'textfsm_'
        var expandedPattern = valueDescriptorCollection.ValueDescriptorNamesRegex.Replace(pattern, match =>
        {
            if (match.Groups["invalid"].Success)
                throw new TemplateParseException("Undeclared value " + match.Value, ParseError.UndeclaredValue);

            if (match.Groups["doubleDollar"].Success)
                return "$";

            var name = match.Groups["name"].Value;
            if (!valueDescriptorCollection.TryGetValue(name, out var valueDescriptor))
                // TODO: need to test that this is an error or if it just ignores unresolvable value descriptors.
                // TODO: need to figure out how one escapes {}
                return match.Value;

            if (valueDescriptor.Options.HasFlag(Option.Regex))
                return (valueDescriptor.Regex.ToString());

            return $"(?<textfsm_{name}>{valueDescriptor.Regex})";
        });

        try
        {
            Regex = new Regex(expandedPattern, RegexOptions.Compiled);
        }
        catch (TemplateParseException exception)
        {
            throw new TemplateParseException($"Could not parse regular expression: {pattern}", ParseError.InvalidRegularExpression);
        }

        // sanity check
    }

    public RuleAction? Action { get; }
    public LineAction LineAction { get; init; } = LineAction.Next;
    public Regex Regex { get; init; }
    public string Pattern { get; init; }
    public RecordAction RecordAction { get; init; } = RecordAction.NoRecord;
    public Template Template { get; }
    public TemplateState State { get; }
    public StateFilter StateFilter { get; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append(" ");
        if (!StateFilter.EffectiveStates.SetEquals(Template.NaturalStates))
        {
            sb.Append("[");
            if (StateFilter.Invert)
                sb.Append("^");

            foreach (var stateFilter in StateFilter.States.OrderBy(state => state.Name))
            {
                sb.Append(stateFilter.Name);
                sb.Append(",");
            }

            if (StateFilter.States.Any())
                sb.Remove(sb.Length - 1, 1);

            sb.Append("]");
            sb.AppendLine();
        }

        sb.Append(Pattern);

        if (Action is ErrorAction errorAction)
        {
            if (string.IsNullOrEmpty(errorAction.Message))
                sb.Append(" -> Error");
            else if (Regex.IsMatch(errorAction.Message, "^[a-zA-Z0-9]*$"))
                sb.Append($" -> Error {errorAction.Message}");
            else
                sb.Append($" -> Error \"{errorAction.Message}\"");

            return sb.ToString();
        }

        if (LineAction != LineAction.Next && RecordAction != RecordAction.NoRecord)
            sb.Append($" -> {LineAction}.{RecordAction}");
        else if (LineAction == LineAction.Next && RecordAction != RecordAction.NoRecord)
            sb.Append($" -> {RecordAction}");
        else if (LineAction != LineAction.Next && RecordAction == RecordAction.NoRecord)
            sb.Append($" -> {LineAction}");

        if (Action is ChangeStateAction changeStateAction)
        {
            if (LineAction == LineAction.Next && RecordAction == RecordAction.NoRecord)
                sb.Append($" -> {changeStateAction.NewState.Name}");
            else
                sb.Append($" {changeStateAction.NewState.Name}");
        }

        return sb.ToString();
    }
}
/*
   Bitvantage.SharpTextFsm
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Bitvantage.SharpTextFsm.Explain;
using Bitvantage.SharpTextFsm.TemplateHelpers;

namespace Bitvantage.SharpTextFsm;

public class TemplateState
{
    private ImmutableArray<Rule> _rules;

    public ImmutableArray<Rule> EffectiveRules { get; private set; }
    public string Name { get; init; }

    public ImmutableArray<Rule> Rules
    {
        get => _rules;
        internal set
        {
            _rules = value;

            switch (Name)
            {
                // state has no rules
                case "End":
                    EffectiveRules = ImmutableArray<Rule>.Empty;
                    break;

                // global rules do not apply to these states; use only the rules defined in the state
                case "EOF":
                case "~Global":
                    EffectiveRules = _rules;
                    break;

                default:
                {
                    // add all global rules with a matching state filter
                    var globalRules = Template
                                          .States
                                          .FirstOrDefault(item => item.Name == "~Global")?
                                          .Rules
                                          .Where(item => item.StateFilter.EffectiveStates.Contains(this))
                                      ?? Enumerable.Empty<Rule>();

                    // add all regular template states
                    EffectiveRules = Enumerable
                        .Empty<Rule>()
                        .Concat(globalRules)
                        .Concat(Rules)
                        .ToImmutableArray();
                    break;
                }
            }
        }
    }

    public Template Template { get; }

    internal TemplateState(Template template, string name)
    {
        Template = template;
        Name = name;
    }

    internal bool TryGetMatch(TemplateFsmContext context, [NotNullWhen(true)] out RuleMatch? ruleMatch)
    {
        for (var ruleIndex = context.CurrentIndex; ruleIndex < EffectiveRules.Length; ruleIndex++)
        {
            var rule = EffectiveRules[ruleIndex];

            var match = rule.Regex.Match(context.Text!);

            if (match.Success)
            {
                ruleMatch = new RuleMatch(rule, ruleIndex, match);
                return true;
            }
        }

        ruleMatch = null;
        return false;
    }

    internal bool TryGetMatch(TemplateFsmContext context, Row currentRow, ExplainResult explainResult, [NotNullWhen(true)] out RuleMatch? ruleMatch)
    {
        for (var ruleIndex = context.CurrentIndex; ruleIndex < EffectiveRules.Length; ruleIndex++)
        {
            var rule = EffectiveRules[ruleIndex];

            var stopwatch = Stopwatch.StartNew();
            var match = rule.Regex.Match(context.Text!);
            stopwatch.Stop();

            var disposition = match.Success switch
            {
                true when rule.Action is ErrorAction => EvaluationDisposition.Error,
                true => EvaluationDisposition.Matched,
                false => EvaluationDisposition.NotMatched
            };

            explainResult.Add(new RuleEvaluation(rule.State, rule, context.Text, context.Line, disposition, currentRow.Clone(), stopwatch.Elapsed));

            if (match.Success)
            {
                ruleMatch = new RuleMatch(rule, ruleIndex, match);
                return true;
            }
        }

        ruleMatch = null;
        return false;
    }

    internal record RuleMatch(Rule Rule, int RuleIndex, Match Match);
}
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

using System.Text;
using Bitvantage.SharpTextFsm.Exceptions;
using Bitvantage.SharpTextFsm.TemplateHelpers;

namespace Bitvantage.SharpTextFsm.Definitions
{
    public class TemplateDefinition
    {
        public List<ValueDefinition> Values { get; set; }
        public List<StateDefinition> States { get; set; }

        public TemplateDefinition(List<ValueDefinition> values, List<StateDefinition> states)
        {
            Values = values;
            States = states;
        }

        public static TemplateDefinition Parse(string textFsmTemplate)
        {
            var valueDescriptors = new List<ValueDefinition>();
            var states = new List<StateDefinition>();

            List<TextFsmTemplateParser> textFsmTemplateRows;
            try
            {
                textFsmTemplateRows = TextFsmTemplateParser.Instance.Run<TextFsmTemplateParser>(textFsmTemplate).ToList();
            }
            catch (TemplateErrorException templateErrorException)
            {
                throw new TemplateParseException($"Syntax error while parsing template in state {templateErrorException.State} on line #{templateErrorException.Line}: {templateErrorException.Text}", ParseError.SyntaxError, templateErrorException);
            }

            foreach (var textFsmTemplateRow in textFsmTemplateRows)
                switch (textFsmTemplateRow.State)
                {
                    case TextFsmTemplateParser.StateState.ValueSection:
                        var option = Option.None;
                        foreach (var valueFlag in textFsmTemplateRow.ValueFlags)
                            option |= valueFlag;

                        var valueDescriptor = new ValueDefinition(textFsmTemplateRow.ValueName, option, textFsmTemplateRow.ValuePattern);

                        valueDescriptors.Add(valueDescriptor);

                        break;

                    case TextFsmTemplateParser.StateState.StateSection:
                        states.Add(new StateDefinition(textFsmTemplateRow.StartName2));
                        break;

                    case TextFsmTemplateParser.StateState.RuleSection:
                        var rule = new RuleDefinition(textFsmTemplateRow.Pattern) { LineAction = textFsmTemplateRow.LineAction ?? LineAction.Next, RecordAction = textFsmTemplateRow.RecordAction ?? RecordAction.NoRecord, StateFilter = textFsmTemplateRow.StateFilters, NegatesStateFilter = textFsmTemplateRow.StateFilterInversion };

                        if (textFsmTemplateRow.ErrorState)
                        {
                            var errorMessage = textFsmTemplateRow.ErrorMessage ?? textFsmTemplateRow.ErrorString;

                            if (errorMessage == null)
                                rule.MatchAction = new ErrorActionDefinition();
                            else
                                rule.MatchAction = new ErrorActionDefinition(errorMessage);
                        }
                        else if (!string.IsNullOrEmpty(textFsmTemplateRow.StateName))
                        {
                            rule.MatchAction = new ChangeStateActionDefinition(textFsmTemplateRow.StateName);
                        }

                        states.Last().Rules.Add(rule);

                        break;

                    case TextFsmTemplateParser.StateState.EndOfFile:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return new TemplateDefinition(valueDescriptors, states);
        }

        public override string ToString()
        {
                var sb = new StringBuilder();

                foreach (var value in Values) 
                    sb.AppendLine(value.ToString());

                if (Values.Any())
                    sb.AppendLine();

                foreach (var state in States)
                {
                    sb.AppendLine($"{state.Name}");

                    foreach (var rule in state.Rules) 
                        sb.AppendLine($"{rule.ToString()}");

                    sb.AppendLine();
                }

                return sb.ToString();
        }
    }
}
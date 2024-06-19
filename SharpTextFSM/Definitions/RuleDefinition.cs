/*
   SharpTextFSM
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
using System.Text.RegularExpressions;
using Bitvantage.SharpTextFSM.TemplateHelpers;

namespace Bitvantage.SharpTextFSM.Definitions
{
    public class RuleDefinition
    {
        public string Pattern { get; }
        public LineAction LineAction { get; set; } = LineAction.Next;
        public RecordAction RecordAction { get; set; } = RecordAction.NoRecord;
        public RuleActionDefinition? MatchAction { get; set; }
        public List<string>? StateFilter { get; set; }
        public bool NegatesStateFilter { get; set; } = false;

        public RuleDefinition(string pattern)
        {
            if (pattern.StartsWith("^"))
                Pattern = pattern;
            else
                Pattern = $"^{pattern}";

            // an action is not required
            // a state filter needs to be here
            // pattern could be an existing regex
            // any state filter needs to reference the existing state...
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (StateFilter != null)
            {
                sb.Append("[");
                if (NegatesStateFilter)
                    sb.Append("^");

                foreach (var stateFilter in StateFilter)
                {
                    sb.Append(stateFilter);
                    sb.Append(",");
                }

                if (StateFilter.Count > 0)
                    sb.Remove(sb.Length - 1, 1);
                
                sb.Append("]");
                
                sb.AppendLine();
            }

            sb.Append(Pattern);

            if (MatchAction is ErrorActionDefinition errorAction)
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

            if (MatchAction is ChangeStateActionDefinition changeStateAction)
            {
                if(LineAction == LineAction.Next && RecordAction == RecordAction.NoRecord)
                    sb.Append($" -> {changeStateAction.StateName}");
                else
                    sb.Append($" {changeStateAction.StateName}");
            }

            return sb.ToString();
        }
    }
}

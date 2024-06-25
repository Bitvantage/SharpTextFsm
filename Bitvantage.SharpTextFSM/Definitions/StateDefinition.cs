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

using System.Text;

namespace Bitvantage.SharpTextFSM.Definitions
{
    public class StateDefinition
    {
        public string Name { get; }
        public List<RuleDefinition> Rules { get; set; } = new();

        public StateDefinition(string name)
        {
            Name = name;
        }

        public StateDefinition(string name, List<RuleDefinition> rules)
        {
            Name = name;
            Rules = rules;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(Name);

            foreach (var rule in Rules) 
                sb.AppendLine(rule.ToString());

            sb.AppendLine();

            return sb.ToString();
        }

    }
}

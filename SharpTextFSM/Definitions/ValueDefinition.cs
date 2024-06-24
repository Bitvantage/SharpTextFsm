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

using Bitvantage.SharpTextFSM.TemplateHelpers;

namespace Bitvantage.SharpTextFSM.Definitions
{
    public class ValueDefinition
    {
        public string Name { get; }
        public Option Options { get; }
        public string Pattern { get; }

        public ValueDefinition(string name, Option options, string pattern)
        {
            Name = name;
            Options = options;
            Pattern = pattern;
        }

        public override string ToString()
        {
            if (Options == Option.None)
                return ($"Value {Name} ({Pattern})");

            return ($"Value {Options.ToString().Replace(" ", "")} {Name} ({Pattern})");
               
        }
    }
}

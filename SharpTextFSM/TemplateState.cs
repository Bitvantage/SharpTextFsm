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

using System.Collections.Immutable;
using Bitvantage.SharpTextFSM.TemplateHelpers;

namespace Bitvantage.SharpTextFSM
{
    public class TemplateState
    {
        public ImmutableArray<Rule> Rules { get; internal set; }
        public Template Template { get; }
        public string Name { get; init; }
        internal TemplateState(Template template, string name)
        {
            Template = template;
            Name = name;
        }

        // TODO: it might be possible to combine all the rules into a single regular expression, with the rule params encoded into the capture group
    }
}

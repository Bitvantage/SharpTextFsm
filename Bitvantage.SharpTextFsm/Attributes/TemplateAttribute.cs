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

using System.ComponentModel;

namespace Bitvantage.SharpTextFsm.Attributes
{
    [Flags]
    public enum MappingStrategy
    {
        [Description("An explicit mapping must exist")]
        Disabled = 1 << 0,

        [Description("The name must match exactly")]
        Exact = 1 << 1,

        [Description("Ignore the case of variable names")]
        IgnoreCase = 1 << 2,

        [Description("Ignore '_' in variable names")]
        SnakeCase = 1 << 3,

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TemplateAttribute : Attribute
    {
        public MappingStrategy MappingStrategies { get; }

        public TemplateAttribute(MappingStrategy mappingStrategies)
        {
            MappingStrategies = mappingStrategies;
        }
    }
}
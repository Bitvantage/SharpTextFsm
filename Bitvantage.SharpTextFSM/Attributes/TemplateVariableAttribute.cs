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

namespace Bitvantage.SharpTextFsm.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TemplateVariableAttribute : Attribute
{
    public enum TrimType
    {
        None,
        Trim,
        TrimStart,
        TrimEnd,
    }
    internal static TemplateVariableAttribute Default { get; } = new();

    public Type? Converter { get; set; }
    public string? DefaultValue { get; set; }
    public bool Ignore { get; set; } = false;
    public Type? ListConverter { get; set; }
    public string? Name { get; set; }
    public bool SkipEmpty { get; set; } = true;
    public bool ThrowOnConversionFailure { get; set; } = true;
    public TrimType Trim { get; set; } = TrimType.None;
}
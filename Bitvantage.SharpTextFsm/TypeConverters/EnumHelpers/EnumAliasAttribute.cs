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

using System.Diagnostics.CodeAnalysis;

namespace Bitvantage.SharpTextFsm.TypeConverters.EnumHelpers;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class EnumAliasAttribute : Attribute
{
    /// <summary>
    /// Specifies the default value for the <see cref='EnumAliasAttribute'/>,
    /// which is an empty string (""). This <see langword='static'/> field is read-only.
    /// </summary>
    public static readonly EnumAliasAttribute Default = new();

    public EnumAliasAttribute() : this(string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref='EnumAliasAttribute'/> class.
    /// </summary>
    public EnumAliasAttribute(string mapping)
    {
        MappingValue = mapping;
    }

    /// <summary>
    /// Gets the description stored in this attribute.
    /// </summary>
    public virtual string Mapping => MappingValue;

    /// <summary>
    /// Read/Write property that directly modifies the string stored in the description
    /// attribute. The default implementation of the <see cref="Mapping"/> property
    /// simply returns this value.
    /// </summary>
    protected string MappingValue { get; set; }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is EnumAliasAttribute other && other.Mapping == Mapping;

    public override int GetHashCode() => Mapping?.GetHashCode() ?? 0;

    public override bool IsDefaultAttribute() => Equals(Default);
}
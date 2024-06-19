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

using System.Collections.ObjectModel;
using System.Reflection;

namespace Bitvantage.SharpTextFSM.TypeConverters.EnumHelpers;

internal static class EnumMap<T> where T : Enum
{
    private static readonly ReadOnlyDictionary<string, T> ValueMap;

    static EnumMap()
    {
        var enumMembers = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        var valueLookup = typeof(T)
            .GetEnumValues()
            .Cast<T>()
            .ToDictionary(@enum => @enum.ToString(), @enum => @enum);

        var members = enumMembers
            .Select(enumMember => new
            {
                enumMember.Name,
                Instance = valueLookup[enumMember.Name],
                Mappings = enumMember.GetCustomAttributes<EnumAliasAttribute>()?.Select(item => item.Mapping).ToList()
            })
            .ToList();

        var valueMap = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);

        // raw member names
        foreach (var member in members)
            valueMap.Add(member.Name, member.Instance);

        // aliases learned from mappings attributes
        foreach (var member in members.Where(member => member.Mappings != null))
            foreach (var mapping in member.Mappings)
                valueMap.TryAdd(mapping, member.Instance);

        ValueMap = valueMap.AsReadOnly();

   }

    public static T Parse(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (TryParse(value, out T member))
            return member;

        throw new KeyNotFoundException();
    }

    public static bool TryParse(string value, out T member)
    {
        if (ValueMap.TryGetValue(value, out member))
            return true;

        return false;
    }
}
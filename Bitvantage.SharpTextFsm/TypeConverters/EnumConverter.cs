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

using Bitvantage.SharpTextFsm.TypeConverters.EnumHelpers;

namespace Bitvantage.SharpTextFsm.TypeConverters
{
    public class EnumConverter<TItem> : ValueConverter<TItem> where TItem : Enum
    {
        public override bool TryConvert(string value, out TItem convertedValue)
        {
            var success = EnumMap<TItem>.TryParse(value, out var enumValue);
            convertedValue = enumValue;

            return success;
        }

        public static bool CanConvert()
        {
            return typeof(TItem).IsEnum;
        }
    }
}

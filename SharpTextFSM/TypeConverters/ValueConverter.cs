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

using System.Diagnostics.CodeAnalysis;

namespace Bitvantage.SharpTextFSM.TypeConverters
{
    public abstract class ValueConverter
    {
        internal abstract bool TryConvert(string value, [NotNullWhen(true)] out object? convertedValue);
    }

    public abstract class ValueConverter<TItem> : ValueConverter
    {
        public abstract bool TryConvert(string value, out TItem convertedValue);

        internal override bool TryConvert(string value, [NotNullWhen(true)]out object? convertedValue)
        {
            var success = TryConvert(value, out TItem result);
            convertedValue = result;
            
            return success;
        }
    }

}

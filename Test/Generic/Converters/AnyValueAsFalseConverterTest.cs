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

using Bitvantage.SharpTextFsm.TypeConverters;

namespace Test.Generic.Converters
{
    internal class AnyValueAsFalseConverterTest
    {
        [Test]
        public void Test01()
        {
            var success = new AnyValueAsFalseConverter().TryConvert("xyz", out var value);

            Assert.That(success, Is.True);
            Assert.That(value, Is.False);
        }

        [Test]
        public void Test02()
        {
            var success = new AnyValueAsFalseConverter().TryConvert("", out var value);

            Assert.That(success, Is.True);
            Assert.That(value, Is.True);
        }

        [Test]
        public void Test03()
        {
            var success = new AnyValueAsFalseConverter().TryConvert(null, out var value);

            Assert.That(success, Is.True);
            Assert.That(value, Is.True);
        }

    }
}

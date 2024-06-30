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
    internal class TerseTimeSpanConverterTest
    {
        [Test]
        public void Test01()
        {
            var success = new TerseTimeSpanConverter().TryConvert("3w1d5h", out var value);

            Assert.That(success, Is.True);

            Assert.That(value.ToString(), Is.EqualTo("22.05:00:00"));
        }


        [Test]
        public void Test02()
        {
            var success = new TerseTimeSpanConverter().TryConvert("99y51w6d23h59m59s", out var value);

            Assert.That(success, Is.True);

            Assert.That(value.ToString(), Is.EqualTo("36498.23:59:59"));
        }

        [Test]
        public void Test03()
        {
            var success = new TerseTimeSpanConverter().TryConvert("51w6d23h59m59s", out var value);

            Assert.That(success, Is.True);

            Assert.That(value.ToString(), Is.EqualTo("363.23:59:59"));
        }

        [Test(Description = "Non sequential units")]
        public void Test04()
        {
            var success = new TerseTimeSpanConverter().TryConvert("1y3w10s", out var value);

            Assert.That(success, Is.False);
        }

        [Test(Description = "Non sequential units")]
        public void Test05()
        {
            var success = new TerseTimeSpanConverter().TryConvert("3w10s", out var value);

            Assert.That(success, Is.False);
        }
    }
}

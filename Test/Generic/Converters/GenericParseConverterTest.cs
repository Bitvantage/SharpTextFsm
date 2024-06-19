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

using System.Net;
using Bitvantage.SharpTextFSM.TypeConverters;

namespace Test.Generic.Converters
{
    internal class GenericParseConverterTest
    {
        [Test]
        public void Long()
        {
            var converter = new GenericParseConverter<long>();

            long convertedValue;
            Assert.That(converter.TryConvert("100", out convertedValue), Is.True);
            Assert.That(convertedValue, Is.TypeOf<long>());
            Assert.That(convertedValue, Is.EqualTo((long)100));

            Assert.That(converter.TryConvert("x", out convertedValue), Is.False);
            Assert.That(convertedValue, Is.TypeOf<long>());
            Assert.That(convertedValue, Is.EqualTo((long)0));

            Assert.That(converter.TryConvert(null, out convertedValue), Is.False);
            Assert.That(convertedValue, Is.TypeOf<long>());
            Assert.That(convertedValue, Is.EqualTo((long)0));
        }

        [Test]
        public void NullableLong()
        {
            var converter = new GenericParseConverter<long?>();

            long? convertedValue;
            Assert.That(converter.TryConvert("100", out convertedValue), Is.True);
            Assert.That(convertedValue, Is.TypeOf<long>());
            Assert.That(convertedValue, Is.EqualTo((long?)100));

            Assert.That(converter.TryConvert("x", out convertedValue), Is.False);
            Assert.That(convertedValue, Is.Null);

            Assert.That(converter.TryConvert(null, out convertedValue), Is.False);
            Assert.That(convertedValue, Is.Null);
        }

        [Test]
        public void IpAddress()
        {
            var converter = new GenericParseConverter<IPAddress>();

            IPAddress? convertedValue;
            Assert.That(converter.TryConvert("1.2.3.4", out convertedValue), Is.True);
            Assert.That(convertedValue, Is.TypeOf<IPAddress>());
            Assert.That(convertedValue, Is.EqualTo(IPAddress.Parse("1.2.3.4")));

            Assert.That(converter.TryConvert("x", out convertedValue), Is.False);
            Assert.That(convertedValue, Is.Null);

            Assert.That(converter.TryConvert(null, out convertedValue), Is.False);
            Assert.That(convertedValue, Is.Null);
        }

        [Test]
        public void TimeSpan()
        {
            var converter = new GenericParseConverter<TimeSpan>();
            TimeSpan convertedValue;

            Assert.That(converter.TryConvert("01.00:00:00", out convertedValue), Is.True);
            Assert.That(convertedValue, Is.TypeOf<TimeSpan>());
            Assert.That(convertedValue, Is.EqualTo(System.TimeSpan.FromDays(1)));

            Assert.That(converter.TryConvert("x", out convertedValue), Is.False);
            Assert.That(convertedValue, Is.EqualTo(System.TimeSpan.Zero));

            Assert.That(converter.TryConvert(null, out convertedValue), Is.False);
            Assert.That(convertedValue, Is.EqualTo(System.TimeSpan.Zero));
        }

        [Test]
        public void NullableTimeSpan()
        {
            var converter = new GenericParseConverter<TimeSpan?>();
            TimeSpan? convertedValue = null;

            Assert.That(converter.TryConvert("01.00:00:00", out convertedValue), Is.True);
            Assert.That(convertedValue, Is.TypeOf<TimeSpan>());
            Assert.That(convertedValue, Is.EqualTo(System.TimeSpan.FromDays(1)));

            Assert.That(converter.TryConvert("x", out convertedValue), Is.False);
            Assert.That(convertedValue, Is.Null);

            Assert.That(converter.TryConvert(null, out convertedValue), Is.False);
            Assert.That(convertedValue, Is.Null);
        }
    }
}

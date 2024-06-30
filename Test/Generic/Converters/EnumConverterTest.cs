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
using Bitvantage.SharpTextFsm.TypeConverters.EnumHelpers;

namespace Test.Generic.Converters
{
    internal class EnumConverterTest
    {
        enum Animal
        {
            None,
            Armadillo,
            Blobfish,
            Capybara,
            Fossa,
            [EnumAlias("Ghost Shark")]
            GhostShark,
            [EnumAlias("Goblin Shark")]
            GoblinShark,
            Hagfish,
            Hoatzin,
            Pangolin,
            Platypus,
            [EnumAlias("Sea Hog")]
            [EnumAlias("Sea Pig")]
            [EnumAlias("Sea Piggy")]
            [EnumAlias("Sea Swine")]
            SeaPig,
            Sloth,
            Tarsier,
            Uakari,
        }

        [Test]
        public void ExactNameMatch01()
        {
            var converter = new EnumConverter<Animal>();

            Animal convertedValue;
            Assert.That(converter.TryConvert("Platypus", out convertedValue), Is.True);
            Assert.That(convertedValue, Is.TypeOf<Animal>());
            Assert.That(convertedValue, Is.EqualTo(Animal.Platypus));
        }

        [Test]
        public void LowerCaseMatch01()
        {
            var converter = new EnumConverter<Animal>();

            Animal convertedValue;
            Assert.That(converter.TryConvert("platypus", out convertedValue), Is.True);
            Assert.That(convertedValue, Is.TypeOf<Animal>());
            Assert.That(convertedValue, Is.EqualTo(Animal.Platypus));
        }

        [Test]
        public void EnumMappingTest01()
        {
            var converter = new EnumConverter<Animal>();

            Animal convertedValue;
            Assert.That(converter.TryConvert("SeaPig", out convertedValue), Is.True);
            Assert.That(convertedValue, Is.TypeOf<Animal>());
            Assert.That(convertedValue, Is.EqualTo(Animal.SeaPig));

            Assert.That(converter.TryConvert("Sea Hog", out convertedValue), Is.True);
            Assert.That(convertedValue, Is.TypeOf<Animal>());
            Assert.That(convertedValue, Is.EqualTo(Animal.SeaPig));

            Assert.That(converter.TryConvert("Sea Piggy", out convertedValue), Is.True);
            Assert.That(convertedValue, Is.TypeOf<Animal>());
            Assert.That(convertedValue, Is.EqualTo(Animal.SeaPig));

            Assert.That(converter.TryConvert("sea swine", out convertedValue), Is.True);
            Assert.That(convertedValue, Is.TypeOf<Animal>());
            Assert.That(convertedValue, Is.EqualTo(Animal.SeaPig));
        }

        [Test]
        public void NoMatch01()
        {
            var converter = new EnumConverter<Animal>();

            Animal convertedValue;
            Assert.That(converter.TryConvert("SeaDog", out convertedValue), Is.False);
            Assert.That(convertedValue, Is.TypeOf<Animal>());
            Assert.That(convertedValue, Is.EqualTo(Animal.None));
        }
    }
}

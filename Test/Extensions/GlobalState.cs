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

using Bitvantage.SharpTextFsm;

namespace Test.Extensions
{
    internal class GlobalState
    {
        [Test]
        public void Global01()
        {
            var template = new Template("""
                Value NUMBER1 (\d)
                Value NUMBER2 (\d)
                Value NUMBER3 (\d)

                ~Global
                 ^Section \d+ -> Continue.Record Section
                 ^ Sub-Section \d+ -> Continue SubSection
                 ^  Sub-Sub-Section \d+ -> Continue SubSubSection

                Start
                 ^. -> Error

                Section
                 ^Section ${NUMBER1}
                 ^. -> Error
                 
                SubSection
                 ^\s*Sub-Section ${NUMBER2}
                 ^. -> Error
                 
                SubSubSection
                 ^\s*Sub-Sub-Section ${NUMBER3}
                 ^. -> Error

                """);

            var data = """
                Section 1
                 Sub-Section 2
                  Sub-Sub-Section 3
                Section 4
                 Sub-Section 5
                  Sub-Sub-Section 6
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0]["NUMBER1"], Is.EqualTo("1"));
            Assert.That(result[0]["NUMBER2"], Is.EqualTo("2"));
            Assert.That(result[0]["NUMBER3"], Is.EqualTo("3"));

            Assert.That(result[1]["NUMBER1"], Is.EqualTo("4"));
            Assert.That(result[1]["NUMBER2"], Is.EqualTo("5"));
            Assert.That(result[1]["NUMBER3"], Is.EqualTo("6"));
        }
    }
}

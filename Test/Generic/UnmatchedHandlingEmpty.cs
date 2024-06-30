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

namespace Test.Generic
{
    internal class UnmatchedHandlingEmpty : ITemplate
    {
        public string X { get; set; }
        public string Y { get; set; }

        string ITemplate.TextFsmTemplate =>
            """
            Value X (\d+)
            Value Y (\d+)

            Start
             ^P${X} -> Record
             ^F${Y} -> Record
            """;

        [Test]
        public void Test()
        {
            var template = Template.FromType<UnmatchedHandlingEmpty>(new TemplateOptions(UnmatchedHandling.Empty));
            var data = """
                P100
                F200
                """;

            var results = template.Parse<UnmatchedHandlingEmpty>(data).ToList();

            Assert.That(results.Count, Is.EqualTo(2));

            Assert.That(results[0].X, Is.EqualTo("100"));
            Assert.That(results[0].Y, Is.EqualTo(string.Empty));

            Assert.That(results[1].X, Is.EqualTo(string.Empty));
            Assert.That(results[1].Y, Is.EqualTo("200"));
        }
    }
}

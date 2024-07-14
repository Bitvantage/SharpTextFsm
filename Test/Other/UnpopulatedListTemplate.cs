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
using Bitvantage.SharpTextFsm.Attributes;

namespace Test.Other
{
    public record UnpopulatedListTemplate : ITemplate
    {
        string ITemplate.TextFsmTemplate => """
            Value NUMBER (\d+)
            Value List LETTERS ([a-zA-Z])
            Value List EMPTY_LIST (DOES NOT MATCH ANYTHING)

            Start
             ^${LETTERS}
             ^${NUMBER} -> Record
             ^. -> Error
            """;

        private readonly string _data = """
            A
            B
            C
            1
            X
            Y
            Z
            2
            """;

        [Variable(Name = "LETTERS")]
        public List<string> Letters { get; set; }

        [Variable(Name = "EMPTY_LIST")]
        public List<string> EmptyList { get; set; }

        [Variable(Name = "NUMBER")]
        public int Number { get; set; }

        [Test(Description = "Unpopulated lists are empty and not null")]
        public void ListTest01()
        {
            /*
             * FSM Table:
             * ['NUMBER', 'LETTERS', 'EMPTY_LIST']
             * ['1', ['A', 'B', 'C'], []]
             * ['2', ['X', 'Y', 'Z'], []]
             *
             */

            var genericTemplate = Template.FromType<UnpopulatedListTemplate>();

            var template = new Template(((ITemplate)new UnpopulatedListTemplate()).TextFsmTemplate);

            var result = template.Parse(_data).ToDynamic().ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].NUMBER, Is.EqualTo("1"));
            Assert.That(result[0].LETTERS, Is.EqualTo(new[] { "A", "B", "C" }));
            Assert.That(result[0].EMPTY_LIST, Is.Empty);

            Assert.That(result[1].NUMBER, Is.EqualTo("2"));
            Assert.That(result[1].LETTERS, Is.EqualTo(new[] { "X", "Y", "Z" }));
            Assert.That(result[1].EMPTY_LIST, Is.Empty);

            var genericResult = genericTemplate.Parse<UnpopulatedListTemplate>(_data).ToList();

            Assert.That(genericResult.Count, Is.EqualTo(2));
            Assert.That(genericResult[0].Number, Is.EqualTo(1));
            Assert.That(genericResult[0].Letters, Is.EqualTo(new[] { "A", "B", "C" }));
            Assert.That(genericResult[0].EmptyList, Is.Empty);

            Assert.That(genericResult[1].Number, Is.EqualTo(2));
            Assert.That(genericResult[1].Letters, Is.EqualTo(new[] { "X", "Y", "Z" }));
            Assert.That(genericResult[1].EmptyList, Is.Empty);

        }
    }
}

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
    internal class MetadataValueFlag
    {
        [Test]
        public void Metadata01()
        {
            var template = new Template("""
                Value boo (one)
                Value hoo (two)
                Value Metadata LINE_NUMBER (Line)
                Value Metadata TEXT (Text)
                Value Metadata CURRENT_STATE (State)
                Value Metadata RULE_INDEX (RuleIndex)

                Start
                 ^${boo}.* -> Record State1
                 ^${hoo}.* -> Record State1
                
                State1
                 ^${boo}.* -> Record
                 ^${hoo}.* -> Record
                """);

            var data = """
                one A
                two B
                one C
                two D
                one E
                two F
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(6));

            Assert.That(result[0]["boo"], Is.EqualTo("one"));
            Assert.That(result[0]["hoo"], Is.EqualTo(string.Empty));
            Assert.That(result[0]["LINE_NUMBER"], Is.EqualTo("1"));
            Assert.That(result[0]["TEXT"], Is.EqualTo("one A"));
            Assert.That(result[0]["CURRENT_STATE"], Is.EqualTo("Start"));
            Assert.That(result[0]["RULE_INDEX"], Is.EqualTo("0"));

            Assert.That(result[1]["boo"], Is.EqualTo(string.Empty));
            Assert.That(result[1]["hoo"], Is.EqualTo("two"));
            Assert.That(result[1]["LINE_NUMBER"], Is.EqualTo("2"));
            Assert.That(result[1]["TEXT"], Is.EqualTo("two B"));
            Assert.That(result[1]["CURRENT_STATE"], Is.EqualTo("State1"));
            Assert.That(result[1]["RULE_INDEX"], Is.EqualTo("1"));

            Assert.That(result[2]["boo"], Is.EqualTo("one"));
            Assert.That(result[2]["hoo"], Is.EqualTo(string.Empty));
            Assert.That(result[2]["LINE_NUMBER"], Is.EqualTo("3"));
            Assert.That(result[2]["TEXT"], Is.EqualTo("one C"));
            Assert.That(result[2]["CURRENT_STATE"], Is.EqualTo("State1"));
            Assert.That(result[2]["RULE_INDEX"], Is.EqualTo("0"));

            Assert.That(result[3]["boo"], Is.EqualTo(string.Empty));
            Assert.That(result[3]["hoo"], Is.EqualTo("two"));
            Assert.That(result[3]["LINE_NUMBER"], Is.EqualTo("4"));
            Assert.That(result[3]["TEXT"], Is.EqualTo("two D"));
            Assert.That(result[3]["CURRENT_STATE"], Is.EqualTo("State1"));
            Assert.That(result[3]["RULE_INDEX"], Is.EqualTo("1"));
        }
    }
}

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

using Bitvantage.SharpTextFSM;
using Bitvantage.SharpTextFSM.Exceptions;

namespace Test.Extensions
{
    internal class RegexValueFlag
    {
        [Test(Description = "Basic test of Regex flag")]
        public void Test01()
        {
            var template = new Template("""
                Value X (.*)
                Value Regex NUMBER (\d+)

                Start
                 ^${NUMBER} -> Record
                 ^.* -> Error
                """);

            var data = """
                10000
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test(Description = "Regex flag in Value")]
        public void Test02()
        {
            var template = new Template("""
                Value Regex NUMBER (\d+)
                Value X (${NUMBER})

                Start
                 ^${X} -> Record
                 ^.* -> Error
                """);

            var data = """
                    10000
                    """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0]["X"], Is.EqualTo("10000"));
        }

        [Test(Description = "Regex flag in rule")]
        public void Test03()
        {
            var template = new Template("""
                Value Regex NUMBER (123)
                Value X (45)

                Start
                 ^${NUMBER}${X} -> Record
                 ^.* -> Error
                """);

            var data = """
                12345
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0]["X"], Is.EqualTo("45"));
        }

        [Test(Description = "Regex value in a regex value in a Value")]
        public void Test04()
        {
            var template = new Template("""
                Value Regex NUMBER1 (123)
                Value Regex NUMBER2 (${NUMBER1}456)
                Value X (${NUMBER2})

                Start
                 ^${X} -> Record
                 ^.* -> Error
                """);

            var data = """
                123456
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0]["X"], Is.EqualTo("123456"));
        }

        [Test(Description = "Builtin pattern in Value")]
        public void Test05()
        {
            var template = new Template("""
                Value X (${_POSITIVE_INTEGER})

                Start
                 ^${X} -> Record
                 ^.* -> Error
                """);

            var data = """
                123456
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0]["X"], Is.EqualTo("123456"));
        }

        [Test(Description = "Builtin pattern in Value and Rule")]
        public void Test06()
        {
            var template = new Template("""
                Value X (${_IP})

                Start
                 ^${X} ${_IP} -> Record
                 ^.* -> Error
                """);

            var data = """
                10.0.0.1 192.160.0.1
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0]["X"], Is.EqualTo("10.0.0.1"));
        }

        [Test(Description = "Builtin name overlap")]
        public void Test07()
        {
            var template = new Template("""
                Value _IP (XXX)

                Start
                 ^${_IP} -> Record
                 ^.* -> Error
                """);

            var data = """
                XXX
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0]["_IP"], Is.EqualTo("XXX"));
        }

        [Test(Description = "Builtin name overlap with duplicate")]
        public void Test08()
        {
            var exception = Assert.Throws<TemplateParseException>(()=>new Template("""
                Value _IP (XXX)
                Value _IP (AAA)

                Start
                 ^${_IP} -> Record
                 ^.* -> Error
                """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.DuplicateValueName));
        }
    }
}

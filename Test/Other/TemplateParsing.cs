/*
   Bitvantage.SharpTextFSM
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
using Bitvantage.SharpTextFSM.Definitions;
using Bitvantage.SharpTextFSM.Exceptions;
using Bitvantage.SharpTextFSM.TemplateHelpers;
using Test.Helpers;

namespace Test.Other
{
    internal class TemplateParsing
    {
        [Test(Description = "No state filters in regular states")]
        public void Test01()
        {
            var templateText = """
            # Comment 1
            # Comment 2
            Value NUMBER1 (2)
            Value List,Filldown,Fillup,Key,Metadata,Required NUMBER2 (2)
            #Comment 2
            Start
             ^Test -> Start1
             ^Test -> Next.Record Start1
             ^Test -> Record Start1
             ^Test -> Next Start1
             ^Test ->
             ^Test
             
            Start1
             ^Test -> Error
             ^Test -> Error Message
             ^Test -> Error "Message Test"
             ^Test
             [Start1,Start]
             ^Test
            """;

            var exception = Assert.Throws<TemplateParseException>(() => new Template(templateText));
            Assert.That(exception.ErrorCode == ParseError.StateFilterInRegularRule);
        }

        [Test]
        public void Test02()
        {
            var templateText = """
                # Comment 1
                # Comment 2
                Value NUMBER1 (2)
                Value List,Filldown,Fillup,Key,Metadata,Required NUMBER2 (2)
                #Comment 2
                Start
                 ^Test -> Start1
                 ^Test -> Next.Record Start1
                 ^Test -> Record Start1
                 ^Test -> Next Start1
                 ^Test ->
                 ^Test
                 
                Start1
                 ^Test -> Error
                 ^Test -> Error Message
                 ^Test -> Error "Message Test"
                 ^Test
                 [Start1,Start]
                 ^Test
                 [^Start1,Start]
                 ^Test
                """;

            var t = TemplateDefinition.Parse(templateText);
            var tt = t.ToString();

            var t1 = new TemplateDefinition(
                new List<ValueDefinition>
                {
                    new ValueDefinition("Test1", Option.List, "xxx")
                },
                new List<StateDefinition>
                {
                    new StateDefinition("Start", new List<RuleDefinition>
                    {
                        new RuleDefinition("Test")
                    })
                }
                );

            var t1c = new Template(t1);

            var t2 = new TemplateDefinition(
                new List<ValueDefinition>
                {
                    new ValueDefinition("Test1", Option.List, "xxx")
                },
                new List<StateDefinition>
                {
                    new StateDefinition("Start", new List<RuleDefinition>
                    {
                        new RuleDefinition("Test")
                    })
                }
            );

            var t2c = new Template(t2);

            Assert.That(t1.ToString(), Is.EqualTo(t2.ToString()));

            Assert.That(ReferenceEquals(t1c.GetFieldValue<Template>("_template"), t2c.GetFieldValue<Template>("_template")), Is.True);


        }
    }
}
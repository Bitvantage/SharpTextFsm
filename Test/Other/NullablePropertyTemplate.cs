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
using Bitvantage.SharpTextFSM.Attributes;

namespace Test.Other
{
    public record NullablePropertyTemplate : ITemplate
    {
        string ITemplate.TextFsmTemplate => """
            Value A_NUMBER_MAYBE (\d+)
            Value A_LETTER_ALWAYS (\w+)

            Start
             ^Y${A_NUMBER_MAYBE}?${A_LETTER_ALWAYS}
             ^Y${A_LETTER_ALWAYS}
             ^. -> Error
            """;

        [TemplateVariable(Name = "A_NUMBER_MAYBE", ThrowOnConversionFailure = false)]
        public int? Number { get; set; }

        [TemplateVariable(Name = "A_LETTER_ALWAYS")]
        public string Letter { get; set; }


        private readonly string _data = """
            Y9b
            Yb
            """;

        [Test(Description = "A nullable value that matches an empty string returns null")]
        public void NullableInt()
        {
            /*
             *  FSM Table:
             *  ['A_NUMBER_MAYBE', 'A_LETTER_ALWAYS']
             *  ['', 'b']
             *
             */

            var genericTemplate = Template.FromType<NullablePropertyTemplate>();

            var template = new Template(((ITemplate)new NullablePropertyTemplate()).TextFsmTemplate);

            var result = template.Parse(_data).ToDynamic().ToList();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].A_NUMBER_MAYBE, Is.Empty);
            Assert.That(result[0].A_LETTER_ALWAYS, Is.EqualTo("b"));

            var genericResult = genericTemplate.Parse<NullablePropertyTemplate>(_data).ToList();
            Assert.That(genericResult.Count, Is.EqualTo(1));
            Assert.That(genericResult[0].Number, Is.Null);
            Assert.That(genericResult[0].Letter, Is.EqualTo("b"));
        }
    }
}
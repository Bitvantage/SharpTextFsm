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

namespace Test.Generic
{
    internal class Translations : ITemplate
    {
        [TemplateVariable(Name = "ValueProperty")]
        [TemplateTranslation("100", "1000")]
        [TemplateTranslation("101", "")]
        public long? ValueProperty { get; set; }
        [TemplateTranslation("200", "2000")] 
        [TemplateTranslation("201", null)] 
        public long? ValueField { get; set; }

        string ITemplate.TextFsmTemplate =>
            """
            Value ValueProperty (\d+)
            Value ValueField (\d+)

            Start
             ^P${ValueProperty}
             ^F${ValueField} -> Record
            """;

        [Test]
        public void Test()
        {
            var template = Template.FromType<Translations>();
            var data = """
                P100
                F200
                P101
                F201
                P102
                F202
                """;

            var results = template.Parse<Translations>(data).ToList();

            Assert.That(results.Count, Is.EqualTo(3));

            Assert.That(results[0].ValueProperty, Is.EqualTo(1000));
            Assert.That(results[0].ValueField, Is.EqualTo(2000));

            Assert.That(results[1].ValueProperty, Is.Null);
            Assert.That(results[1].ValueField, Is.Null);

            Assert.That(results[2].ValueProperty, Is.EqualTo(102));
            Assert.That(results[2].ValueField, Is.EqualTo(202));
        }
    }


}
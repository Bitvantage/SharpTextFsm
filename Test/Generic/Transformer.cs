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
    internal class Transformer : ITemplate
    {
        [TemplateVariable(Name = "ValueProperty")]
        [TemplateValueTransformer("204", "100", MatchMode.Literal, MatchMethod.Full, MatchDisposition.Continue)]
        [TemplateValueTransformer("100", "1000")]
        [TemplateValueTransformer("101", "")]
        [TemplateValueTransformer(@"^1\d3$", "999", MatchMode.Regex)]
        public long? ValueProperty { get; set; }
        [TemplateValueTransformer("200", "2000")] 
        [TemplateValueTransformer("201", null)] 
        [TemplateValueTransformer("201", null)]
        [TemplateValueTransformer(@"^2\d3", "999", MatchMode.Regex, MatchMethod.Substring)]
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
            var template = Template.FromType<Transformer>();
            var data = """
                P100
                F200
                P101
                F201
                P102
                F202
                P103
                F203888
                P204
                """;

            var results = template.Parse<Transformer>(data).ToList();

            Assert.That(results.Count, Is.EqualTo(5));

            Assert.That(results[0].ValueProperty, Is.EqualTo(1000));
            Assert.That(results[0].ValueField, Is.EqualTo(2000));

            Assert.That(results[1].ValueProperty, Is.Null);
            Assert.That(results[1].ValueField, Is.Null);

            Assert.That(results[2].ValueProperty, Is.EqualTo(102));
            Assert.That(results[2].ValueField, Is.EqualTo(202));

            Assert.That(results[3].ValueProperty, Is.EqualTo(999));
            Assert.That(results[3].ValueField, Is.EqualTo(999888));

            Assert.That(results[4].ValueProperty, Is.EqualTo(1000));
            Assert.That(results[4].ValueField, Is.Null);

        }
    }


}
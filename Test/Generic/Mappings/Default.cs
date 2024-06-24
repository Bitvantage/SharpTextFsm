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

namespace Test.Generic.Mappings
{
    internal class Default : ITemplate
    {
        [TemplateVariable(ThrowOnConversionFailure = false, DefaultValue = "50")]
        public long ValueProperty { get; set; }

        [TemplateVariable(ThrowOnConversionFailure = false, DefaultValue = "75")] 
        public long ValueField { get; set; }

        string ITemplate.TextFsmTemplate =>
            """
            Value ValueProperty (.+)
            Value ValueField (.+)

            Start
             ^P${ValueProperty}
             ^F${ValueField}
            """;

        [Test]
        public void Test()
        {
            var template = Template.FromType<Default>();
            var data = """
                P100X
                F200X
                """;

            var results = template.Parse<Default>(data).ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].ValueProperty, Is.EqualTo(50));
            Assert.That(results[0].ValueField, Is.EqualTo(75));
        }
    }


}

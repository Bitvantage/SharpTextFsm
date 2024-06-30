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
using Bitvantage.SharpTextFsm.TemplateHelpers;

namespace Test.Generic
{
    internal class Validator : ITemplate, ITemplateValidator
    {
        public long ValueProperty { get; set; }
        public long ValueField { get; set; }
        public long UnboundProperty { get; set; }

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
            var template = Template.FromType<Validator>();
            var data = """
                P100
                F200
                P300
                F400
                P500
                F600
                """;

            var results = template.Parse<Validator>(data).ToList();

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results[0].ValueProperty, Is.EqualTo(100));
            Assert.That(results[0].ValueField, Is.EqualTo(200));
            Assert.That(results[0].UnboundProperty, Is.EqualTo(20000));

            Assert.That(results[1].ValueProperty, Is.EqualTo(50));
            Assert.That(results[1].ValueField, Is.EqualTo(600));
            Assert.That(results[1].UnboundProperty, Is.EqualTo(60000));
        }

        bool ITemplateValidator.Validate(Row row)
        {
            UnboundProperty = int.Parse((string)row["ValueField"]) * 100;

            if (ValueProperty == 300)
                return false;

            if (ValueProperty == 500)
                ValueProperty = 50;
            
            return true;
        }
    }
}

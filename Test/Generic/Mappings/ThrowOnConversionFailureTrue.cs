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
using Bitvantage.SharpTextFsm.Exceptions;

namespace Test.Generic.Mappings
{
    internal class ThrowOnConversionFailureTrue : ITemplate
    {
        [TemplateVariable(ThrowOnConversionFailure = true)]
        public long ValueProperty { get; set; }

        [TemplateVariable(ThrowOnConversionFailure = true)]
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
            var template = Template.FromType<ThrowOnConversionFailureTrue>();
            var data = """
                P100X
                F200Y
                """;

            Assert.Throws<TemplateTypeConversionException>(() => template.Parse<ThrowOnConversionFailureTrue>(data).ToList());
        }
    }


}

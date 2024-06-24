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
using Bitvantage.SharpTextFSM.Exceptions;

namespace Test.Generic.Mappings
{
    internal class EmptyTemplateVariableAttributeNoMatch : ITemplate
    {
        [TemplateVariable]
        public long ValueProperty { get; set; }
        [TemplateVariable]

        public long ValueField { get; set; }

        string ITemplate.TextFsmTemplate =>
            """
            Value ValuePropertyX (\d+)
            Value ValueFieldX (\d+)

            Start
             ^P${ValuePropertyX}
             ^F${ValueFieldX}
            """;

        [Test]
        public void Test()
        {
            var template = Template.FromType<EmptyTemplateVariableAttributeNoMatch>();
            var data = """
                P100
                F200
                """;

            Assert.Catch<TemplateMapException>(()=> template.Parse<EmptyTemplateVariableAttributeNoMatch>(data));

        }
    }


}

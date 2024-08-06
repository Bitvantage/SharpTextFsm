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

namespace Test.Generic.Mappings
{
    internal class ListCreator : ITemplate
    {
        [Variable(ListConverter = typeof(CommaSeparatedList))]
        public string ValueProperty { get; set; }

        string ITemplate.TextFsmTemplate =>
            """
            Value List ValueProperty (\d+)

            Start
             ^${ValueProperty}
            """;

        [Test]
        public void Test()
        {
            var template = Template.FromType<ListCreator>();
            var data = """
                100
                200
                300
                400
                """;

            var results = template.Run<ListCreator>(data).ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].ValueProperty, Is.EqualTo("100,200,300,400"));
        }

        class CommaSeparatedList : Bitvantage.SharpTextFsm.ListCreators.ListCreator<string,string>
        {
            public override string Create(string[] values)
            {
                return string.Join(",", values);
            }
        }
    }


}

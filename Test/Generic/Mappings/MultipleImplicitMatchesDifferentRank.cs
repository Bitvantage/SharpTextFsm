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

namespace Test.Generic.Mappings
{
    internal class MultipleImplicitMatchesDifferentRank : ITemplate
    {
        public long valueproperty { get; set; }
        public long Value_Property_ { get; set; }

        string ITemplate.TextFsmTemplate =>
            """
            Value ValueProperty (\d+)
            Value ValueField (\d+)

            Start
             ^P${ValueProperty}
             ^F${ValueField}
            """;

        [Test]
        public void Test()
        {
            var template = Template.FromType<MultipleImplicitMatchesDifferentRank>();
            var data = """
                P100
                F200
                """;

            var results = template.Run<MultipleImplicitMatchesDifferentRank>(data).ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Value_Property_, Is.EqualTo(default(long)));
            Assert.That(results[0].valueproperty, Is.EqualTo(100));
        }
    }


}

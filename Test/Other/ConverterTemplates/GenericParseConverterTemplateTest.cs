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
using Bitvantage.SharpTextFsm.TypeConverters;

namespace Test.Other.ConverterTemplates
{
    internal class GenericParseConverterTemplateTest : ITemplate
    {
        string ITemplate.TextFsmTemplate => """
            Value NUMBER (\d+)
            Value NULLABLE_NUMBER (\d+)
            Value NULLABLE_NUMBER_THAT_IS_NULL (.+)
            Value LONG (\d+)
            Value NULLABLE_LONG (\d+)
            Value NULLABLE_LONG_THAT_IS_NULL (.+)
            Value STRING (.+)
            Value NULLABLE_STRING (.+)

            Start
             ^NUMBER: ${NUMBER}
             ^NULLABLE_NUMBER: ${NULLABLE_NUMBER}
             ^NULLABLE_NUMBER_THAT_IS_NULL: ${NULLABLE_NUMBER_THAT_IS_NULL}
             ^LONG: ${LONG}
             ^NULLABLE_LONG: ${NULLABLE_LONG}
             ^NULLABLE_LONG_THAT_IS_NULL: ${NULLABLE_LONG_THAT_IS_NULL}
             ^$
             ^. -> Error
            """;

        private readonly string _data = """
            NUMBER: 10
            NULLABLE_NUMBER: 20
            NULLABLE_NUMBER_THAT_IS_NULL: dog
            
            LONG: 10
            NULLABLE_LONG: 20
            NULLABLE_LONG_THAT_IS_NULL: dog
            
            NULLABLE_LONG: 20
            NULLABLE_LONG_THAT_IS_NULL: dog
            """;

        [Variable(Name = "NUMBER", Converter = typeof(GenericParseConverter<int>))]
        public int Number { get; set; }

        [Variable(Name = "NULLABLE_NUMBER", Converter = typeof(GenericParseConverter<int?>))]
        public int? NullableNumber { get; set; }

        [Variable(Name = "NULLABLE_NUMBER_THAT_IS_NULL", Converter = typeof(GenericParseConverter<int?>), ThrowOnConversionFailure = false)]
        public int? NullableNumberThatIsNull { get; set; }

        [Variable(Name = "LONG", Converter = typeof(GenericParseConverter<long>))]
        public long LONG { get; set; }

        [Variable(Name = "NULLABLE_LONG", Converter = typeof(GenericParseConverter<long?>))]
        public long? NullableLONG { get; set; }

        [Variable(Name = "NULLABLE_LONG_THAT_IS_NULL", Converter = typeof(GenericParseConverter<long?>), ThrowOnConversionFailure = false)]
        public long? NullableLONGThatIsNull { get; set; }

        [Test(Description = "Unpopulated lists are empty and not null")]
        public void RawTest01()
        {
            var genericTemplate = Template.FromType<GenericParseConverterTemplateTest>();

            var genericResult = genericTemplate.Run<GenericParseConverterTemplateTest>(_data);

            Assert.That(genericResult.Count, Is.EqualTo(1));

            var result = genericResult.Single();

            Assert.That(result.LONG, Is.EqualTo(10));
            Assert.That(result.NullableLONG, Is.EqualTo(20));
            Assert.That(result.NullableLONGThatIsNull, Is.Null);

            Assert.That(result.Number, Is.EqualTo(10));
            Assert.That(result.NullableNumber, Is.EqualTo(20));
            Assert.That(result.NullableNumberThatIsNull, Is.Null);
        }
    }
}

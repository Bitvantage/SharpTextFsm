using System.Diagnostics.CodeAnalysis;
using System.Net;
using Bitvantage.SharpTextFSM;
using Bitvantage.SharpTextFSM.Attributes;
using Bitvantage.SharpTextFSM.TypeConverters;

namespace Test.Generic.Converters
{
    internal class CustomTypeConverter : ITemplate
    {
        [TemplateVariable(Converter = typeof(MyTypeConverter))]
        public MyCustomType ValueProperty { get; set; }
        [TemplateVariable(Converter = typeof(MyTypeConverter))]
        public MyCustomType ValueField;

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
            var template = Template.FromType<CustomTypeConverter>();
            var data = """
                P100
                F200
                """;

            var results = template.Parse<CustomTypeConverter>(data).ToList();

            Assert.That(results.Count, Is.EqualTo(1));

            Assert.That(results[0].ValueProperty.Address, Is.EqualTo(IPAddress.Parse("100.0.0.0")));
            Assert.That(results[0].ValueProperty.MagicNumber, Is.EqualTo(314.159271f));
            Assert.That(results[0].ValueProperty.OriginalValue, Is.EqualTo("100"));

            Assert.That(results[0].ValueField.Address, Is.EqualTo(IPAddress.Parse("200.0.0.0")));
            Assert.That(results[0].ValueField.MagicNumber, Is.EqualTo(628.318542f));
            Assert.That(results[0].ValueField.OriginalValue, Is.EqualTo("200"));
        }

        public class MyTypeConverter : ValueConverter<MyCustomType?>
        {
            public override bool TryConvert(string value, [NotNullWhen(true)]out MyCustomType? convertedValue)
            {
                if (!uint.TryParse(value, out var parsedValue))
                {
                    convertedValue = null;
                    return false;
                }

                convertedValue = new MyCustomType(value, parsedValue * 3.14159265359f, new IPAddress(BitConverter.GetBytes(parsedValue)));
                return true;
            }
        }
        public record MyCustomType(string OriginalValue, float MagicNumber, IPAddress Address);
    }
}
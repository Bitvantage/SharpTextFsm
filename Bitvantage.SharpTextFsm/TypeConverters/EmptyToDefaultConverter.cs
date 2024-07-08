namespace Bitvantage.SharpTextFsm.TypeConverters
{
    public class EmptyToDefaultConverter<T, TValueConverter> : ValueConverter<T?>
        where TValueConverter : ValueConverter<T>, new()
        where T : struct

    {
        private readonly TValueConverter _innerConverterInstance = new();

        //public override bool TryConvert(string value, out T? convertedValue)
        //{
        //    if (value == string.Empty)
        //    {
        //        convertedValue = null;
        //        return true;
        //    }

        //    var success = _innerConverterInstance.TryConvert(value, out T innerConverterValue);
        //    convertedValue = innerConverterValue;

        //    return success;
        //}

        public override bool TryConvert(string value, out T? convertedValue)
        {
            if (value == string.Empty)
            {
                convertedValue = null;
                return true;
            }

            var success = _innerConverterInstance.TryConvert(value, out T innerConverterValue);
            convertedValue = innerConverterValue;

            return success;
        }
    }

    public class EmptyToDefaultConverterX<T, TValueConverter> : ValueConverter<T?>
        where TValueConverter : ValueConverter<T>, new()
        where T : class

    {
        private readonly TValueConverter _innerConverterInstance = new();

        public override bool TryConvert(string value, out T? convertedValue)
        {
            if (value == string.Empty)
            {
                convertedValue = null;
                return true;
            }

            var success = _innerConverterInstance.TryConvert(value, out T innerConverterValue);
            convertedValue = innerConverterValue;

            return success;
        }

    }
}
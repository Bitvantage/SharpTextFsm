namespace Bitvantage.SharpTextFsm.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class TemplateTranslationAttribute : Attribute
    {
        private readonly string _oldValue;
        private readonly string? _newValue;

        public TemplateTranslationAttribute(string oldValue, string? newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        internal string? Translate(string value)
        {
            return _oldValue == value ? _newValue : value;
        }

        internal static string? TranslateAll(TemplateTranslationAttribute[] translations, string value)
        {
            foreach (var translation in translations)
                if(translation._oldValue == value) 
                    return translation._newValue;

            return value;
        }

    }
}

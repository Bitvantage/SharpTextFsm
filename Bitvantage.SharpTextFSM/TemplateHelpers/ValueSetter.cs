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

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Bitvantage.SharpTextFsm.Attributes;
using Bitvantage.SharpTextFsm.Exceptions;
using Bitvantage.SharpTextFsm.ListCreators;
using Bitvantage.SharpTextFsm.TypeConverters;

namespace Bitvantage.SharpTextFsm.TemplateHelpers
{
    internal class ValueSetter
    {
        private readonly object? _defaultValue;
        private readonly TemplateVariableAttribute _variableConfiguration;
        private readonly TemplateTranslationAttribute[] _translations;
        private readonly ValueConverter _valueConverter;
        private readonly MemberInfo _memberInfo;
        private readonly ListCreator? _listCreator;
        private readonly Action<object, object> _setAction;

        private ValueSetter(MemberInfo memberInfo, Type underlyingType, ListCreator? listCreator, ValueConverter valueConverter, object? defaultValue, TemplateVariableAttribute variableConfiguration, TemplateTranslationAttribute[] translations)
        {
            _memberInfo = memberInfo;
            _listCreator = listCreator;
            _valueConverter = valueConverter;
            _defaultValue = defaultValue;
            _variableConfiguration = variableConfiguration;
            _translations = translations;

            _setAction = GenerateAssignAction(memberInfo, underlyingType);
        }

        public static bool TryCreate(ValueDescriptor valueDescriptor, TemplateVariableAttribute memberConfiguration, TemplateTranslationAttribute[] translations, MemberInfo memberInfo, Type underlyingType, [NotNullWhen(true)] out ValueSetter? valueSetter)
        {
            ListCreator? listCreator = null;
            // if the TextFSM descriptor is a list, then create a list creator to handle it
            if ((valueDescriptor.Options & Option.List) != 0)
            {
                listCreator = CreateListCreator(memberConfiguration, underlyingType);
                if (listCreator == null)
                    throw new TemplateMapException($"The value '{valueDescriptor.Name}' is a 'List'; however no {nameof(ListCreator)} could be created for the member named '{memberInfo.Name}' of type {underlyingType}");
            }

            // the item type is the type contained by the list
            var itemType = listCreator?.ItemType ?? underlyingType;

            // if the item type is nullable, use the underlying type; otherwise use the original item time
            var nullableUnderlyingType = Nullable.GetUnderlyingType(itemType);
            itemType = nullableUnderlyingType ?? itemType;

            // create a value converter
            var valueConverter = CreateValueConverter(memberConfiguration, itemType);
            if (valueConverter == null)
            {
                valueSetter = null;
                return false;
            }

            // set the default value on a conversion failure
            object? defaultValue = null;
            if (!memberConfiguration.ThrowOnConversionFailure)
            {
                // if there is no default value specified, use the types default value
                if (memberConfiguration.DefaultValue == null)
                {
                    // if the type is nullable, the default value is always null
                    if (nullableUnderlyingType != null)
                        defaultValue = null;
                    else
                        defaultValue = GetDefault(itemType);
                }
                // if there is a default value specified, attempt to convert the string value to the correct type
                else if (valueConverter.TryConvert(memberConfiguration.DefaultValue, out var convertedDefaultValue))
                    defaultValue = convertedDefaultValue;
                else
                    throw new TemplateTypeConversionException(valueConverter.GetType(), memberInfo, memberConfiguration.DefaultValue);
            }

            valueSetter = new ValueSetter(memberInfo, underlyingType, listCreator, valueConverter, defaultValue, memberConfiguration, translations);
            return true;
        }

        private static object? GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        public void SetValue(object targetInstance, object value)
        {
            if (_listCreator == null)
            {
                // set value of single item
                var stringValue = (string)value;

                // apply any string translations
                stringValue = TemplateTranslationAttribute.TranslateAll(_translations, stringValue);

                if(stringValue == null || (_variableConfiguration.SkipEmpty && stringValue == string.Empty))
                    return;

                if (!_valueConverter.TryConvert(stringValue, out var convertedValue))
                {
                    if (_variableConfiguration.ThrowOnConversionFailure)
                        throw new TemplateTypeConversionException(_valueConverter.GetType(), _memberInfo, stringValue);

                    _setAction.Invoke(targetInstance, _defaultValue);
                }
                else
                    _setAction.Invoke(targetInstance, convertedValue);
            }
            else
            {
                // set value of list
                var stringValues = ((List<string>)value)
                    .Select(stringValue => TemplateTranslationAttribute.TranslateAll(_translations, stringValue))
                    .Where(item => item != null && (!_variableConfiguration.SkipEmpty || item != string.Empty))
                    .ToList();

                var convertedValues = Array.CreateInstance(_listCreator.ItemType, stringValues.Count);

                for (var index = 0; index < stringValues.Count; index++)
                {
                    if (_variableConfiguration.SkipEmpty && stringValues[index] == string.Empty)
                        continue;

                    if (!_valueConverter.TryConvert(stringValues[index], out var convertedValue))
                    {
                        if (_variableConfiguration.ThrowOnConversionFailure)
                            throw new TemplateTypeConversionException(_valueConverter.GetType(), _memberInfo, (string)value);

                        convertedValues.SetValue(_defaultValue, index);
                    }
                    else
                        convertedValues.SetValue(convertedValue, index);
                }

                var convertedList = _listCreator.Create(convertedValues);

                _setAction.Invoke(targetInstance, convertedList);
            }
        }

        private static Action<object, object> GenerateAssignAction(MemberInfo memberInfo, Type underlyingType)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            // (instance, value) => ((InstanceType)instance).PropertyOrField = (PropertyOrFieldType)value
            var assignExpression =
                Expression.Assign(
                    Expression.MakeMemberAccess(
                        Expression.Convert(instanceParameter, memberInfo.DeclaringType),
                        memberInfo),
                    Expression.Convert(valueParameter, underlyingType)
                );

            var lambda = Expression.Lambda<Action<object, object>>(assignExpression, instanceParameter, valueParameter);
            var action = lambda.Compile();

            return action;
        }

        private static ValueConverter? CreateValueConverter(TemplateVariableAttribute? templateVariableAttribute, Type targetType)
        {
            Type? converterType = null;

            if (templateVariableAttribute is { Converter: not null }) // if there is a converter defined on the TemplateVariableAttribute, use it
                converterType = templateVariableAttribute.Converter;
            else if (targetType == typeof(string)) // if there is not a converter defined, try to use the GenericTryParseConverter, then the GenericParseConverter
                converterType = typeof(StringConverter);
            else if (targetType.IsEnum)
                converterType = typeof(EnumConverter<>).MakeGenericType(targetType);

            // TODO: add some smarts in and stop using CanConvert?
            else if (converterType == null)
                foreach (var genericConverterType in new[] { typeof(GenericTryParseConverter<>), typeof(GenericParseConverter<>) })
                {
                    var makeGenericType = genericConverterType.MakeGenericType(targetType);

                    var canConvert = (bool)makeGenericType.GetMethod("CanConvert", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);

                    if (!canConvert)
                        continue;

                    converterType = makeGenericType;
                    break;
                }

            if (converterType == null)
                return null;

            try
            {
                var converterInstanced = Activator.CreateInstance(converterType);

                return (ValueConverter?)converterInstanced;
            }
            catch (Exception exception)
            {
                throw new ValueConverterCreationException(converterType, exception);
            }
        }

        public static ListCreator? CreateListCreator(TemplateVariableAttribute? templateVariableAttribute, Type listType)
        {
            Type? converterType = null;
            
            if (templateVariableAttribute is { ListConverter: not null }) // if there is a converter defined on the TemplateVariableAttribute, use it
                converterType = templateVariableAttribute.ListConverter;
            else if (templateVariableAttribute is { ListConverter: not null }) // if there is not a converter defined, try to use the built-in converters
                converterType = templateVariableAttribute.Converter;
            else if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>)) // TODO: need to validate the IItem type is the same?
                converterType = typeof(GenericListCreator<>).MakeGenericType(listType.GetGenericArguments().Single());
            else if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)) // TODO: need to validate the IItem type is the same?
                converterType = typeof(ReadOnlyCollectionCreator<>).MakeGenericType(listType.GetGenericArguments().Single());
            else if (listType.IsArray) converterType = typeof(ArrayCreator<>).MakeGenericType(listType.GetElementType());

            // if there is no way to convert the value...
            // TODO: throw exception if this was an explicit binding, and ignore if it was an implicit binding?
            if (converterType == null)
                return null; // TODO: throw?

            try
            {
                var converterInstanced = Activator.CreateInstance(converterType);

                return (ListCreator)converterInstanced;
            }
            catch (Exception exception)
            {
                throw new ListCreatorCreationException(converterType, exception);
            }
        }
    }
}
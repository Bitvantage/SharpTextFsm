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

using System.Collections.Immutable;
using System.Reflection;
using Bitvantage.SharpTextFsm.Attributes;
using Bitvantage.SharpTextFsm.Exceptions;

namespace Bitvantage.SharpTextFsm.TemplateHelpers;

internal class TypeSerializer<T>
{
    private readonly List<Action<T, object>> RawRowSetters;
    private readonly Dictionary<ValueDescriptor, List<ValueSetter>> _setters;

    public TypeSerializer(ValueDescriptorCollection valueDescriptorCollection)
    {
        var rawRowFields = typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(fieldInfo => fieldInfo.GetCustomAttribute<RawRowAttribute>() != null)
            .ToList();

        var invalidRawRowFields = rawRowFields
            .Where(fieldInfo => fieldInfo.FieldType != typeof(Row))
            .FirstOrDefault();

        if (invalidRawRowFields != null)
            throw new TemplateMapException($"The field '{invalidRawRowFields.Name}' of type {invalidRawRowFields.FieldType} is decorated with the '{nameof(RawRowAttribute)}' attribute is an invalid type. The type must be of type {nameof(Row)}");

        var rawRowFieldSetters = rawRowFields
            .Where(fieldInfo => fieldInfo.FieldType == typeof(Row))
            .Select(FastInvoker.CreateUntypedSetterAction<T>)
            .ToList();

        var rawRowProperties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(propertyInfo => propertyInfo.GetCustomAttribute<RawRowAttribute>() != null)
            .ToList();

        var invalidRawRowProperties = rawRowProperties
            .Where(propertyInfo => propertyInfo.PropertyType != typeof(Row))
            .FirstOrDefault();

        if (invalidRawRowProperties != null)
            throw new TemplateMapException($"The property '{invalidRawRowProperties.Name}' of type {invalidRawRowProperties.PropertyType} is decorated with the '{nameof(RawRowAttribute)}' attribute is an invalid type. The type must be of type {nameof(Row)}");

        var rawRowPropertySetters = rawRowProperties
            .Where(propertyInfo => propertyInfo.PropertyType == typeof(Row))
            .Where(propertyInfo => propertyInfo.CanWrite)
            .Select(FastInvoker.CreateUntypedSetterAction<T>)
            .ToList();

        RawRowSetters = Enumerable.Empty<Action<T, object>>()
            .Concat(rawRowFieldSetters)
            .Concat(rawRowPropertySetters)
            .ToList();


        _setters = BuildRowSetters(valueDescriptorCollection);
    }

    /// <summary>
    /// generates a delegate that takes a string value and sets the field to the value
    /// </summary>
    /// <param name="valueDescriptorCollection"></param>
    /// <returns></returns>
    /// <exception cref="ValueConverterCreationException"></exception>
    /// <exception cref="TemplateMapException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private Dictionary<ValueDescriptor, List<ValueSetter>> BuildRowSetters(ValueDescriptorCollection valueDescriptorCollection)
    {
        // members that have a VariableAttribute must match a ValueDescriptor
        // multiple members with a VariableAttribute that define a Name can match the same ValueDescriptor
        // members that do not have a name defined in their VariableAttribute are matched using the mapping strategy defined on the type

        var namedFields = typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Select(info => new { Member = (MemberInfo)info, Metadata = info.GetCustomAttribute<VariableAttribute>(), Translations = info.GetCustomAttributes<ValueTransformerAttribute>().ToArray() })
            .Where(item => item.Metadata != null)
            .Where(item => !item.Metadata!.Ignore)
            .Select(item => new NamedFieldPair(item.Metadata.Name, item.Member, item.Metadata, item.Translations));

        var namedProperties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(info => new { Member = info, Metadata = info.GetCustomAttribute<VariableAttribute>(), Translations = info.GetCustomAttributes<ValueTransformerAttribute>().ToArray() })
            .Where(item => item.Member.CanWrite)
            .Select(item => new { Member = (MemberInfo)item.Member, item.Metadata, item.Translations })
            .Where(item => item.Metadata != null)
            .Where(item => !item.Metadata!.Ignore)
            .Select(item => new NamedFieldPair(item.Metadata.Name, item.Member, item.Metadata, item.Translations));

        var namedFieldPairs = Enumerable.Empty<NamedFieldPair>()
            .Concat(namedFields)
            .Concat(namedProperties)
            .Where(item=>item.Configuration?.Name != null)
            .ToLookup(item => item.Name, item => item);

        var configuredAndUnnamedMembers = Enumerable.Empty<NamedFieldPair>()
            .Concat(namedFields)
            .Concat(namedProperties)
            .Where(item=>item.Configuration?.Name == null)
            .Select(item=>item.MemberInfo)
            .ToHashSet();

        var unnamedFields = typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Select(item => new NamedFieldPair(item.Name, item, item.GetCustomAttribute<VariableAttribute>(), item.GetCustomAttributes<ValueTransformerAttribute>().ToArray()))
            .Where(item => item.Configuration?.Name == null && (item.Configuration?.Ignore ?? false) == false);

        var unnamedProperties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(item => item.CanWrite)
            .Select(item => new NamedFieldPair(item.Name, item, item.GetCustomAttribute<VariableAttribute>(), item.GetCustomAttributes<ValueTransformerAttribute>().ToArray()))
            .Where(item => item.Configuration?.Name == null && (item.Configuration?.Ignore ?? false) == false);

        var unnamedFieldPairs = Enumerable.Empty<NamedFieldPair>()
            .Concat(unnamedFields)
            .Concat(unnamedProperties)
            .ToList();

        var usedExplicitNames = new HashSet<string>(namedFieldPairs.Select(item => item.Key));

        // get the mapping strategy from the type; or use the default value if not configured

        var mappingStrategies = typeof(T)
            .GetCustomAttribute<TemplateAttribute>() ?
            .MappingStrategies ?? MappingStrategy.Exact | MappingStrategy.IgnoreCase | MappingStrategy.SnakeCase;

        // if the MappingStrategy flags include Disabled; remove any other flags 
        if (mappingStrategies.HasFlag(MappingStrategy.Disabled))
            mappingStrategies = MappingStrategy.Disabled;

        var valueSetters = new Dictionary<ValueDescriptor, List<ValueSetter>>();
        var unmappedValueDescriptors = valueDescriptorCollection.Values.Where(item=>item.Options != Option.Regex).ToHashSet();

        // go through all fields and properties with configuration attribute and a name defined
        foreach (var fieldPairs in namedFieldPairs)
        {
            // get the value descriptor that matches the name in the configuration attribute
            var valueDescriptor = unmappedValueDescriptors.SingleOrDefault(item => item.Name == fieldPairs.Key);
            if (valueDescriptor != null)
            {
                // create a value setter for each member
                var valueDescriptorSetters = new List<ValueSetter>();
                foreach (var namedFieldPair in fieldPairs)
                    if (ValueSetter.TryCreate(valueDescriptor, namedFieldPair.Configuration ?? VariableAttribute.Default, namedFieldPair.TemplateTranslations?? new ValueTransformerAttribute[] { }, namedFieldPair.MemberInfo, namedFieldPair.UnderlyingType, out var valueSetter))
                        valueDescriptorSetters.Add(valueSetter);
                    else
                        throw new ValueConverterCreationException($"Could not create a value converter for {namedFieldPair.UnderlyingType}.{namedFieldPair.MemberInfo.Name}");

                valueSetters.Add(valueDescriptor, valueDescriptorSetters);
                
                // remove the value descriptor from the mapping pool since it has already been mapped
                unmappedValueDescriptors.Remove(valueDescriptor);
            }
            else
                throw new TemplateMapException($"Failed to bind explicitly defined template VALUES specified in the {nameof(VariableAttribute)}: {fieldPairs.Key}");
        }

        // go through each possible mapping strategy combination
        var unusedUnnamedFieldPairs = unnamedFieldPairs.ToList();
        foreach (var mappingStrategy in new[] { MappingStrategy.Exact, MappingStrategy.IgnoreCase, MappingStrategy.SnakeCase, MappingStrategy.IgnoreCase | MappingStrategy.SnakeCase })
        {
            // if the mapping strategy combination has not been configured then skip it
            if((mappingStrategies & mappingStrategy) != mappingStrategy)
                continue;

            foreach (var valueDescriptor in unmappedValueDescriptors.OrderBy(item=>item.Name).ToList())
            {
                // attempt to match the name using the current mapping strategy
                var unnamedFieldPair = mappingStrategy switch
                {
                    MappingStrategy.Exact => unusedUnnamedFieldPairs.FirstOrDefault(item => item.Name == valueDescriptor.Name),
                    MappingStrategy.IgnoreCase => unusedUnnamedFieldPairs.FirstOrDefault(item => item.Name.Equals(valueDescriptor.Name, StringComparison.InvariantCultureIgnoreCase)),
                    MappingStrategy.SnakeCase => unusedUnnamedFieldPairs.FirstOrDefault(item => item.Name.Replace("_", "") == valueDescriptor.Name.Replace("_", "")),
                    MappingStrategy.IgnoreCase | MappingStrategy.SnakeCase => unusedUnnamedFieldPairs.FirstOrDefault(item => item.Name.Replace("_", "").Equals(valueDescriptor.Name.Replace("_", ""), StringComparison.InvariantCultureIgnoreCase)),
                    _ => throw new ArgumentOutOfRangeException()
                };

                // if there is a matching pair; generate a setter
                if (unnamedFieldPair != null)
                {
                    if (ValueSetter.TryCreate(valueDescriptor, unnamedFieldPair.Configuration ?? VariableAttribute.Default, unnamedFieldPair.TemplateTranslations ?? new ValueTransformerAttribute[]{}, unnamedFieldPair.MemberInfo, unnamedFieldPair.UnderlyingType, out var valueSetter))
                        valueSetters.Add(valueDescriptor, new List<ValueSetter> { valueSetter });

                    unmappedValueDescriptors.Remove(valueDescriptor);
                    unusedUnnamedFieldPairs.Remove(unnamedFieldPair);
                    configuredAndUnnamedMembers.Remove(unnamedFieldPair.MemberInfo);
                }
            }
        }

        // check if any field or property values that are set with a configuration attribute that have no name have been missed
        if (configuredAndUnnamedMembers.Any())
            throw new TemplateMapException($"Failed to bind explicitly defined template VALUES specified in the {nameof(VariableAttribute)}: {string.Join(", ", configuredAndUnnamedMembers.Select(item=>item.Name))}");

        return valueSetters;
    }

    public IEnumerable<T> Serialize(RowCollection rows, object? state)
    {
        var rowIndex = -1;
        IImmutableList<Row> rowsCopy = rows.ToImmutableList();

        foreach (var row in rows.Rows)
        {
            rowIndex++;

            // create a new object
            var instance = Activator.CreateInstance<T>();

            // set values
            foreach (var setter in _setters)
            {
                if(!row.TryGetValue(setter.Key.Name, out var rawValue))
                    continue;

                // skip setting null values
                // BUG: this means any unset value in the POCO ends up with its default value. Unclear if the is correct and desirable
                if (rawValue == null)
                    continue;

                setter.Value.First().SetValue(instance, rawValue);
            }

            // set fields and properties that have the RawRowAttribute
            foreach (var rawRowSetter in RawRowSetters)
                rawRowSetter(instance, row);

            // validate the object
            if (instance is ITemplateValidator rowValidator)
            {
                // if the validation failed, do not return it in the result set 
                if (rowValidator.Validate(row, rowIndex, rowsCopy, state))
                    yield return instance;
            }
            else
                yield return instance;
        }
    }

    public record NamedFieldPair(string Name, MemberInfo MemberInfo, VariableAttribute? Configuration, ValueTransformerAttribute[]? TemplateTranslations)
    {
        public Type UnderlyingType
        {
            get
            {
                switch (MemberInfo.MemberType)
                {
                    case MemberTypes.Event:
                        return ((EventInfo)MemberInfo).EventHandlerType!;

                    case MemberTypes.Field:
                        return ((FieldInfo)MemberInfo).FieldType;

                    case MemberTypes.Method:
                        return ((MethodInfo)MemberInfo).ReturnType;

                    case MemberTypes.Property:
                        return ((PropertyInfo)MemberInfo).PropertyType;

                    default:
                        throw new ArgumentException("Must be of type EventInfo, FieldInfo, MethodInfo, or PropertyInfo", nameof(MemberInfo));
                }
            }
        }
    }
}
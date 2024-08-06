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

using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Bitvantage.SharpTextFsm.Definitions;
using Bitvantage.SharpTextFsm.Exceptions;

namespace Bitvantage.SharpTextFsm.TemplateHelpers;

public class ValueDescriptorCollection : IReadOnlyList<ValueDescriptor>
{
    public static readonly List<ValueDefinition> LibraryRegexValues = new()
    {
        // numbers
        new ValueDefinition("_BASE_10_NUMBER", Option.Regex, "(?<![0-9.+-])(?>[+-]?(?:(?:[0-9]+(?:\\.[0-9]+)?)|(?:\\.[0-9]+)))"),
        new ValueDefinition("_BASE_16_FLOAT", Option.Regex, "\\b(?<![0-9A-Fa-f.])(?:[+-]?(?:0x)?(?:(?:[0-9A-Fa-f]+(?:\\.[0-9A-Fa-f]*)?)|(?:\\.[0-9A-Fa-f]+)))\\b"),
        new ValueDefinition("_BASE_16_NUMBER", Option.Regex, "(?<![0-9A-Fa-f])(?:[+-]?(?:0x)?(?:[0-9A-Fa-f]+))"),
        new ValueDefinition("_INTEGER", Option.Regex, "(?:[+-]?(?:[0-9]+))"),
        new ValueDefinition("_NON_NEGATIVE_INTEGER", Option.Regex, "\\b(?:[0-9]+)\\b"),
        new ValueDefinition("_NUMBER", Option.Regex, "(?:${_BASE_10_NUMBER})"),
        new ValueDefinition("_POSITIVE_INTEGER", Option.Regex, "\\b(?:[1-9][0-9]*)\\b"),

        // words
        new ValueDefinition("_WORD", Option.Regex, "\\b\\w+\\b"),
        new ValueDefinition("_NOT_SPACE ", Option.Regex, "\\S+"),
        new ValueDefinition("_SPACE ", Option.Regex, "\\s*"),
        new ValueDefinition("_DATA ", Option.Regex, ".*?"),
        new ValueDefinition("_GREEDY_DATA ", Option.Regex, ".*"),
        new ValueDefinition("_QUOTED_STRING ", Option.Regex, "(?>(?<!\\\\)(?>\"(?>\\\\.|[^\\\\\"]+)+\"|\"\"|(?>'(?>\\\\.|[^\\\\']+)+')|''|(?>`(?>\\\\.|[^\\\\`]+)+`)|``))"),
        new ValueDefinition("_UUID ", Option.Regex, "[A-Fa-f0-9]{8}-(?:[A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}"),
        new ValueDefinition("_URN ", Option.Regex, "urn:[0-9A-Za-z][0-9A-Za-z-]{0,31}:(?:%[0-9a-fA-F]{2}|[0-9A-Za-z()+,.:=@;$_!*'/?#-])+"),
        new ValueDefinition("_EMAIL_LOCAL_PART", Option.Regex, "[a-zA-Z0-9!#$%&'*+\\-/=?^_`{|}~]{1,64}(?:\\.[a-zA-Z0-9!#$%&'*+\\-/=?^_`{|}~]{1,62}){0,63}"),
        new ValueDefinition("_HOSTNAME", Option.Regex, "\\b(?:[0-9A-Za-z][0-9A-Za-z-]{0,62})(?:\\.(?:[0-9A-Za-z][0-9A-Za-z-]{0,62}))*(\\.?|\\b)"),
        new ValueDefinition("_EMAIL_ADDRESS", Option.Regex, "${_EMAIL_LOCAL_PART}@${_HOSTNAME}"),

        // network
        new ValueDefinition("_MAC_ADDRESS_DOUBLE_COLON", Option.Regex, "(?:[A-Fa-f0-9]{2}:[A-Fa-f0-9]{2}:[A-Fa-f0-9]{2}:[A-Fa-f0-9]{2}:[A-Fa-f0-9]{2}:[A-Fa-f0-9]{2})"),
        new ValueDefinition("_MAC_ADDRESS_DOUBLE_DASH", Option.Regex, "(?:[A-Fa-f0-9]{2}-[A-Fa-f0-9]{2}-[A-Fa-f0-9]{2}-[A-Fa-f0-9]{2}-[A-Fa-f0-9]{2}-[A-Fa-f0-9]{2})"),
        new ValueDefinition("_MAC_ADDRESS_DOUBLE_DOT", Option.Regex, "(?:[A-Fa-f0-9]{2}\\.[A-Fa-f0-9]{2}\\.[A-Fa-f0-9]{2}\\.[A-Fa-f0-9]{2}\\.[A-Fa-f0-9]{2}\\.[A-Fa-f0-9]{2})"),

        new ValueDefinition("_MAC_ADDRESS_QUAD_COLON", Option.Regex, "(?:[A-Fa-f0-9]{4}:[A-Fa-f0-9]{4}:[A-Fa-f0-9]{4})"),
        new ValueDefinition("_MAC_ADDRESS_QUAD_DOT", Option.Regex, "(?:[A-Fa-f0-9]{4}\\.[A-Fa-f0-9]{4}\\.[A-Fa-f0-9]{4})"),

        new ValueDefinition("_MAC_ADDRESS", Option.Regex, "(?:${_MAC_ADDRESS_DOUBLE_COLON}|${_MAC_ADDRESS_DOUBLE_DASH}|${_MAC_ADDRESS_DOUBLE_DOT}|${_MAC_ADDRESS_QUAD_COLON}|${_MAC_ADDRESS_QUAD_DOT})"),

        new ValueDefinition("_IPV6", Option.Regex, "((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)(\\.(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)(\\.(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)(\\.(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)(\\.(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)(\\.(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)(\\.(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)(\\.(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)){3}))|:)))(%.+)?"),
        new ValueDefinition("_IPV4", Option.Regex, "(?<![0-9])(?:(?:[0-1]?[0-9]{1,2}|2[0-4][0-9]|25[0-5])[.](?:[0-1]?[0-9]{1,2}|2[0-4][0-9]|25[0-5])[.](?:[0-1]?[0-9]{1,2}|2[0-4][0-9]|25[0-5])[.](?:[0-1]?[0-9]{1,2}|2[0-4][0-9]|25[0-5]))(?![0-9])"),
        new ValueDefinition("_IP", Option.Regex, "(?:${_IPV6}|${_IPV4})"),
        new ValueDefinition("_IP_OR_HOST", Option.Regex, "(?:${_IP}|${_HOSTNAME})"),
        new ValueDefinition("_HOST_AND_PORT", Option.Regex, "${_IP_OR_HOST}:${_POSITIVE_INTEGER}"),

        // dates and times
        new ValueDefinition("_MONTH", Option.Regex, "\\b(?:[Jj]an(?:uary|uar)?|[Ff]eb(?:ruary|ruar)?|[Mm](?:a|ä)?r(?:ch|z)?|[Aa]pr(?:il)?|[Mm]a(?:y|i)?|[Jj]un(?:e|i)?|[Jj]ul(?:y|i)?|[Aa]ug(?:ust)?|[Ss]ep(?:tember)?|[Oo](?:c|k)?t(?:ober)?|[Nn]ov(?:ember)?|[Dd]e(?:c|z)(?:ember)?)\\b"),
        new ValueDefinition("_MONTH_NUMBER", Option.Regex, "(?:0?[1-9]|1[0-2])"),
        new ValueDefinition("_MONTH_DAY", Option.Regex, "(?:(?:0[1-9])|(?:[12][0-9])|(?:3[01])|[1-9])"),
        new ValueDefinition("_DAY", Option.Regex, "(?:Mon(?:day)?|Tue(?:sday)?|Wed(?:nesday)?|Thu(?:rsday)?|Fri(?:day)?|Sat(?:urday)?|Sun(?:day)?)"),
        new ValueDefinition("_YEAR", Option.Regex, "(?>\\d\\d){1,2}"),
        new ValueDefinition("_HOUR", Option.Regex, "(?:2[0123]|[01]?[0-9])"),
        new ValueDefinition("_MINUTE", Option.Regex, "(?:[0-5][0-9])"),
        new ValueDefinition("_SECOND", Option.Regex, "(?:(?:[0-5]?[0-9]|60)(?:[:.,][0-9]+)?)"),
        new ValueDefinition("_TIME", Option.Regex, "(?!<[0-9])${_HOUR}:${_MINUTE}(?::${_SECOND})(?![0-9])"),
        new ValueDefinition("_DATE_US", Option.Regex, "${_MONTH_NUMBER}[/-]${_MONTH_DAY}[/-]${_YEAR}"),
        new ValueDefinition("_DATE_EU", Option.Regex, "${_MONTH_DAY}[./-]${_MONTH_NUMBER}[./-]${_YEAR}"),
        new ValueDefinition("_DATE", Option.Regex, "${_DATE_US}|${_DATE_EU}")
    };

    private static readonly Regex ValueRegex = new("(\\${(?<name>[a-zA-Z0-9_\\-]+?)})|(\\$(?<name>[a-zA-Z0-9_\\-]+(?=\\s|$)))", RegexOptions.Compiled);

    private readonly LookupList<string, ValueDescriptor> _listImplementation;
    public static ReadOnlyDictionary<string, ValueDescriptor> RegexLibrary { get; }

    public ValueDescriptor this[string key] => _listImplementation[key];

    public IEnumerable<string> Keys => _listImplementation.Keys;

    internal Option OptionMask { get; } = Option.None;

    internal Regex ValueDescriptorNamesRegex { get; }

    public IEnumerable<ValueDescriptor> Values => _listImplementation.Values;

    static ValueDescriptorCollection()
    {
        // expand the library regex values
        Dictionary<string, ValueDescriptor> valueDescriptors = new();
        foreach (var valueDefinition in LibraryRegexValues)
        {
            var valueDescriptor = Expand(null, valueDefinition, valueDescriptors);
            valueDescriptors.Add(valueDescriptor.Name, valueDescriptor);
        }

        RegexLibrary = valueDescriptors.AsReadOnly();
    }

    internal ValueDescriptorCollection(Template? template, List<ValueDefinition> valueDefinitions)
    {
        var valueDescriptors = new Dictionary<string, ValueDescriptor>(RegexLibrary);

        foreach (var value in valueDefinitions)
        {
            var valueDescriptor = Expand(template, value, valueDescriptors);

            // if there is a user defined value descriptor with the same name as a built-in, overwrite the built-in
            if (valueDescriptors.TryGetValue(value.Name, out var builtInRegexValueToRemove) && RegexLibrary.TryGetValue(builtInRegexValueToRemove.Name, out var builtInRegex) && ReferenceEquals(builtInRegex, builtInRegexValueToRemove))
                valueDescriptors.Remove(builtInRegexValueToRemove.Name);

            // if the value was previously added; throw
            if (!valueDescriptors.TryAdd(value.Name, valueDescriptor))
                throw new TemplateParseException($"Duplicate value name: {value.Name}", ParseError.DuplicateValueName);
        }

        _listImplementation = new LookupList<string, ValueDescriptor>(descriptor => descriptor.Name, valueDescriptors.Values);

        ValueDescriptorNamesRegex = GenerateRegex();

        // compute the combined option mask for all value descriptors
        foreach (var valueDescriptor in this)
            OptionMask |= valueDescriptor.Options;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_listImplementation).GetEnumerator();
    }

    public IEnumerator<ValueDescriptor> GetEnumerator()
    {
        return _listImplementation.GetEnumerator();
    }

    public int Count => _listImplementation.Count;

    public ValueDescriptor this[int index] => _listImplementation[index];

    public bool ContainsKey(string key)
    {
        return _listImplementation.ContainsKey(key);
    }

    private static ValueDescriptor Expand(Template? template, ValueDefinition? value, Dictionary<string, ValueDescriptor> dictionary)
    {
        // expand 'Value Regex's
        var expandedPattern = ValueRegex.Replace(value.Pattern, match =>
        {
            var key = match.Groups["name"].Value;
            if (!dictionary.TryGetValue(key, out var regexValueDescriptor))
                throw new ArgumentException();

            if (regexValueDescriptor.Options != Option.Regex)
                throw new ArgumentException();

            return regexValueDescriptor.Regex.ToString();
        });

        Regex regex;
        try
        {
            // no need to compile the expression, that will happen later
            // the purpose of this is really to validate the expression and provide a useful exception with context
            regex = new Regex(expandedPattern);
        }
        catch (ArgumentException regularExpressionParseException)
        {
            throw new TemplateParseException($"Value {value.Name} contains an invalid regular expression", ParseError.InvalidValueRegularExpression, regularExpressionParseException);
        }

        var valueDescriptor = new ValueDescriptor(template, value.Name, value.Options, regex);

        return valueDescriptor;
    }

    private Regex GenerateRegex()
    {
        var orderedValueDescriptorNames = this
            .Select(item => item.Name)
            .OrderByDescending(item => item.Length)
            .ThenBy(item => item)
            .Select(Regex.Escape)
            .ToList();

        var sb = new StringBuilder();

        sb.Append("(?<doubleDollar>\\${2})|\n");

        // $variable syntax
        foreach (var valueDescriptorName in orderedValueDescriptorNames)
            sb.Append($"(\\$(?<name>{valueDescriptorName}))|\n");
        sb.Append("(\\$(?<invalid>\\w+))|\n");

        // ${variable} syntax
        foreach (var valueDescriptorName in orderedValueDescriptorNames)
            sb.Append($"(\\$\\{{(?<name>{valueDescriptorName})\\}})|\n");
        sb.Append("(\\$\\{(?<invalid>\\w+)\\})");

        return new Regex(sb.ToString(), RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var valueDescriptor in this.Where(item => item.Options != Option.Regex))
            sb.AppendLine(valueDescriptor.ToString());

        return sb.ToString();
    }

    public bool TryGetValue(string key, out ValueDescriptor value)
    {
        return _listImplementation.TryGetValue(key, out value);
    }
}
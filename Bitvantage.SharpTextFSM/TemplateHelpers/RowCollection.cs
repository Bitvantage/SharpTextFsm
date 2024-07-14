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
using System.Dynamic;
using System.Text.RegularExpressions;

namespace Bitvantage.SharpTextFsm.TemplateHelpers;

public class RowCollection : IReadOnlyList<Row>
{
    internal readonly List<Row> Rows = new();

    private readonly TemplateOptions _templateOptions;

    private readonly ValueDescriptorCollection _valueDescriptorCollection;
    private readonly ReadOnlyDictionary<string, ValueDescriptor> _regularValueDescriptors;
    private Dictionary<string, object?> _filldown = new();
    private List<TemplateFsmContext> _metadata = new();

    internal RowCollection(ValueDescriptorCollection valueDescriptorCollection, TemplateOptions templateOptions)
    {
        _valueDescriptorCollection = valueDescriptorCollection;
        _regularValueDescriptors = _valueDescriptorCollection
            .Where(item => item.Options != Option.Regex)
            .ToDictionary(item => item.Name)
            .AsReadOnly();

        _templateOptions = templateOptions;
    }

    internal Row CurrentRow { get; private set; } = new();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Rows).GetEnumerator();
    }

    public IEnumerator<Row> GetEnumerator()
    {
        return Rows.GetEnumerator();
    }

    public int Count => Rows.Count;

    public Row this[int index] => Rows[index];

    internal void Clear()
    {
        CurrentRow = new Row();
        _metadata = new List<TemplateFsmContext>();
    }

    internal void ClearAll()
    {
        Clear();

        _filldown = new Dictionary<string, object?>();
    }

    private Dictionary<string, object?> CloneRow(Dictionary<string, object?> row)
    {
        var currentRowClone = new Dictionary<string, object?>();
        foreach (var keyValuePair in row)
            switch (keyValuePair.Value)
            {
                case string stringValue:
                    currentRowClone.Add(keyValuePair.Key, stringValue);
                    break;

                case List<string> stringList:
                    currentRowClone.Add(keyValuePair.Key, new List<string>(stringList));
                    break;

                default:
                    throw new InvalidOperationException();
            }

        return currentRowClone;
    }

    internal void Record()
    {
        // skip record if it contains no data
        if (CurrentRow.Count == 0)
            return;

        // populate metadata
        foreach (var valuePair in _valueDescriptorCollection.Where(item => item.Metadata != null))
        {
            var metadataValues = _metadata.Select(item => item.Get(valuePair.Metadata!.Value));

            if (valuePair.Options.HasFlag(Option.List))
                CurrentRow.Add(valuePair.Name, metadataValues.ToList());
            else
                CurrentRow.Add(valuePair.Name, metadataValues.Last());
        }

        // populate filldown values
        foreach (var filldown in _filldown)
            if (_valueDescriptorCollection[filldown.Key].Options.HasFlag(Option.List))
                CurrentRow[filldown.Key] = new List<string>((List<string>)filldown.Value);
            else
                CurrentRow.TryAdd(filldown.Key, filldown.Value);

        // skip record if all required fields are not set
        foreach (var valuePair in _valueDescriptorCollection.Where(item => item.Options.HasFlag(Option.Required)))
            if (!CurrentRow.ContainsKey(valuePair.Name))
            {
                Clear();
                return;
            }

        // add values that were not populated
        foreach (var valueDescriptor in _regularValueDescriptors)
        {
            if (CurrentRow.ContainsKey(valueDescriptor.Key))
                continue;

            if (valueDescriptor.Value.Options.HasFlag(Option.List))
                switch (_templateOptions.UnmatchedListHandling)
                {
                    case UnmatchedHandling.Empty:
                        CurrentRow.Add(valueDescriptor.Value.Name, new List<string>());
                        break;

                    case UnmatchedHandling.Null:
                        CurrentRow.Add(valueDescriptor.Value.Name, null);
                        break;

                    default:
                        throw
                            new ArgumentOutOfRangeException();
                }
            else
                switch (_templateOptions.UnmatchedValueHandling)
                {
                    case UnmatchedHandling.Empty:
                        CurrentRow.Add(valueDescriptor.Value.Name, string.Empty);
                        break;

                    case UnmatchedHandling.Null:
                        CurrentRow.Add(valueDescriptor.Value.Name, null);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }

        Rows.Add(CurrentRow);
        Clear();
    }

    internal void SetMetadata(TemplateFsmContext metadata)
    {
        _metadata.Add(metadata.Clone());
    }

    internal void SetValue(string key, Group value)
    {
        SetValue(_valueDescriptorCollection[key], value);
    }

    internal void SetValue(ValueDescriptor valueDescriptor, Group value)
    {
        if (valueDescriptor.Options.HasFlag(Option.List))
        {
            List<string> list;
            if (CurrentRow.TryGetValue(valueDescriptor.Name, out var listObject))
            {
                // the list already exists in the current row; use it
                list = (List<string>)listObject!;
            }
            else if (_filldown.TryGetValue(valueDescriptor.Name, out listObject))
            {
                // the list does not exist in the current row; but does exist in the filldown cache
                // use the list from the filldown cache and add it to the current row
                list = (List<string>)listObject!;
                CurrentRow.Add(valueDescriptor.Name, list);
            }
            else
            {
                // the list does not exist in either the filldown cache or the current row; add it to both.
                list = new List<string>();
                CurrentRow.Add(valueDescriptor.Name, list);
            }

            //// if the group has multiple capture groups, then add each one as an item to the list
            //// TODO: is there a case where if there is only one capture group it is not equal to the value?
            //if ((value.Captures.Count == 0 && value.Success) || (value.Captures.Count == 1 && value.Captures[0].Value != value.Value))
            //    ;

            if (value.Captures.Count > 1)
                foreach (Capture capture in value.Captures)
                    list.Add(capture.Value);
            else
                list!.Add(value.Value);

            if (valueDescriptor.Options.HasFlag(Option.Filldown)) 
                _filldown[valueDescriptor.Name] = list;

            // populates upwards through the rows until there is a non-empty row
            if (valueDescriptor.Options.HasFlag(Option.Fillup))
                for (var rowIndex = Rows.Count - 1; rowIndex >= 0; rowIndex--) // go backwards through the rows
                    if (Rows[rowIndex].TryGetValue(valueDescriptor.Name, out var existingValue) == false || existingValue == null || ((List<string>)existingValue).Count == 0)
                        Rows[rowIndex][valueDescriptor.Name] = new List<string>(list); // set the value
                    else
                        break; // stop setting values as soon as we hit one where the column already exists
        }
        else
        {
            CurrentRow[valueDescriptor.Name] = value.Value;

            if (valueDescriptor.Options.HasFlag(Option.Filldown))
                _filldown[valueDescriptor.Name] = value.Value;

            // populates upwards through the rows until there is a non-empty row
            if (valueDescriptor.Options.HasFlag(Option.Fillup))
                for (var rowIndex = Rows.Count - 1; rowIndex >= 0; rowIndex--) // go backwards through the rows
                    if (Rows[rowIndex].TryGetValue(valueDescriptor.Name, out var existingValue) == false || existingValue == null || (string)existingValue == string.Empty)
                        Rows[rowIndex][valueDescriptor.Name] = value.Value; // set the value
                    else
                        break; // stop setting values as soon as we hit one where the column already exists
        }
    }

    public IEnumerable<dynamic> ToDynamic()
    {
        foreach (var row in Rows)
        {
            var expandoObject = new ExpandoObject();
            foreach (var valueDescriptor in _valueDescriptorCollection)
                expandoObject.TryAdd(valueDescriptor.Name, row.GetValueOrDefault(valueDescriptor.Name));

            yield return expandoObject;
        }
    }
}
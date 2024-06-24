/*
   Bitvantage.SharpTextFSM
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

namespace Bitvantage.SharpTextFSM.TemplateHelpers;

public class Row : IReadOnlyDictionary<string, object>
{
    private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_values).GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return _values.GetEnumerator();
    }

    public int Count => _values.Count;

    public bool ContainsKey(string key)
    {
        return _values.ContainsKey(key);
    }

    public object this[string key]
    {
        get => _values[key];
        internal set => _values[key] = value;
    }

    public IEnumerable<string> Keys => _values.Keys;

    public bool TryGetValue(string key, out object value)
    {
        return _values.TryGetValue(key, out value);
    }

    public IEnumerable<object> Values => _values.Values;

    internal void Add(KeyValuePair<string, object> item)
    {
        _values.Add(item);
    }

    internal void Add(string key, object value)
    {
        _values.Add(key, value);
    }

    internal void Clear()
    {
        _values.Clear();
    }

    internal Row Clone()
    {
        var currentRowClone = new Row();
        foreach (var keyValuePair in _values)
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

    internal bool Contains(KeyValuePair<string, object> item)
    {
        return _values.Contains(item);
    }

    internal void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        _values.CopyTo(array, arrayIndex);
    }

    internal bool Remove(KeyValuePair<string, object> item)
    {
        return _values.Remove(item);
    }

    internal bool Remove(string key)
    {
        return _values.Remove(key);
    }

    internal bool TryAdd(string key, object value)
    {
        return _values.TryAdd(key, value);
    }
}
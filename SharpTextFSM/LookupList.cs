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

namespace Bitvantage.SharpTextFSM
{
    internal class LookupList<TKey, TValue> : IReadOnlyList<TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Func<TValue, TKey> _keyFunc;
        private readonly IList<TValue> _listImplementation = new List<TValue>();
        private readonly IDictionary<TKey, TValue> _dictionaryImplementation = new Dictionary<TKey, TValue>();


        internal LookupList(Func<TValue, TKey> keyFunc)
        {
            _keyFunc = keyFunc;
        }

        internal LookupList(Func<TValue, TKey> keyFunc, IEnumerable<TValue> values)
        {
            _keyFunc = keyFunc;

            foreach (var value in values) 
                Add(value);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return _dictionaryImplementation.GetEnumerator();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _listImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_listImplementation).GetEnumerator();
        }

        internal void Add(TValue item)
        {
            _dictionaryImplementation.Add(_keyFunc.Invoke(item), item);
            _listImplementation.Add(item);
        }

        internal void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionaryImplementation.Add(item);
            _listImplementation.Add(item.Value);
        }

        internal void Clear()
        {
            _dictionaryImplementation.Clear();
            _listImplementation.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionaryImplementation.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionaryImplementation.CopyTo(array, arrayIndex);
        }

        internal bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if(!_dictionaryImplementation.Remove(item))
                return false;

            _listImplementation.Remove(item.Value);

            return true;
        }

        public bool Contains(TValue item)
        {
            return _listImplementation.Contains(item);
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            _listImplementation.CopyTo(array, arrayIndex);
        }

        internal bool Remove(TValue item)
        {
            if (!_dictionaryImplementation.Remove(_keyFunc.Invoke(item)))
                return false;

            _listImplementation.Remove(item);

            return true;
        }

        public int Count => _listImplementation.Count;

        public bool IsReadOnly => _listImplementation.IsReadOnly;

        public int IndexOf(TValue item)
        {
            return _listImplementation.IndexOf(item);
        }

        internal void Insert(int index, TValue item)
        {
            var key = _keyFunc.Invoke(item);

            if (_dictionaryImplementation.ContainsKey(key))
                throw new ArgumentException("Item with Same Key has already been added");

            _listImplementation.Insert(index, item);
            _dictionaryImplementation.Add(key, item);
        }

        internal void RemoveAt(int index)
        {
            var item = _listImplementation[index];

            _listImplementation.RemoveAt(index);
            _dictionaryImplementation.Remove(_keyFunc.Invoke(item));
        }

        public TValue this[int index]
        {
            get => _listImplementation[index];
            internal set
            {
                var key = _keyFunc.Invoke(value);
                _dictionaryImplementation.Remove(key);

                _dictionaryImplementation.Add(key, value);
                _listImplementation[index] = value;
            }
        }

        internal void Add(TKey key, TValue value)
        {
            _dictionaryImplementation.Add(key, value);
            _listImplementation.Add(value);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionaryImplementation.ContainsKey(key);
        }

        internal bool Remove(TKey key)
        {
            if(!_dictionaryImplementation.Remove(key, out var value))
                return false;

            _listImplementation.Remove(value);

            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionaryImplementation.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => _dictionaryImplementation[key];
            set
            {
                if (_dictionaryImplementation.Remove(key, out var oldValue)) 
                    _listImplementation.Remove(oldValue);

                _dictionaryImplementation[key] = value;
                _listImplementation.Add(value);
            }
        }

        public IEnumerable<TKey> Keys => _dictionaryImplementation.Keys;

        public IEnumerable<TValue> Values => _dictionaryImplementation.Values;
    }

}

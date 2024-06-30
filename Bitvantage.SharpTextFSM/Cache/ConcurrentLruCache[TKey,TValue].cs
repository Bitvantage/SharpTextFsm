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

namespace Bitvantage.SharpTextFsm.Cache;

internal class ConcurrentLruCache<TKey, TValue> : ITemplateCache<TKey, TValue>
    where TKey : notnull
{
    private readonly uint _cacheSize;
    private readonly LinkedList<TKey> _leastRecentlyUsed = new();
    private readonly object _lockObject = new();
    private readonly NamedLocker<TKey, TValue> _namedLocker = new();

    private readonly Dictionary<TKey, Element> _valueCache = new();

    public ConcurrentLruCache(uint cacheSize)
    {
        _cacheSize = cacheSize;
    }

    public CacheStats Stats { get; } = new();

    public void Clear()
    {
        lock (_lockObject)
        {
            _valueCache.Clear();
            _leastRecentlyUsed.Clear();
        }
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> createFunction)
    {
        if (_cacheSize == 0)
            return createFunction.Invoke(key);

        // indirectly lock on the key value
        var value = _namedLocker.Invoke(key, _ =>
        {
            // if the key exists then bump the associated lru entry to the top of the lru list
            Element? element;
            lock (_lockObject)
            {
                if (_valueCache.TryGetValue(key, out element))
                {
                    // bump the node
                    _leastRecentlyUsed.Remove(element.Node);
                    _leastRecentlyUsed.AddFirst(element.Node);

                    ++Stats.Total;
                    ++Stats.Hits;

                    return element.Value;
                }
            }

            // if the key does not exist, create the value
            var createdValue = createFunction.Invoke(key);
            element = new Element(createdValue, new LinkedListNode<TKey>(key));

            lock (_lockObject)
            {
                _valueCache.Add(key, element);
                _leastRecentlyUsed.AddFirst(element.Node);

                ++Stats.Total;
                ++Stats.Misses;

                // if the lru list exceeds the maximum length, remove the oldest entry
                while (_leastRecentlyUsed.Count > _cacheSize)
                {
                    var lastNode = _leastRecentlyUsed.Last;
                    _leastRecentlyUsed.RemoveLast();
                    _valueCache.Remove(lastNode!.Value);
                }

                return createdValue;
            }
        });

        return value;
    }

    internal class CacheStats
    {
        public ulong Hits { get; internal set; }
        public ulong Misses { get; internal set; }
        public double Ratio => Hits / (double)Total;
        public ulong Total { get; internal set; }
    }

    private record Element(TValue Value, LinkedListNode<TKey> Node);
}
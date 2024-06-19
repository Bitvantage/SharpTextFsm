/*
   SharpTextFSM
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

namespace Bitvantage.SharpTextFSM.Cache;

internal class NamedLocker<T, TResult>
    where T : notnull
{
    private readonly Dictionary<T, NamedLock> _locker = new();
    private readonly object _lockerLock = new();

    public TResult Invoke(T name, Func<T, TResult> func)
    {
        NamedLock? namedLock;
        lock (_lockerLock)
        {
            // get the named lock
            // if the named lock does not exist, create it
            if (!_locker.TryGetValue(name, out namedLock))
            {
                namedLock = new NamedLock();
                _locker.Add(name, namedLock);
            }

            namedLock.ReferenceCount++;
        }

        TResult result;
        lock (namedLock.LockObject)
        {
            result = func.Invoke(name);
        }

        lock (_lockerLock)
        {
            namedLock.ReferenceCount--;

            // if there are no longer any references to the named lock, remove it
            if (namedLock.ReferenceCount == 0)
                _locker.Remove(name);
        }

        return result;
    }

    private class NamedLock
    {
        public object LockObject { get; } = new();
        public long ReferenceCount { get; set; }
    }
}
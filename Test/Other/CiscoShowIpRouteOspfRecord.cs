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

using System.Net;
using Bitvantage.SharpTextFsm.Attributes;

namespace Test.Other
{
    [Template(MappingStrategy.IgnoreCase)]
    internal class CiscoShowIpRouteOspfRecord
    {
        [Variable(Name = "network")]
        public IPAddress Network { get; init; }
        [Variable(Name = "mask")]
        public int Mask { get; init; }
        [Variable(Name = "distance")]
        public int Distance { get; init; }
        [Variable(Name = "metric")]
        public int Metric { get; init; }
        [Variable(Name = "nexthop")]
        public List<IPAddress> NextHop { get; init; }

    }
}

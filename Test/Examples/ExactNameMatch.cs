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
using Bitvantage.SharpTextFsm;
using Bitvantage.SharpTextFsm.Attributes;
using Test.Generic.Mappings;

namespace Test.Examples
{
    internal record ShowIpArp : ITemplate
    {
        public string Protocol { get; set; }
        public IPAddress IpAddress { get; set; }
        [Variable(ThrowOnConversionFailure = false)]
        public long? Age { get; set; }
        public string MacAddress { get; set; }
        public string Type { get; set; }
        public string Interface { get; set; }

        string ITemplate.TextFsmTemplate =>
            """
            Value PROTOCOL (\S+)
            Value IP_ADDRESS (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})
            Value AGE (-|\d+)
            Value MAC_ADDRESS ([a-f0-9]{4}\.[a-f0-9]{4}\.[a-f0-9]{4})
            Value TYPE (\S+)
            Value INTERFACE (\S+)
            
            Start
             ^Protocol\s+Address\s+Age\(min\)\s+Hardware Addr\s+Type\s+Interface -> Entry
             ^.* -> Error
            
            Entry
             ^${PROTOCOL}\s+${IP_ADDRESS}\s+${AGE}\s+${MAC_ADDRESS}\s+${TYPE}(\s+${INTERFACE})?$$ -> Record
             ^.* -> Error
            """;

        [Test]
        public void Test()
        {
            var template = Template.FromType<ShowIpArp>();
            var data = """
                Protocol  Address              Age(min)       Hardware Addr     Type      Interface
                Internet  172.16.233.229       -              0000.0c59.f892    ARPA      Ethernet0/0
                Internet  172.16.233.218       -              0000.0c07.ac00    ARPA      Ethernet0/0
                Internet  172.16.233.19        -              0000.0c63.1300    ARPA      Ethernet0/0
                Internet  172.16.233.209       -              0000.0c36.6965    ARPA      Ethernet0/0
                Internet  172.16.168.11        -              0000.0c63.1300    ARPA      Ethernet0/0
                Internet  172.16.168.254       9              0000.0c36.6965    ARPA      Ethernet0/0
                Internet  10.0.0.0             -              aabb.cc03.8200    SRP-A
                """;

            var results = template.Run<ShowIpArp>(data);

            Assert.That(results.Count, Is.EqualTo(7));

        }
    }


}

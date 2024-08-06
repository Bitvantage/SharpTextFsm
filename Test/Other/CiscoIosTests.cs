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

using System.Diagnostics;
using Bitvantage.SharpTextFsm;

namespace Test.Other
{
    public class CiscoIosTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CiscoBgpSummary()
        {
            var templateText = """
                # Carry down the local end information so that it is present on each row item.
                Value Filldown RouterID (\S+)
                Value Filldown LocalAS (\d+)
                Value RemoteAS (\d+)
                Value Required RemoteIP (\d+(\.\d+){3})
                Value Uptime (\d+\S+)
                Value Received_V4 (\d+)
                Value Status (\D.*)

                Start
                  ^BGP router identifier ${RouterID}, local AS number ${LocalAS}
                  ^${RemoteIP}\s+\d+\s+${RemoteAS}(\s+\S+){5}\s+${Uptime}\s+${Received_V4} -> Record
                  ^${RemoteIP}\s+\d+\s+${RemoteAS}(\s+\S+){5}\s+${Uptime}\s+${Status} -> Record

                # Last record is already recorded then skip doing so here.
                EOF
                """;

            var data = """
                BGP router identifier 192.0.2.70, local AS number 65550
                BGP table version is 9, main routing table version 9
                4 network entries using 468 bytes of memory
                4 path entries using 208 bytes of memory
                3/2 BGP path/bestpath attribute entries using 420 bytes of memory
                1 BGP AS-PATH entries using 24 bytes of memory
                1 BGP community entries using 24 bytes of memory
                0 BGP route-map cache entries using 0 bytes of memory
                0 BGP filter-list cache entries using 0 bytes of memory
                BGP using 1144 total bytes of memory
                BGP activity 12/4 prefixes, 12/4 paths, scan interval 5 secs

                Neighbor        V    AS MsgRcvd MsgSent   TblVer  InQ OutQ Up/Down  State/PfxRcd
                192.0.2.77      4 65551    6965    1766        9    0    0  5w4d           1
                192.0.2.78      4 65552    6965    1766        9    0    0  5w4d          10
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }

        [Test]
        public void CiscoIpv6Interface()
        {
            var templateText = """
                Value Interface (\S+)
                Value Admin (\S+)
                Value Oper (\S+)
                Value Description (.*)
                Value LinkLocal (\S+)
                Value List Addresses (\S+)
                Value List Subnets (\S+)
                Value List GroupAddresses (\S+)
                Value Mtu (\d+)

                Start
                  ^${Interface} is ${Admin}, line protocol is ${Oper}
                  ^.*link-local address is ${LinkLocal}
                  ^  Description: ${Description}
                  ^  Global unicast address -> Unicast
                  ^  Joined group address -> Multicast
                  ^  MTU is ${Mtu} bytes -> Record

                Unicast
                  ^    ${Addresses}, subnet is ${Subnets}
                  ^  Joined group address -> Multicast
                  ^  \S -> Start

                Multicast
                  ^    ${GroupAddresses}
                  ^  MTU is ${Mtu} bytes -> Record
                  ^  \S -> Start
                
                EOF
                """;

            var data = """
                Dialer0 is up, line protocol is up
                  IPv6 is enabled, link-local address is FE80::21B:2BFF:FECE:4EE3 
                  No Virtual link-local address(es):
                  Description: PPP Dialer
                  Stateless address autoconfig enabled
                  General-prefix in use for addressing
                  Global unicast address(es):
                    2001:4567:1212:B2:21B:2BFF:FECE:4EE3, subnet is 2001:4567:1212:B2::/64 [EUI/CAL/PRE]
                      valid lifetime 5041 preferred lifetime 5041
                    2001:4567:1111:56FF::1, subnet is 2001:4567:1111:56FF::1/128 [CAL/PRE]
                      valid lifetime 5945 preferred lifetime 2344
                  Joined group address(es):
                    FF02::1
                    FF02::2
                    FF02::1:FF00:1
                    FF02::1:FFCE:4EE3
                  MTU is 1500 bytes
                  ICMP error messages limited to one every 100 milliseconds
                  ICMP redirects are enabled
                  ICMP unreachables are sent
                  Input features: Access List
                  Inbound access list IPV6-IN
                  ND DAD is enabled, number of DAD attempts: 1
                  ND reachable time is 30000 milliseconds (using 21397)
                  Hosts use stateless autoconfig for addresses.
                Vlan1 is up, line protocol is up
                  IPv6 is enabled, link-local address is FE80::21B:2BFF:FECE:4EE3 
                  No Virtual link-local address(es):
                  Description: Local VLAN
                  General-prefix in use for addressing
                  Global unicast address(es):
                    2001:4567:1212:5600::1, subnet is 2001:4567:1212:5600::/64 [CAL/PRE]
                      valid lifetime 5943 preferred lifetime 2342
                  Joined group address(es):
                    FF02::1
                    FF02::2
                    FF02::1:2
                    FF02::1:FF00:1
                    FF02::1:FFCE:4EE3
                    FF05::1:3
                  MTU is 1500 bytes
                  ICMP error messages limited to one every 100 milliseconds
                  ICMP redirects are enabled
                  ICMP unreachables are sent
                  ND DAD is enabled, number of DAD attempts: 1
                  ND reachable time is 30000 milliseconds (using 26371)
                  ND advertised reachable time is 0 (unspecified)
                  ND advertised retransmit interval is 0 (unspecified)
                  ND router advertisements are sent every 200 seconds
                  ND router advertisements live for 1800 seconds
                  ND advertised default router preference is Medium
                  Hosts use stateless autoconfig for addresses.
                  Hosts use DHCP to obtain other configuration.
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }

        [Test]
        public void CiscoVersion()
        {
            var templateText = """
                Value Model (\S+)
                Value Memory (\S+)
                Value ConfigRegister (0x\S+)
                Value Uptime (.*)
                Value Version (.*?)
                Value ReloadReason (.*)
                Value ReloadTime (.*)
                Value ImageFile ([^"]+)

                Start
                  ^Cisco IOS Software.*Version ${Version},
                  ^.*uptime is ${Uptime}
                  ^System returned to ROM by ${ReloadReason}
                  ^System restarted at ${ReloadTime}
                  ^System image file is "${ImageFile}"
                  ^cisco ${Model} .* with ${Memory} bytes of memory
                  ^Configuration register is ${ConfigRegister}
                """;

            var data = """
                Cisco IOS Software, Catalyst 4500 L3 Switch Software (cat4500-ENTSERVICESK9-M), Version 12.2(31)SGA1, RELEASE SOFTWARE (fc3)
                Technical Support: http://www.cisco.com/techsupport
                Copyright (c) 1986-2007 by Cisco Systems, Inc.
                Compiled Fri 26-Jan-07 14:28 by kellythw
                Image text-base: 0x10000000, data-base: 0x118AD800

                ROM: 12.2(31r)SGA
                Pod Revision 0, Force Revision 34, Gill Revision 20

                router.abc uptime is 3 days, 13 hours, 53 minutes
                System returned to ROM by reload
                System restarted at 05:09:09 PDT Wed Apr 2 2008
                System image file is "bootflash:cat4500-entservicesk9-mz.122-31.SGA1.bin"


                This product contains cryptographic features and is subject to United
                States and local country laws governing import, export, transfer and
                use. Delivery of Cisco cryptographic products does not imply
                third-party authority to import, export, distribute or use encryption.
                Importers, exporters, distributors and users are responsible for
                compliance with U.S. and local country laws. By using this product you
                agree to comply with applicable laws and regulations. If you are unable
                to comply with U.S. and local laws, return this product immediately.

                A summary of U.S. laws governing Cisco cryptographic products may be found at:
                http://www.cisco.com/wwl/export/crypto/tool/stqrg.html

                If you require further assistance please contact us by sending email to export@cisco.com.

                cisco WS-C4948-10GE (MPC8540) processor (revision 5) with 262144K bytes of memory.
                Processor board ID FOX111700ZP
                MPC8540 CPU at 667Mhz, Fixed Module
                Last reset from Reload
                2 Virtual Ethernet interfaces
                48 Gigabit Ethernet interfaces
                2 Ten Gigabit Ethernet interfaces
                511K bytes of non-volatile configuration memory.

                Configuration register is 0x2102
                
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }

        [Test]
        public void F10IpBgpSummary()
        {
            var templateText = """
                Value Filldown RouterID (\d+(\.\d+){3})
                Value Filldown LocalAS (\d+)
                Value RemoteAS (\d+)
                Value Required RemoteIP (\d+(\.\d+){3})
                Value Uptime (\S+)
                Value Received_V4 (\d+)
                Value Received_V6 ()
                Value Status (\D.*)

                Start
                  ^BGP router identifier ${RouterID}, local AS number ${LocalAS}
                  ^${RemoteIP}\s+${RemoteAS}(\s+\S+){5}\s+${Uptime}\s+${Received_V4} -> Next.Record
                  ^${RemoteIP}\s+${RemoteAS}(\s+\S+){5}\s+${Uptime}\s+${Status} -> Next.Record

                EOF
                """;

            var data = """
                BGP router identifier 192.0.2.1, local AS number 65551
                BGP table version is 173711, main routing table version 173711
                255 network entrie(s) using 43260 bytes of memory
                1114 paths using 75752 bytes of memory
                BGP-RIB over all using 76866 bytes of memory
                23 BGP path attribute entrie(s) using 1472 bytes of memory
                3 BGP AS-PATH entrie(s) using 137 bytes of memory
                10 BGP community entrie(s) using 498 bytes of memory
                2 BGP route-reflector cluster entrie(s) using 62 bytes of memory
                6 neighbor(s) using 28128 bytes of memory

                Neighbor        AS            MsgRcvd  MsgSent     TblVer  InQ  OutQ Up/Down  State/Pfx

                10.10.10.10     65551             647      397      73711    0   (0) 10:37:12         5
                10.10.100.1     65552             664      416      73711    0   (0) 10:38:27         0
                10.100.10.9     65553             709      526      73711    0   (0) 07:55:38         1
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }

        [Test]
        public void F10Version()
        {
            var templateText = """
                Value Chassis (\S+)
                Value Model (.*)
                Value Software (.*)
                Value Image ([^"]*)

                Start
                  ^Force10 Application Software Version: ${Software}
                  ^Chassis Type: ${Chassis} -> Continue
                  ^Chassis Type: ${Model}
                  ^System image file is "${Image}"
                """;

            var data = """
                Force10 Networks Real Time Operating System Software
                Force10 Operating System Version: 1.0
                Force10 Application Software Version: 7.7.1.1
                Copyright (c) 1999-2008 by Force10 Networks, Inc.
                Build Time: Fri Sep 12 14:08:26 PDT 2008
                Build Path: /sites/sjc/work/sw/build/special_build/Release/E7-7-1/SW/SRC
                router.abc uptime is 3 day(s), 2 hour(s), 3 minute(s)

                System image file is "flash://FTOS-EF-7.7.1.1.bin"

                Chassis Type: E1200
                Control Processor: IBM PowerPC 750FX (Rev D2.2) with 536870912 bytes of memory.
                Route Processor 1: IBM PowerPC 750FX (Rev D2.2) with 1073741824 bytes of memory.
                Route Processor 2: IBM PowerPC 750FX (Rev D2.2) with 1073741824 bytes of memory.

                128K bytes of non-volatile configuration memory.

                  1 Route Processor Module
                  9 Switch Fabric Module
                  1 48-port GE line card with SFP optics (EF)
                  7 4-port 10GE LAN/WAN PHY line card with XFP optics (EF)
                  1 FastEthernet/IEEE 802.3 interface(s)
                 48 GigabitEthernet/IEEE 802.3 interface(s)
                 28 Ten GigabitEthernet/IEEE 802.3 interface(s)
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }


        [Test]
        public void JuniperBgpSummary()
        {
            var templateText = """
                Value RemoteAS (\d+)
                Value RemoteIP (\S+)
                Value Uptime (.*[0-9h])
                Value Active_V4 (\d+)
                Value Received_V4 (\d+)
                Value Accepted_V4 (\d+)
                Value Damped_V4 (\d+)
                Value Active_V6 (\d+)
                Value Received_V6 (\d+)
                Value Accepted_V6 (\d+)
                Value Damped_V6 (\d+)
                Value Status ([a-zA-Z]+)

                Start
                  # New format IPv4 & IPv6 split across newlines.
                  ^\s+inet.0: ${Active_V4}/${Received_V4}/${Damped_V4}
                  ^\s+inet6.0: ${Active_V6}/${Received_V6}/${Damped_V6}
                  ^ -> Continue.Record
                  ^${RemoteIP}\s+${RemoteAS}(\s+\d+){4}\s+${Uptime}\s+${Status}
                  ^${RemoteIP}\s+${RemoteAS}(\s+\d+){4}\s+${Uptime}\s+${Active_V4}/${Received_V4}/${Damped_V4}\s+${Active_V6}/${Received_V6}/${Damped_V6} -> Next.Record
                  ^${RemoteIP}\s+${RemoteAS}(\s+\d+){4}\s+${Uptime}\s+${Active_V4}/${Received_V4}/${Accepted_V4}/${Damped_V4}\s+${Active_V6}/${Received_V6}/${Accepted_V6}/${Damped_V6} -> Next.Record
                  ^${RemoteIP}\s+${RemoteAS}(\s+\d+){4}\s+${Uptime}\s+${Status} -> Next.Record
                """;

            var data = """
                Groups: 3 Peers: 3 Down peers: 0
                Table          Tot Paths  Act Paths Suppressed    History Damp State    Pending
                inet.0               947        310          0          0          0          0
                inet6.0              849        807          0          0          0          0
                Peer                     AS      InPkt     OutPkt    OutQ   Flaps Last Up/Dwn State|#Active/Received/Damped...
                10.247.68.182         65550     131725   28179233       0      11     6w3d17h Establ
                  inet.0: 4/5/1
                  inet6.0: 0/0/0
                10.254.166.246        65550     136159   29104942       0       0      6w5d6h Establ
                  inet.0: 0/0/0
                  inet6.0: 7/8/1
                192.0.2.100           65551    1269381    1363320       0       1      9w5d6h 2/3/0 0/0/0
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }

        [Test]
        public void JuniperVersion()
        {
            var templateText = """
                Value Chassis (\S+)
                Value Required Model (\S+)
                Value Boot (.*)
                Value Base (.*)
                Value Kernel (.*)
                Value Crypto (.*)
                Value Documentation (.*)
                Value Routing (.*)

                Start
                # Support multiple chassis systems.
                  ^\S+:$$ -> Continue.Record
                  ^${Chassis}:$$
                  ^Model: ${Model}
                  ^JUNOS Base OS boot \[${Boot}\]
                  ^JUNOS Software Release \[${Base}\]
                  ^JUNOS Base OS Software Suite \[${Base}\]
                  ^JUNOS Kernel Software Suite \[${Kernel}\]
                  ^JUNOS Crypto Software Suite \[${Crypto}\]
                  ^JUNOS Online Documentation \[${Documentation}\]
                  ^JUNOS Routing Software Suite \[${Routing}\]
                """;

            var data = """
                Hostname: router.abc
                Model: mx960
                JUNOS Base OS boot [9.1S3.5]
                JUNOS Base OS Software Suite [9.1S3.5]
                JUNOS Kernel Software Suite [9.1S3.5]
                JUNOS Crypto Software Suite [9.1S3.5]
                JUNOS Packet Forwarding Engine Support (M/T Common) [9.1S3.5]
                JUNOS Packet Forwarding Engine Support (MX Common) [9.1S3.5]
                JUNOS Online Documentation [9.1S3.5]
                JUNOS Routing Software Suite [9.1S3.5]
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }

        [Test]
        public void UnixIfConfig()
        {
            var templateText = """
                Value Required Interface ([^:]+)
                Value MTU (\d+)
                Value State ((in)?active)
                Value MAC ([\d\w:]+)
                Value List Inet ([\d\.]+)
                Value List Netmask (\S+)
                # Don't match interface local (fe80::/10) - achieved with excluding '%'.
                Value List Inet6 ([^%]+)
                Value List Prefix (\d+)

                Start
                  # Record interface record (if we have one).
                  ^\S+:.* -> Continue.Record
                  # Collect data for new interface.
                  ^${Interface}:.* mtu ${MTU}
                  ^\s+ether ${MAC}
                  ^\s+inet6 ${Inet6} prefixlen ${Prefix}
                  ^\s+inet ${Inet} netmask ${Netmask}
                """;

            var data = """
                lo0: flags=8049<UP,LOOPBACK,RUNNING,MULTICAST> mtu 16384
                	inet6 ::1 prefixlen 128
                	inet6 fe80::1%lo0 prefixlen 64 scopeid 0x1
                	inet 127.0.0.1 netmask 0xff000000
                en0: flags=8863<UP,BROADCAST,SMART,RUNNING,SIMPLEX,MULTICAST> mtu 1500
                	ether 34:15:9e:27:45:e3
                	inet6 fe80::3615:9eff:fe27:45e3%en0 prefixlen 64 scopeid 0x4
                	inet6 2001:db8::3615:9eff:fe27:45e3 prefixlen 64 autoconf
                	inet6 999:db8::3615:9eff:fe27:45e3 prefixlen 64 autoconf
                	inet 192.0.2.215 netmask 0xfffffe00 broadcast 192.0.2.255
                	media: autoselect (1000baseT <full-duplex,flow-control>)
                	status: active
                en1: flags=8863<UP,BROADCAST,SMART,RUNNING,SIMPLEX,MULTICAST> mtu 1500
                	ether 90:84:0d:f6:d1:55
                	media: <unknown subtype> (<unknown type>)
                	status: inactive
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }

        [Test]
        public void ShowClock()
        {
            var templateText = """
                Value Time (..:..:..)
                Value Timezone (\S+)
                Value WeekDay (\w+)
                Value Month (\w+)
                Value MonthDay (\d+)
                Value Year (\d+)

                Start
                  ^${Time}.* ${Timezone} ${WeekDay} ${Month} ${MonthDay} ${Year} -> Record
                """;

            var data = """
                15:10:44.867 UTC Sun Nov 13 2016
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }

        [Test]
        public void CiscoIpInterfaceBrief()
        {
            var templateText = """
                Value INTF (\S+)
                Value ADDR (\S+)
                Value STATUS (up|down|administratively down)
                Value PROTO (up|down)

                Start
                  ^${INTF}\s+${ADDR}\s+\w+\s+\w+\s+${STATUS}\s+${PROTO} -> Record
                """;

            var data = """
                R1#show ip interface brief
                Interface                  IP-Address      OK? Method Status                Protocol
                FastEthernet0/0            15.0.15.1       YES manual up                    up
                FastEthernet0/1            10.0.12.1       YES manual up                    up
                FastEthernet0/2            10.0.13.1       YES manual up                    up
                FastEthernet0/3            unassigned      YES unset  up                    up
                Loopback0                  10.1.1.1        YES manual up                    up
                Loopback100                100.0.0.1       YES manual up                    up
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }

        [Test]
        public void CiscoShowCdpNeighborsDetail()
        {
            var templateText = """
                Value Filldown LOCAL_HOST (\S+)
                Value Required DEST_HOST (\S+)
                Value MGMNT_IP (.*)
                Value PLATFORM (.*)
                Value LOCAL_PORT (.*)
                Value REMOTE_PORT (.*)
                Value IOS_VERSION (\S+)

                Start
                  ^${LOCAL_HOST}[>#].
                  ^Device ID: ${DEST_HOST}
                  ^.*IP address: ${MGMNT_IP}
                  ^Platform: ${PLATFORM},
                  ^Interface: ${LOCAL_PORT},  Port ID \(outgoing port\): ${REMOTE_PORT}
                  ^.*Version ${IOS_VERSION}, -> Record
                """;

            var data = """
                SW1#show cdp neighbors detail
                -------------------------
                Device ID: SW2
                Entry address(es):
                  IP address: 10.1.1.2
                Platform: cisco WS-C2960-8TC-L,  Capabilities: Switch IGMP
                Interface: GigabitEthernet1/0/16,  Port ID (outgoing port): GigabitEthernet0/1
                Holdtime : 164 sec

                Version :
                Cisco IOS Software, C2960 Software (C2960-LANBASEK9-M), Version 12.2(55)SE9, RELEASE SOFTWARE (fc1)
                Technical Support: http://www.cisco.com/techsupport
                Copyright (c) 1986-2014 by Cisco Systems, Inc.
                Compiled Mon 03-Mar-14 22:53 by prod_rel_team

                advertisement version: 2
                VTP Management Domain: ''
                Native VLAN: 1
                Duplex: full
                Management address(es):
                  IP address: 10.1.1.2

                -------------------------
                Device ID: R1
                Entry address(es):
                  IP address: 10.1.1.1
                Platform: Cisco 3825,  Capabilities: Router Switch IGMP
                Interface: GigabitEthernet1/0/22,  Port ID (outgoing port): GigabitEthernet0/0
                Holdtime : 156 sec

                Version :
                Cisco IOS Software, 3800 Software (C3825-ADVENTERPRISEK9-M), Version 12.4(24)T1, RELEASE SOFTWARE (fc3)
                Technical Support: http://www.cisco.com/techsupport
                Copyright (c) 1986-2009 by Cisco Systems, Inc.
                Compiled Fri 19-Jun-09 18:40 by prod_rel_team

                advertisement version: 2
                VTP Management Domain: ''
                Duplex: full
                Management address(es):

                -------------------------
                Device ID: R2
                Entry address(es):
                  IP address: 10.2.2.2
                Platform: Cisco 2911,  Capabilities: Router Switch IGMP
                Interface: GigabitEthernet1/0/21,  Port ID (outgoing port): GigabitEthernet0/0
                Holdtime : 156 sec

                Version :
                Cisco IOS Software, 2900 Software (C3825-ADVENTERPRISEK9-M), Version 15.2(2)T1, RELEASE SOFTWARE (fc3)
                Technical Support: http://www.cisco.com/techsupport
                Copyright (c) 1986-2009 by Cisco Systems, Inc.
                Compiled Fri 19-Jun-09 18:40 by prod_rel_team

                advertisement version: 2
                VTP Management Domain: ''
                Duplex: full
                Management address(es):
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }

        [Test]
        public void CiscoShowIpRouteOspf()
        {
            var templateText = """
                Value network (\S+)
                Value mask (\d+)
                Value distance (\d+)
                Value metric (\d+)
                Value List nexthop (\S+)

                Start
                  ^O -> Continue.Record
                  ^O +${network}/${mask}\s\[${distance}/${metric}\]\svia\s${nexthop},
                  ^\s+\[${distance}/${metric}\]\svia\s${nexthop},
                """;

            var data = """
                R1#sh ip route ospf
                Codes: L - local, C - connected, S - static, R - RIP, M - mobile, B - BGP
                       D - EIGRP, EX - EIGRP external, O - OSPF, IA - OSPF inter area
                       N1 - OSPF NSSA external type 1, N2 - OSPF NSSA external type 2
                       E1 - OSPF external type 1, E2 - OSPF external type 2
                       i - IS-IS, su - IS-IS summary, L1 - IS-IS level-1, L2 - IS-IS level-2
                       ia - IS-IS inter area, * - candidate default, U - per-user static route
                       o - ODR, P - periodic downloaded static route, H - NHRP, l - LISP
                       + - replicated route, % - next hop override

                Gateway of last resort is not set

                      10.0.0.0/8 is variably subnetted, 10 subnets, 2 masks
                O        10.1.1.0/24 [110/20] via 10.0.12.2, 1w2d, Ethernet0/1
                O        10.2.2.0/24 [110/20] via 10.0.13.3, 1w2d, Ethernet0/2
                O        10.3.3.3/32 [110/11] via 10.0.12.2, 1w2d, Ethernet0/1
                O        10.4.4.4/32 [110/11] via 10.0.13.3, 1w2d, Ethernet0/2
                                     [110/11] via 10.0.14.4, 1w2d, Ethernet0/3
                O        10.5.5.5/32 [110/21] via 10.0.13.3, 1w2d, Ethernet0/2
                                     [110/21] via 10.0.12.2, 1w2d, Ethernet0/1
                                     [110/21] via 10.0.14.4, 1w2d, Ethernet0/3
                O        10.6.6.0/24 [110/20] via 10.0.13.3, 1w2d, Ethernet0/2
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }

        [Test]
        public void CiscoShowVersion01()
        {
            var data = """
                Cisco IOS XE Software, Version 16.12.07
                Cisco IOS Software [Gibraltar], Catalyst L3 Switch Software (CAT3K_CAA-UNIVERSALK9-M), Version 16.12.7, RELEASE SOFTWARE (fc2)
                Technical Support: http://www.cisco.com/techsupport
                Copyright (c) 1986-2022 by Cisco Systems, Inc.
                Compiled Wed 02-Feb-22 07:28 by mcpre


                Cisco IOS-XE software, Copyright (c) 2005-2022 by cisco Systems, Inc.
                All rights reserved.  Certain components of Cisco IOS-XE software are
                licensed under the GNU General Public License ("GPL") Version 2.0.  The
                software code licensed under GPL Version 2.0 is free software that comes
                with ABSOLUTELY NO WARRANTY.  You can redistribute and/or modify such
                GPL code under the terms of GPL Version 2.0.  For more details, see the
                documentation or "License Notice" file accompanying the IOS-XE software,
                or the applicable URL provided on the flyer accompanying the IOS-XE
                software.


                ROM: IOS-XE ROMMON
                BOOTLDR: CAT3K_CAA Boot Loader (CAT3K_CAA-HBOOT-M) Version 4.66, RELEASE SOFTWARE (P)

                Switch1 uptime is 28 weeks, 5 hours, 15 minutes
                Uptime for this control processor is 28 weeks, 5 hours, 20 minutes
                System returned to ROM by Power Failure or Unknown at 06:15:00 PST Fri Feb 3 2023
                System restarted at 06:21:25 PST Fri Feb 3 2023
                System image file is "flash:packages.conf"
                Last reload reason: Power Failure or Unknown



                This product contains cryptographic features and is subject to United
                States and local country laws governing import, export, transfer and
                use. Delivery of Cisco cryptographic products does not imply
                third-party authority to import, export, distribute or use encryption.
                Importers, exporters, distributors and users are responsible for
                compliance with U.S. and local country laws. By using this product you
                agree to comply with applicable laws and regulations. If you are unable
                to comply with U.S. and local laws, return this product immediately.

                A summary of U.S. laws governing Cisco cryptographic products may be found at:
                http://www.cisco.com/wwl/export/crypto/tool/stqrg.html

                If you require further assistance please contact us by sending email to
                export@cisco.com.


                Technology Package License Information: 

                ------------------------------------------------------------------------------
                Technology-package                                     Technology-package
                Current                        Type                       Next reboot  
                ------------------------------------------------------------------------------
                ipservicesk9        	Smart License                 	 ipservicesk9        
                None                	Subscription Smart License    	 None                          


                Smart Licensing Status: REGISTERED/OUT OF COMPLIANCE

                cisco WS-C3650-48PD (MIPS) processor (revision K0) with 794816K/6147K bytes of memory.
                Processor board ID FDO1934E2AF
                21 Virtual Ethernet interfaces
                100 Gigabit Ethernet interfaces
                4 Ten Gigabit Ethernet interfaces
                2048K bytes of non-volatile configuration memory.
                4194304K bytes of physical memory.
                257008K bytes of Crash Files at crashinfo:.
                257008K bytes of Crash Files at crashinfo-2:.
                1550272K bytes of Flash at flash:.
                1550272K bytes of Flash at flash-2:.
                0K bytes of WebUI ODM Files at webui:.

                Base Ethernet MAC Address          : 04:62:73:67:05:00
                Motherboard Assembly Number        : 73-15897-04
                Motherboard Serial Number          : FDO19341D4N
                Model Revision Number              : K0
                Motherboard Revision Number        : B0
                Model Number                       : WS-C3650-48PD
                System Serial Number               : FDO1934E2AF


                Switch Ports Model              SW Version        SW Image              Mode   
                ------ ----- -----              ----------        ----------            ----   
                *    1 52    WS-C3650-48PD      16.12.07          CAT3K_CAA-UNIVERSALK9 INSTALL
                     2 52    WS-C3650-48PD      16.12.07          CAT3K_CAA-UNIVERSALK9 INSTALL


                Switch 02
                ---------
                Switch uptime                      : 28 weeks, 5 hours, 19 minutes 

                Base Ethernet MAC Address          : 04:62:73:7e:8a:80
                Motherboard Assembly Number        : 73-15897-04
                Motherboard Serial Number          : FDO19341CU2
                Model Revision Number              : K0
                Motherboard Revision Number        : B0
                Model Number                       : WS-C3650-48PD
                System Serial Number               : FDO1934P1AR
                Last reload reason                 : code upgrade

                Configuration register is 0x102
                
                """;

            var template = Template.FromType<CiscoShowVersion>();

            var r = template.Run(data);
        }

        [Test]
        public void CiscoShowIpRouteOspf02()
        {
            var templateText = """
                Value network (\S+)
                Value mask (\d+)
                Value distance (\d+)
                Value metric (\d+)
                Value List nexthop (\S+)

                Start
                  ^O -> Continue.Record
                  ^O +${network}/${mask}\s\[${distance}/${metric}\]\svia\s${nexthop},
                  ^\s+\[${distance}/${metric}\]\svia\s${nexthop},
                """;

            var data = """
                R1#sh ip route ospf
                Codes: L - local, C - connected, S - static, R - RIP, M - mobile, B - BGP
                       D - EIGRP, EX - EIGRP external, O - OSPF, IA - OSPF inter area
                       N1 - OSPF NSSA external type 1, N2 - OSPF NSSA external type 2
                       E1 - OSPF external type 1, E2 - OSPF external type 2
                       i - IS-IS, su - IS-IS summary, L1 - IS-IS level-1, L2 - IS-IS level-2
                       ia - IS-IS inter area, * - candidate default, U - per-user static route
                       o - ODR, P - periodic downloaded static route, H - NHRP, l - LISP
                       + - replicated route, % - next hop override

                Gateway of last resort is not set

                      10.0.0.0/8 is variably subnetted, 10 subnets, 2 masks
                O        10.1.1.0/24 [110/20] via 10.0.12.2, 1w2d, Ethernet0/1
                O        10.2.2.0/24 [110/20] via 10.0.13.3, 1w2d, Ethernet0/2
                O        10.3.3.3/32 [110/11] via 10.0.12.2, 1w2d, Ethernet0/1
                O        10.4.4.4/32 [110/11] via 10.0.13.3, 1w2d, Ethernet0/2
                                     [110/11] via 10.0.14.4, 1w2d, Ethernet0/3
                O        10.5.5.5/32 [110/21] via 10.0.13.3, 1w2d, Ethernet0/2
                                     [110/21] via 10.0.12.2, 1w2d, Ethernet0/1
                                     [110/21] via 10.0.14.4, 1w2d, Ethernet0/3
                O        10.6.6.0/24 [110/20] via 10.0.13.3, 1w2d, Ethernet0/2
                """;

            var template = new Template(templateText);

            var sw = Stopwatch.StartNew();
            var values = template.Run<CiscoShowIpRouteOspfRecord>(data);

            sw.Stop();
            Assert.Pass();
        }


        [Test]
        public void CiscoShowEtherchannelSummary()
        {
            var templateText = """
                Value CHANNEL (\S+)
                Value List MEMBERS (\w+\d+\/\d+)
                Start
                  ^\d+.* -> Continue.Record
                  ^\d+ +${CHANNEL}\(\S+ +[\w-]+ +[\w ]+ +${MEMBERS}\( -> Continue
                  ^\d+ +${CHANNEL}\(\S+ +[\w-]+ +[\w ]+ +\S+ +${MEMBERS}\( -> Continue
                  ^\d+ +${CHANNEL}\(\S+ +[\w-]+ +[\w ]+ +(\S+ +){2} +${MEMBERS}\( -> Continue
                  ^ +${MEMBERS} -> Continue
                  ^ +\S+ +${MEMBERS} -> Continue
                  ^ +(\S+ +){2} +${MEMBERS} -> Continue
                  ^ +(\S+ +){3} +${MEMBERS} -> Continue
                """;

            var data = """
                sw1# sh etherchannel summary
                Flags:  D - down        P - bundled in port-channel
                        I - stand-alone s - suspended
                        H - Hot-standby (LACP only)
                        R - Layer3      S - Layer2
                        U - in use      f - failed to allocate aggregator

                        M - not in use, minimum links not met
                        u - unsuitable for bundling
                        w - waiting to be aggregated
                        d - default port


                Number of channel-groups in use: 2
                Number of aggregators:           2

                Group  Port-channel  Protocol    Ports
                ------+-------------+-----------+-----------------------------------------------
                1      Po1(SU)         LACP      Fa0/1(P)   Fa0/2(P)   Fa0/3(P)
                3      Po3(SU)          -        Fa0/11(P)   Fa0/12(P)   Fa0/13(P)   Fa0/14(P)
                                                 Fa0/15(P)   Fa0/16(P)
                """;

            var template = new Template(templateText);

            var values = template.Run(data);

            Assert.Pass();
        }


    }
}

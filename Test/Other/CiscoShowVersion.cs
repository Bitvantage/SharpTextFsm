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

using Bitvantage.SharpTextFSM;
using Bitvantage.SharpTextFSM.Attributes;

namespace Test.Other
{
    internal record CiscoShowVersion : ITemplate
    {
        public string TextFsmTemplate => """
            Value SOFTWARE_IMAGE (\S+)
            Value VERSION (.+?)
            Value RELEASE (\S+)
            Value ROMMON (\S+)
            Value HOSTNAME (\S+)
            Value UPTIME (.+)
            Value UPTIME_YEARS (\d+)
            Value UPTIME_WEEKS (\d+)
            Value UPTIME_DAYS (\d+)
            Value UPTIME_HOURS (\d+)
            Value UPTIME_MINUTES (\d+)
            Value RELOAD_REASON (.+?)
            Value RUNNING_IMAGE (\S+)
            Value List HARDWARE (\S+|\S+\d\S+)
            Value List SERIAL (\S+)
            Value CONFIG_REGISTER (\S+)
            Value List MAC_ADDRESS ([0-9a-fA-F]{2}(:[0-9a-fA-F]{2}){5})
            Value RESTARTED (.+)
            
            Start
              ^.*Software,*\s+\(${SOFTWARE_IMAGE}\),\sVersion\s${VERSION},*\s+RELEASE.*\(${RELEASE}\)
              ^.*Software,*\s+\(${SOFTWARE_IMAGE}\),\sVersion\s${VERSION},
              ^ROM:\s+${ROMMON}
              ^\s*${HOSTNAME}\s+uptime\s+is\s+${UPTIME} -> Continue
              ^.*\s+uptime\s+is.*\s+${UPTIME_YEARS}\syear -> Continue
              ^.*\s+uptime\s+is.*\s+${UPTIME_WEEKS}\sweek -> Continue
              ^.*\s+uptime\s+is.*\s+${UPTIME_DAYS}\sday -> Continue
              ^.*\s+uptime\s+is.*\s+${UPTIME_HOURS}\shour -> Continue
              ^.*\s+uptime\s+is.*\s+${UPTIME_MINUTES}\sminute
              ^[sS]ystem\s+image\s+file\s+is\s+"(.*?):${RUNNING_IMAGE}"
              ^(?:[lL]ast\s+reload\s+reason:|System\s+returned\s+to\s+ROM\s+by)\s+${RELOAD_REASON}\s*$$
              ^[Pp]rocessor\s+board\s+ID\s+${SERIAL}
              ^[Cc]isco\s+${HARDWARE}\s+\(.+\).+
              ^[Cc]onfiguration\s+register\s+is\s+${CONFIG_REGISTER}
              ^Base\s+[Ee]thernet\s+MAC\s+[Aa]ddress\s+:\s+${MAC_ADDRESS}
              ^System\s+restarted\s+at\s+${RESTARTED}$$
              ^Switch\s+Port -> Stack
              # Capture time-stamp if vty line has command time-stamping turned on
              ^Switch\s\d+ -> Stack
              ^Load\s+for\s+
              ^Time\s+source\s+is
            
            Stack
              ^[Ss]ystem\s+[Ss]erial\s+[Nn]umber\s+:\s+${SERIAL}
              ^[Mm]odel\s+[Nn]umber\s+:\s+${HARDWARE}\s*
              ^[Cc]onfiguration\s+register\s+is\s+${CONFIG_REGISTER}
              ^Base [Ee]thernet MAC [Aa]ddress\s+:\s+${MAC_ADDRESS}
            """;

        [TemplateVariable(Name = "VERSION")]
        public string Version { get; init; }
    }
}

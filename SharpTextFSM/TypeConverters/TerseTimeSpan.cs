using System.Text.RegularExpressions;

namespace Bitvantage.SharpTextFSM.TypeConverters
{
    public class TerseTimeSpanConverter : ValueConverter<TimeSpan>
    {
        private static readonly Regex Pattern = new("""
            ^
            (
                (
                    ((?<years>\d+)\ years?(,\ |$))?
                    ((?<weeks>\d+)\ weeks?(,\ |$))?
                    ((?<days>\d+)\ days?(,\ |$))?
                    ((?<hours>\d+)\ hours?(,\ |$))?
                    ((?<minutes>\d+)\ minutes?(,\ |$))?
                    ((?<seconds>\d+)\ seconds?)?
                )|
                (
                    ((?<years>\d+)y)?
                    ((?<weeks>\d+)w)?
                    ((?<days>\d+)d)?
                    ((?<hours>\d+)h)?
                    ((?<minutes>\d+)m)?
                    ((?<seconds>\d+)s)?
                )
            )
            $
            """, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        public override bool TryConvert(string text, out TimeSpan value)
        {
            var match = Pattern.Match(text);

            // failed to match
            if (!match.Success)
            {
                value = TimeSpan.Zero;
                return false;
            }

            // at least one group must match
            if (match.Groups.Count <= 1)
            {
                value = TimeSpan.Zero;
                return false;
            }

            // matched groups are optional; but must be consecutive
            var matched = false; // is set on the first match
            var matchDone = false; // is set on the first non-match after a match
            for (var groupIndex = 1; groupIndex < match.Groups.Count; groupIndex++)
            {
                if (match.Groups[groupIndex].Success)
                {
                    if (matchDone)
                    {
                        value = TimeSpan.Zero;
                        return false;
                    }

                    matched = true;
                }
                else if (matched == true)
                {
                    matchDone = true;
                }
            }

            var years = 0;
            var weeks = 0;
            var days = 0;
            var hours = 0;
            var minutes = 0;
            var seconds = 0;

            if (match.Groups["years"].Success)
                years = int.Parse(match.Groups["years"].Value);

            if (match.Groups["weeks"].Success)
                weeks = int.Parse(match.Groups["weeks"].Value);

            if (match.Groups["days"].Success)
                days = int.Parse(match.Groups["days"].Value);

            if (match.Groups["hours"].Success)
                hours = int.Parse(match.Groups["hours"].Value);

            if (match.Groups["minutes"].Success)
                minutes = int.Parse(match.Groups["minutes"].Value);

            if (match.Groups["seconds"].Success)
                seconds = int.Parse(match.Groups["seconds"].Value);

            value = new TimeSpan(years * 365 + weeks * 7 + days, hours, minutes, seconds);
            return true;
        }
    }
}

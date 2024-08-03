// /*
//    Bitvantage.SharpTextFsm 2024 Michael Crino
// 
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
// 
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// */

using BenchmarkDotNet.Attributes;
using Bitvantage.SharpTextFsm;

namespace PerformanceTest.MeteoriteLanding;

public class MeteoriteLandingBenchmark
{
    private static readonly string MeteoriteLandingData;
    private static readonly Template Template;

    static MeteoriteLandingBenchmark()
    {
        // loads NASA meteorite landings sample data
        // original data from: https://data.nasa.gov/api/views/gh4g-9sfh/rows.csv?accessType=DOWNLOAD
        MeteoriteLandingData = File.ReadAllText(@".\MeteoriteLanding\MeteoriteLandingsData.txt");
        Template = Template.FromType<MeteoriteLandingRecord>();
    }

    [IterationCount(20)]
    [Benchmark]
    public void MeteoriteLandingTyped()
    {
        var records = Template.Parse<MeteoriteLandingRecord>(MeteoriteLandingData).ToList();
    }

    [IterationCount(20)]
    [Benchmark]
    public void MeteoriteLandingRowCollection()
    {
        var records = Template.Parse(MeteoriteLandingData).ToList();
    }


}
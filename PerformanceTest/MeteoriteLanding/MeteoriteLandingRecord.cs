using System.Diagnostics.CodeAnalysis;
using Bitvantage.SharpTextFsm;

namespace PerformanceTest.MeteoriteLanding
{
    internal class MeteoriteLandingRecord : ITemplate
    {
        public string TextFsmTemplate => """
            Value NAME (.*)
            Value ID (\d+)
            Value TYPE (Valid|Relict)
            Value List CLASS (.*?)
            Value MASS (${_NUMBER})
            Value FALL (Found|Fell)
            Value YEAR (\d+)
            Value LATITUDE (${_NUMBER})
            Value LONGITUDE (${_NUMBER})
            Value GEO_LOCATION (${_NUMBER}, ${_NUMBER})
            
            ~Global
             ^[^\t] -> Continue.Record Start
            
            Start
             ^${NAME} -> Data
             ^.* -> Error
            
            Data
             ^\tId: ${ID}$$
             ^\tType: ${TYPE}$$
             ^\tClass: ${CLASS}(, ${CLASS})*$$
             ^\tMass: ${MASS}$$
             ^\tMass: $$
             ^\tFall: ${FALL}$$
             ^\tYear: ${YEAR}$$
             ^\tYear: $$
             ^\tLatitude: ${LATITUDE}$$
             ^\tLatitude: $$
             ^\tLongitude: ${LONGITUDE}$$
             ^\tLongitude: $$
             ^\tGeo Location: \(${GEO_LOCATION}\)$$
             ^\tGeo Location: $$
             ^.* -> Error
            """;

        public enum RecordType
        {
            Valid,
            Relict,
        }

        public enum FallType
        {
            Found,
            Fell,
        }

        public record GeoLocationRecord(float Latitude, float Longitude)
        {
            public static bool TryParse(string value, [NotNullWhen(true)] out GeoLocationRecord geoLocationRecord)
            {
                var values = value.Split(", ");
                if (values.Length != 2)
                {
                    geoLocationRecord = null;
                    return false;
                }

                if (values.Length != 2 || !float.TryParse(values[0], out var latitude) || !float.TryParse(values[1], out var longitude))
                {
                    geoLocationRecord = null;
                    return false;
                }

                geoLocationRecord = new GeoLocationRecord(latitude, longitude);
                return true;

            }
        }

        public string Name { get; set; }
        public int Id { get; set; }
        public RecordType Type { get; set; }
        public string[] Class { get; set; }
        public double? Mass { get; set; }
        public FallType Fall { get; set; }
        public int? Year { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public GeoLocationRecord? GeoLocation { get; set; }

    }
}

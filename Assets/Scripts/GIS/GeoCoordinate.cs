using System;
using UnityEngine;

namespace Osm4x.GIS
{
    [Serializable]
    public struct GeoCoordinate
    {
        public double Latitude;
        public double Longitude;
        public double ElevationMeters;

        public GeoCoordinate(double latitude, double longitude, double elevationMeters = 0)
        {
            Latitude = latitude;
            Longitude = longitude;
            ElevationMeters = elevationMeters;
        }

        public static GeoCoordinate ArlingtonVa => new(38.8816, -77.0910, 0);

        public override string ToString() => $"{Latitude:F5}, {Longitude:F5} ({ElevationMeters:F0}m)";
    }
}

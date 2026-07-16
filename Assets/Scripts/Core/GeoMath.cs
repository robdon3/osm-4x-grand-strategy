using System.Runtime.CompilerServices;
using UnityEngine;

namespace Osm4x.Core
{
    /// <summary>
    /// GPS to Cartesian helpers for strategic globe and chunk placement.
    /// Uses a simple spherical Earth model; swap for ArcGIS surface placement
    /// when the map component is the authority for world positions.
    /// </summary>
    public static class GeoMath
    {
        public const double EarthRadiusMeters = 6_371_000.0;

        /// <summary>WGS84 degrees to unit-sphere direction (Y-up Unity).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 LatLonToUnitSphere(double latitudeDeg, double longitudeDeg)
        {
            double lat = latitudeDeg * Mathf.Deg2Rad;
            double lon = longitudeDeg * Mathf.Deg2Rad;
            double cosLat = System.Math.Cos(lat);
            float x = (float)(cosLat * System.Math.Sin(lon));
            float y = (float)System.Math.Sin(lat);
            float z = (float)(cosLat * System.Math.Cos(lon));
            return new Vector3(x, y, z);
        }

        public static Vector3 LatLonToWorld(double latitudeDeg, double longitudeDeg, float globeRadius)
        {
            return LatLonToUnitSphere(latitudeDeg, longitudeDeg) * globeRadius;
        }

        public static void WorldToLatLon(Vector3 worldOnSphere, float globeRadius, out double latitudeDeg, out double longitudeDeg)
        {
            Vector3 n = worldOnSphere.normalized;
            latitudeDeg = System.Math.Asin(Mathf.Clamp(n.y, -1f, 1f)) * Mathf.Rad2Deg;
            longitudeDeg = System.Math.Atan2(n.x, n.z) * Mathf.Rad2Deg;
        }

        public static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
        {
            double rlat1 = lat1 * System.Math.PI / 180.0;
            double rlat2 = lat2 * System.Math.PI / 180.0;
            double dLat = (lat2 - lat1) * System.Math.PI / 180.0;
            double dLon = (lon2 - lon1) * System.Math.PI / 180.0;
            double a = System.Math.Sin(dLat / 2) * System.Math.Sin(dLat / 2) +
                       System.Math.Cos(rlat1) * System.Math.Cos(rlat2) *
                       System.Math.Sin(dLon / 2) * System.Math.Sin(dLon / 2);
            double c = 2 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1 - a));
            return EarthRadiusMeters * c;
        }

        public static float ApproximateAltitudeMeters(Vector3 cameraWorld, float globeRadiusMeters)
        {
            return cameraWorld.magnitude - globeRadiusMeters;
        }
    }
}

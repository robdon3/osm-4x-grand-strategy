using Osm4x.Core;
using UnityEngine;

namespace Osm4x.WorldGen
{
    /// <summary>
    /// Deterministic layered noise. Pure functions of world XZ + WorldConfig seed offsets.
    /// </summary>
    public static class NoiseSampler
    {
        public static float Sample01(float worldX, float worldZ, float scale, Vector2 offset)
        {
            float x = (worldX + offset.x) * scale;
            float z = (worldZ + offset.y) * scale;
            return Mathf.PerlinNoise(x, z);
        }

        public static float Continental(float worldX, float worldZ, WorldConfig cfg)
        {
            float n = Sample01(worldX, worldZ, cfg.ContinentalScale, cfg.NoiseOffsetA);
            float n2 = Sample01(worldX, worldZ, cfg.ContinentalScale * 2.1f, cfg.NoiseOffsetB);
            return Mathf.Clamp01(n * 0.7f + n2 * 0.3f);
        }

        public static float Elevation(float worldX, float worldZ, WorldConfig cfg)
        {
            float e1 = Sample01(worldX, worldZ, cfg.ElevationScale, cfg.NoiseOffsetA);
            float e2 = Sample01(worldX, worldZ, cfg.ElevationScale * 2.3f, cfg.NoiseOffsetB);
            float detail = Sample01(worldX, worldZ, cfg.DetailScale, cfg.NoiseOffsetC);
            return Mathf.Clamp01(e1 * 0.55f + e2 * 0.3f + detail * 0.15f);
        }

        public static float Temperature(float worldX, float worldZ, WorldConfig cfg)
        {
            float band = Mathf.InverseLerp(-2000f, 2000f, worldZ);
            float n = Sample01(worldX, worldZ, cfg.TemperatureScale, cfg.NoiseOffsetB);
            return Mathf.Clamp01(band * 0.65f + n * 0.35f);
        }

        public static float Moisture(float worldX, float worldZ, WorldConfig cfg)
        {
            return Sample01(worldX, worldZ, cfg.MoistureScale, cfg.NoiseOffsetC);
        }

        public static float SurfaceHeight(float worldX, float worldZ, WorldConfig cfg)
        {
            float continental = Continental(worldX, worldZ, cfg);
            float elev = Elevation(worldX, worldZ, cfg);

            if (continental < cfg.SeaLevel)
            {
                float depth = (cfg.SeaLevel - continental) / Mathf.Max(0.001f, cfg.SeaLevel);
                return cfg.BaseHeight * 0.25f - depth * 3f;
            }

            float land = (continental - cfg.SeaLevel) / Mathf.Max(0.001f, 1f - cfg.SeaLevel);
            float h = cfg.BaseHeight + land * cfg.HeightAmplitude * (0.35f + elev * 0.65f);
            return h;
        }

        public static bool IsLand(float worldX, float worldZ, WorldConfig cfg)
        {
            return Continental(worldX, worldZ, cfg) >= cfg.SeaLevel;
        }
    }
}

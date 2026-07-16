using Osm4x.Core;
using UnityEngine;

namespace Osm4x.WorldGen
{
    public static class BiomeClassifier
    {
        public static BiomeType Classify(float worldX, float worldZ, WorldConfig cfg)
        {
            float continental = NoiseSampler.Continental(worldX, worldZ, cfg);
            if (continental < cfg.SeaLevel)
                return BiomeType.Ocean;

            if (continental < cfg.SeaLevel + 0.03f)
                return BiomeType.Beach;

            float elev = NoiseSampler.Elevation(worldX, worldZ, cfg);
            float temp = NoiseSampler.Temperature(worldX, worldZ, cfg);
            float moist = NoiseSampler.Moisture(worldX, worldZ, cfg);

            if (elev > 0.78f)
                return BiomeType.Mountains;

            if (temp < 0.22f)
                return moist > 0.45f ? BiomeType.Snow : BiomeType.Tundra;

            if (temp > 0.7f && moist < 0.3f)
                return BiomeType.Desert;

            if (temp > 0.55f && moist < 0.45f)
                return BiomeType.Savanna;

            if (moist > 0.7f && temp > 0.4f && elev < 0.45f)
                return BiomeType.Swamp;

            if (moist > 0.5f)
                return BiomeType.Forest;

            return BiomeType.Plains;
        }

        public static Color ColorFor(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Ocean => new Color(0.12f, 0.28f, 0.55f),
                BiomeType.Beach => new Color(0.86f, 0.80f, 0.55f),
                BiomeType.Plains => new Color(0.45f, 0.70f, 0.30f),
                BiomeType.Forest => new Color(0.15f, 0.45f, 0.18f),
                BiomeType.Desert => new Color(0.85f, 0.75f, 0.40f),
                BiomeType.Savanna => new Color(0.70f, 0.65f, 0.30f),
                BiomeType.Tundra => new Color(0.55f, 0.60f, 0.55f),
                BiomeType.Snow => new Color(0.92f, 0.94f, 0.96f),
                BiomeType.Mountains => new Color(0.45f, 0.42f, 0.40f),
                BiomeType.Swamp => new Color(0.25f, 0.40f, 0.28f),
                _ => Color.magenta
            };
        }
    }
}

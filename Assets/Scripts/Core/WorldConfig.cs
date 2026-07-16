using UnityEngine;

namespace Osm4x.Core
{
    /// <summary>
    /// Global procedural world settings. One instance per game.
    /// Same seed always produces the same terrain.
    /// </summary>
    public sealed class WorldConfig : MonoBehaviour
    {
        public static WorldConfig Instance { get; private set; }

        [Header("Identity")]
        [SerializeField] private int seed = 42;

        [Header("Chunk grid")]
        [SerializeField] private int chunkSize = 16;
        [SerializeField] private float cellSize = 1f;

        [Header("Height")]
        [Range(0f, 1f)]
        [SerializeField] private float seaLevel = 0.42f;
        [SerializeField] private float heightAmplitude = 24f;
        [SerializeField] private float baseHeight = 4f;

        [Header("Noise scales (world units)")]
        [SerializeField] private float continentalScale = 0.003f;
        [SerializeField] private float elevationScale = 0.012f;
        [SerializeField] private float detailScale = 0.05f;
        [SerializeField] private float temperatureScale = 0.004f;
        [SerializeField] private float moistureScale = 0.006f;

        public int Seed => seed;
        public int ChunkSize => Mathf.Max(4, chunkSize);
        public float CellSize => Mathf.Max(0.1f, cellSize);
        public float SeaLevel => seaLevel;
        public float HeightAmplitude => heightAmplitude;
        public float BaseHeight => baseHeight;
        public float ContinentalScale => continentalScale;
        public float ElevationScale => elevationScale;
        public float DetailScale => detailScale;
        public float TemperatureScale => temperatureScale;
        public float MoistureScale => moistureScale;

        public Vector2 NoiseOffsetA { get; private set; }
        public Vector2 NoiseOffsetB { get; private set; }
        public Vector2 NoiseOffsetC { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ApplySeed(seed);
        }

        public void ApplySeed(int newSeed)
        {
            seed = newSeed;
            var rng = new System.Random(seed);
            NoiseOffsetA = new Vector2(rng.Next(-100000, 100000), rng.Next(-100000, 100000));
            NoiseOffsetB = new Vector2(rng.Next(-100000, 100000), rng.Next(-100000, 100000));
            NoiseOffsetC = new Vector2(rng.Next(-100000, 100000), rng.Next(-100000, 100000));
        }

        public void RandomizeSeed()
        {
            ApplySeed(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        }
    }
}

using System.Collections.Generic;
using Osm4x.Core;
using UnityEngine;

namespace Osm4x.Chunks
{
    /// <summary>
    /// Minecraft-style streaming: keep a square of chunks loaded around the focus.
    /// </summary>
    public sealed class ChunkManager : MonoBehaviour
    {
        [SerializeField] private int loadRadius = 3;
        [SerializeField] private Transform chunkRoot;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private float refreshInterval = 0.25f;

        private readonly Dictionary<ChunkCoord, MapChunk> _loaded = new();
        private ChunkCoord _center;
        private float _nextRefresh;
        private Material _runtimeMaterial;

        private void Start()
        {
            if (chunkRoot == null)
                chunkRoot = transform;

            if (terrainMaterial == null)
            {
                var shader = Shader.Find("Proc4x/VertexColorUnlit")
                             ?? Shader.Find("Sprites/Default")
                             ?? Shader.Find("Universal Render Pipeline/Unlit");
                _runtimeMaterial = new Material(shader);
            }
            else
            {
                _runtimeMaterial = terrainMaterial;
            }

            ForceRefresh();
        }

        private void Update()
        {
            if (Time.time < _nextRefresh) return;
            _nextRefresh = Time.time + refreshInterval;

            Vector3 focus = ResolveFocus();
            if (GameModeController.Instance != null)
                GameModeController.Instance.SetFocus(focus);

            UpdateCenter(focus);
        }

        private Vector3 ResolveFocus()
        {
            if (followTarget != null)
                return followTarget.position;
            if (Camera.main != null)
                return Camera.main.transform.position;
            return GameModeController.Instance != null
                ? GameModeController.Instance.FocusWorld
                : Vector3.zero;
        }

        public void ForceRefresh()
        {
            UpdateCenter(ResolveFocus(), force: true);
        }

        public void UpdateCenter(Vector3 world, bool force = false)
        {
            var cfg = WorldConfig.Instance;
            if (cfg == null)
            {
                Debug.LogWarning("[Proc4x] WorldConfig missing — add it to the scene.");
                return;
            }

            var newCenter = ChunkCoord.FromWorld(world, cfg);
            if (!force && newCenter.Equals(_center) && _loaded.Count > 0)
                return;

            _center = newCenter;
            SyncWindow(cfg);
        }

        private void SyncWindow(WorldConfig cfg)
        {
            var needed = new HashSet<ChunkCoord>();
            for (int dz = -loadRadius; dz <= loadRadius; dz++)
            for (int dx = -loadRadius; dx <= loadRadius; dx++)
                needed.Add(new ChunkCoord(_center.X + dx, _center.Z + dz));

            var toRemove = new List<ChunkCoord>();
            foreach (var kv in _loaded)
            {
                if (!needed.Contains(kv.Key))
                    toRemove.Add(kv.Key);
            }
            foreach (var c in toRemove)
            {
                _loaded[c].Unload();
                _loaded.Remove(c);
            }

            foreach (var c in needed)
            {
                if (_loaded.ContainsKey(c)) continue;
                _loaded[c] = SpawnChunk(c, cfg);
            }
        }

        private MapChunk SpawnChunk(ChunkCoord coord, WorldConfig cfg)
        {
            var go = new GameObject(coord.ToString());
            go.transform.SetParent(chunkRoot, false);
            var chunk = go.AddComponent<MapChunk>();
            var mesh = ChunkMeshBuilder.Build(coord, cfg);
            chunk.Initialize(coord, mesh, _runtimeMaterial);
            return chunk;
        }

        private void OnDestroy()
        {
            foreach (var kv in _loaded)
            {
                if (kv.Value != null)
                    Destroy(kv.Value.gameObject);
            }
            _loaded.Clear();
        }
    }
}

using System.Collections.Generic;
using Osm4x.Core;
using UnityEngine;

namespace Osm4x.Chunks
{
    /// <summary>
    /// Maintains a (2*radius+1)^2 window of chunks around the current focus.
    /// Default 5x5 with 0.5 degree tiles for Phase 2 proof of concept.
    /// </summary>
    public sealed class ChunkManager : MonoBehaviour
    {
        [SerializeField] private double degreesPerChunk = 0.5;
        [SerializeField] private int loadRadius = 2;
        [SerializeField] private MapChunk chunkPrefab;
        [SerializeField] private Transform chunkRoot;

        private readonly Dictionary<ChunkCoord, MapChunk> _loaded = new();
        private ChunkCoord _center;

        private void OnEnable()
        {
            if (GameModeController.Instance != null)
                GameModeController.Instance.OnModeChanged += HandleModeChanged;
        }

        private void OnDisable()
        {
            if (GameModeController.Instance != null)
                GameModeController.Instance.OnModeChanged -= HandleModeChanged;
        }

        private void Start()
        {
            if (chunkRoot == null)
                chunkRoot = transform;
            RefreshAroundFocus();
        }

        private void HandleModeChanged(GameMode prev, GameMode next)
        {
            if (next == GameMode.Tactical || next == GameMode.Transition)
                RefreshAroundFocus();
        }

        public void RefreshAroundFocus()
        {
            var gm = GameModeController.Instance;
            if (gm == null) return;
            UpdateCenter(gm.FocusLatitude, gm.FocusLongitude);
        }

        public void UpdateCenter(double latitude, double longitude)
        {
            var newCenter = ChunkCoord.FromLatLon(latitude, longitude, degreesPerChunk);
            if (newCenter.Equals(_center) && _loaded.Count > 0) return;
            _center = newCenter;
            SyncWindow();
        }

        private void SyncWindow()
        {
            var needed = new HashSet<ChunkCoord>();
            for (int dy = -loadRadius; dy <= loadRadius; dy++)
            for (int dx = -loadRadius; dx <= loadRadius; dx++)
                needed.Add(new ChunkCoord(_center.X + dx, _center.Y + dy));

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
                var chunk = SpawnChunk(c);
                _loaded[c] = chunk;
            }

            Debug.Log($"[Osm4x] Chunk window center={_center} loaded={_loaded.Count}");
        }

        private MapChunk SpawnChunk(ChunkCoord coord)
        {
            MapChunk chunk;
            if (chunkPrefab != null)
                chunk = Instantiate(chunkPrefab, chunkRoot);
            else
            {
                var go = new GameObject(coord.ToString(), typeof(MapChunk));
                go.transform.SetParent(chunkRoot, false);
                chunk = go.GetComponent<MapChunk>();
            }

            chunk.Initialize(coord);
            chunk.MarkLoaded();
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

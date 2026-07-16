using UnityEngine;

namespace Osm4x.Chunks
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class MapChunk : MonoBehaviour
    {
        public ChunkCoord Coord { get; private set; }
        public bool IsLoaded { get; private set; }

        private MeshFilter _filter;
        private MeshCollider _collider;

        private void Awake()
        {
            _filter = GetComponent<MeshFilter>();
            _collider = GetComponent<MeshCollider>();
        }

        public void Initialize(ChunkCoord coord, Mesh mesh, Material material)
        {
            Coord = coord;
            name = coord.ToString();
            if (_filter == null) _filter = gameObject.AddComponent<MeshFilter>();
            var renderer = GetComponent<MeshRenderer>();
            if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();

            _filter.sharedMesh = mesh;
            if (material != null)
                renderer.sharedMaterial = material;

            if (_collider == null)
                _collider = gameObject.AddComponent<MeshCollider>();
            _collider.sharedMesh = mesh;

            IsLoaded = true;
        }

        public void Unload()
        {
            IsLoaded = false;
            if (_filter != null && _filter.sharedMesh != null)
            {
                Destroy(_filter.sharedMesh);
                _filter.sharedMesh = null;
            }
            Destroy(gameObject);
        }
    }
}

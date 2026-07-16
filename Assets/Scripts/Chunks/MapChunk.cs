using UnityEngine;

namespace Osm4x.Chunks
{
    /// <summary>
    /// Runtime handle for one lat/long tile. Expand with Addressables load of
    /// terrain mesh + vector buildings when assets exist.
    /// </summary>
    public sealed class MapChunk : MonoBehaviour
    {
        public ChunkCoord Coord { get; private set; }
        public bool IsLoaded { get; private set; }

        public void Initialize(ChunkCoord coord)
        {
            Coord = coord;
            name = coord.ToString();
        }

        public void MarkLoaded()
        {
            IsLoaded = true;
        }

        public void Unload()
        {
            IsLoaded = false;
            Destroy(gameObject);
        }
    }
}

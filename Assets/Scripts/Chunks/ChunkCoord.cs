using System;

namespace Osm4x.Chunks
{
    /// <summary>
    /// Discrete lat/long tile index. Chunk size defaults to 0.5 degrees.
    /// </summary>
    public readonly struct ChunkCoord : IEquatable<ChunkCoord>
    {
        public readonly int X;
        public readonly int Y;

        public ChunkCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static ChunkCoord FromLatLon(double latitude, double longitude, double degreesPerChunk)
        {
            int y = (int)Math.Floor(latitude / degreesPerChunk);
            int x = (int)Math.Floor(longitude / degreesPerChunk);
            return new ChunkCoord(x, y);
        }

        public void GetBounds(double degreesPerChunk, out double minLat, out double minLon, out double maxLat, out double maxLon)
        {
            minLat = Y * degreesPerChunk;
            minLon = X * degreesPerChunk;
            maxLat = minLat + degreesPerChunk;
            maxLon = minLon + degreesPerChunk;
        }

        public bool Equals(ChunkCoord other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is ChunkCoord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"Chunk({X},{Y})";
    }
}

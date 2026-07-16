using System;
using Osm4x.Core;
using UnityEngine;

namespace Osm4x.Chunks
{
    /// <summary>Integer chunk grid coordinate (Minecraft-style XZ).</summary>
    public readonly struct ChunkCoord : IEquatable<ChunkCoord>
    {
        public readonly int X;
        public readonly int Z;

        public ChunkCoord(int x, int z)
        {
            X = x;
            Z = z;
        }

        public static ChunkCoord FromWorld(Vector3 world, WorldConfig cfg)
        {
            float size = cfg.ChunkSize * cfg.CellSize;
            int x = Mathf.FloorToInt(world.x / size);
            int z = Mathf.FloorToInt(world.z / size);
            return new ChunkCoord(x, z);
        }

        public Vector3 OriginWorld(WorldConfig cfg)
        {
            float size = cfg.ChunkSize * cfg.CellSize;
            return new Vector3(X * size, 0f, Z * size);
        }

        public bool Equals(ChunkCoord other) => X == other.X && Z == other.Z;
        public override bool Equals(object obj) => obj is ChunkCoord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Z);
        public override string ToString() => $"Chunk({X},{Z})";
    }
}

using Osm4x.Core;
using Osm4x.WorldGen;
using UnityEngine;

namespace Osm4x.Chunks
{
    /// <summary>
    /// Builds a heightmap grid mesh for one chunk from the procedural sampler.
    /// </summary>
    public static class ChunkMeshBuilder
    {
        public static Mesh Build(ChunkCoord coord, WorldConfig cfg)
        {
            int n = cfg.ChunkSize;
            float cell = cfg.CellSize;
            Vector3 origin = coord.OriginWorld(cfg);

            int vertsPerSide = n + 1;
            var vertices = new Vector3[vertsPerSide * vertsPerSide];
            var colors = new Color[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[n * n * 6];

            for (int iz = 0; iz < vertsPerSide; iz++)
            for (int ix = 0; ix < vertsPerSide; ix++)
            {
                float wx = origin.x + ix * cell;
                float wz = origin.z + iz * cell;
                float h = NoiseSampler.SurfaceHeight(wx, wz, cfg);
                int vi = iz * vertsPerSide + ix;
                vertices[vi] = new Vector3(wx, h, wz);
                var biome = BiomeClassifier.Classify(wx, wz, cfg);
                colors[vi] = BiomeClassifier.ColorFor(biome);
                uvs[vi] = new Vector2(ix / (float)n, iz / (float)n);
            }

            int ti = 0;
            for (int iz = 0; iz < n; iz++)
            for (int ix = 0; ix < n; ix++)
            {
                int i0 = iz * vertsPerSide + ix;
                int i1 = i0 + 1;
                int i2 = i0 + vertsPerSide;
                int i3 = i2 + 1;
                triangles[ti++] = i0;
                triangles[ti++] = i2;
                triangles[ti++] = i1;
                triangles[ti++] = i1;
                triangles[ti++] = i2;
                triangles[ti++] = i3;
            }

            var mesh = new Mesh
            {
                name = coord.ToString(),
                indexFormat = vertices.Length > 65000
                    ? UnityEngine.Rendering.IndexFormat.UInt32
                    : UnityEngine.Rendering.IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}

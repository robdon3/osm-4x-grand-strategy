# Architecture — Procedural 4X (Minecraft-like)

Target: **macOS M4, 16GB RAM** · Peak play budget **< 8GB**.

## Design goal

A **generative map**, not a real one:

- World defined by a single **seed**
- Deterministic noise → height, moisture, temperature, biomes
- **Chunked streaming** (Minecraft pattern) so the world can be huge without loading everything

## Layers

| Layer | Scale | Representation |
|-------|--------|----------------|
| **Strategic** | Continent / empire | Hex or tile overlay, biome colors, city markers, fog of war |
| **Transition** | Zoom fly-in | Camera dive into a strategic cell / chunk |
| **Tactical** | Battlefield | Higher-res height mesh (or voxels), NavMesh, units |

Strategic and tactical share the **same seed and biome rules** so a mountain on the map is a mountain in battle.

## Generation pipeline

1. **WorldConfig** — seed, sea level, chunk size, amplitude.
2. **NoiseSampler** — layered Perlin (later: OpenSimplex / Burst jobs).
3. **BiomeClassifier** — temperature × moisture → biome.
4. **ChunkBuilder** — sample height + biome → mesh.
5. **ChunkManager** — load/unload chunks around focus (Minecraft-style).

## Chunk model

- Integer grid `(cx, cz)`, not lat/lon.
- Default **16×16** columns per chunk.
- Heightmap first; full voxels optional later.

## Non-goals (MVP)

- Real Earth data
- Full Minecraft creative editing

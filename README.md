# Procedural 4X Grand Strategy

**Minecraft-style generative world** + 4X grand strategy for macOS (Apple Silicon / M4, 16GB target).

No real maps. No OSM. No ArcGIS. Every world is **seed-driven noise** — continents, biomes, height, and tactical terrain all come from the same generator.

| | |
|---|---|
| **Engine** | Unity 6.x LTS (URP) |
| **World** | Procedural (layered noise + biomes + chunks) |
| **Platform** | macOS Apple Silicon (Metal) |
| **Language** | C# · Burst/Jobs-ready hot paths |
| **Repo** | https://github.com/robdon3/osm-4x-grand-strategy |

---

## Vision

- **Strategic layer** — abstract hex/tile map over a generated planet: continents, oceans, biomes, resources. Civ-style free camera.
- **Tactical layer** — same seed, higher detail: chunked terrain (heightfield/voxel-style) for battles, cover, and movement.
- **Key enabler** — infinite (or huge finite) **chunk streaming** like Minecraft: only keep a window of chunks loaded around the focus.

## How generation works

```
WorldSeed
   │
   ├─ Continental noise  → land vs ocean
   ├─ Height noise       → mountains / plains
   ├─ Temperature noise  → poles ↔ equator band
   └─ Moisture noise     → desert / forest / swamp
         │
         ▼
      Biome table  → surface type + colors + resources
         │
         ▼
   Chunk mesh (16×16 or 32×32 columns)
```

Same seed + same chunk coords always produce the same terrain (deterministic, multiplayer-safe later).

## Repo layout

```
docs/                     Architecture, roadmap, setup
Assets/Scripts/
  Core/                   Seed, game modes
  WorldGen/               Noise, biomes, height sampling
  Chunks/                 Chunk coords, mesh build, stream manager
  Camera/                 Strategic orbit + tactical zoom
  Strategy/               Hex overlay
  Tactical/               Battle bootstrap
```

## Quick start

1. Install **Unity Hub** + **Unity 6.x LTS** (Apple Silicon).
2. Create a **3D (URP)** project and copy `Assets/Scripts/` into it.
3. Empty scene → add empty `GameSystems` with:
   - `WorldConfig` (set seed)
   - `ChunkManager`
   - `GameModeController`
   - Camera + `StrategicCameraController`
4. Press Play — chunks should stream around the origin.

See [docs/SETUP.md](docs/SETUP.md) and [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Status

**Pivoted off real-world GIS** → pure procedural generation (Minecraft-like).

## License

MIT — see [LICENSE](LICENSE).

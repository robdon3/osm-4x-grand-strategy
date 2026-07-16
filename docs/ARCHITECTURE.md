<<<<<<< HEAD
# Architecture — Procedural 4X (local Python)

**No Unity.** Runs on macOS with Python + pygame.

## Goal

A **generative map** (Minecraft-style), not a real one:

- Single **seed** defines the world
- Deterministic noise → height, moisture, temperature, biomes
- **Chunk streaming** so only nearby terrain is generated

## Stack

| Piece | Choice |
|-------|--------|
| Language | Python 3 |
| Window / input | pygame (SDL) |
| Arrays / noise | numpy |
| Platform | macOS Apple Silicon |

## Pipeline

```
WorldConfig(seed)
    → WorldSampler (perlin noise layers)
    → height + biome per cell
    → ChunkCache (pygame surfaces)
    → camera pans; chunks load around view
```

## Modules

- `proc4x/world.py` — config, noise, biomes, height
- `proc4x/app.py` — game loop, camera, HUD, rendering

## Future (optional)

- True 3D heightmesh (moderngl) without Unity
- Hex strategic overlay for 4X systems
- Fog of war, cities, turn loop
=======
# Architecture — OSM 4X Grand Strategy

Target: **macOS M4 Silicon, 16GB RAM** · Peak play budget **&lt; 8GB**.

## Visual model: seamless strategic ↔ tactical

| Layer | Altitude / scale | Content |
|-------|------------------|---------|
| **Strategic** | Global / continental | Abstracted hex/tiles on OSM basemap, city markers, simplified roads, elevation tint, low-detail shaders |
| **Transition** | Cinematic fly-in | Cinemachine spline + DOF + vignette to a GPS focus |
| **Tactical** | City / battlefield | Extruded OSM buildings (height tags), road splines, water, elevation mesh, NavMesh, units |

**Enabler:** Dynamic LOD + **chunk-based detail swap**. Around ~10 km altitude, swap to full OSM vector features + procedural foliage. Battles use physics/NavMesh on real terrain (roads/buildings as cover).

## Technical stack (2026)

| Concern | Choice | Notes |
|---------|--------|-------|
| Engine | **Unity 6.x LTS** (URP) | Native Metal, MetalFX upscale path |
| Language | C# + Burst/Jobs | Geo transforms, pathfinding hot paths |
| Primary GIS | **ArcGIS Maps SDK for Unity** (v2.2+) | OSM 3D buildings, elevation, globe, streaming |
| Fallback GIS | Online Maps (Infinity Code) | Lighter OSM tiles/globe/offline cache |
| Avoid | Mapbox Unity SDK v2 | Maintenance-only; v3 delayed |
| Offline OSM | libosmium / OSMSharp plugins | PBF parse; Overpass for dev queries |
| Prebaked 3D | OSM2World → glTF | Optional building bake pipeline |
| Streaming | Addressables + async | Chunk prefabs |

## Chunking & memory

- **Chunk size:** 0.5°–1° lat/long (~50–110 km side).
- **Contents per chunk:** elevation mesh, vector buildings/roads (combined mesh or instances), LOD groups.
- **Load window:** 5×5 around focus; unload outer ring; prioritize frustum + distance.
- **Hybrid data:** OSM vectors for truth (roads, cities); Perlin/procedural for forests/fill.
- **RAM mitigations:** aggressive unload, GPU instancing, mesh compression, texture atlases, Metal residency only for active chunks.

```
                ┌─────────────────────────┐
                │   StrategicCamera       │
                │   (orbit + zoom alt)    │
                └───────────┬─────────────┘
                            │ altitude / mode
                ┌───────────▼─────────────┐
                │   GameModeController    │
                │ Strategic | Transition  │
                │         | Tactical      │
                └───────────┬─────────────┘
            ┌───────────────┼───────────────┐
            ▼               ▼               ▼
     ArcGIS Map        ChunkManager    TacticalBattle
     (globe/stream)    (5×5 grid)      (NavMesh/units)
```

## Coordinate pipeline

1. **GPS** `(lat, lon, elev)` — source of truth for placement and saves.
2. **GeoMath** — spherical / ECEF helpers (Burst-ready); ArcGIS also provides surface placement when the map component is active.
3. **ChunkCoord** — quantize lat/lon to tile indices for load/unload.

## Performance targets

- 60 FPS strategic orbit on M4 (MetalFX OK for tactical fill rate).
- Chunk swap without hitch &gt; 100 ms (async Addressables).
- Peak RAM &lt; 8GB with 5×5 tactical-ready window in one metro region.

## Licensing notes

- **OSM:** ODbL attribution required in UI/credits.
- **ArcGIS Maps SDK:** free for development; commercial production needs Esri licensing review.
- **Game code in this repo:** MIT.
>>>>>>> 50e71d0 (Phase 0: architecture docs and Unity C# scaffold for OSM 4X grand strategy)

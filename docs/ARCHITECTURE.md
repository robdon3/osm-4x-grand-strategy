# Architecture — OSM 4X Grand Strategy

Target: **macOS M4 Silicon, 16GB RAM** · Peak play budget **< 8GB**.

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

## Coordinate pipeline

1. **GPS** `(lat, lon, elev)` — source of truth for placement and saves.
2. **GeoMath** — spherical / ECEF helpers (Burst-ready); ArcGIS also provides surface placement when the map component is active.
3. **ChunkCoord** — quantize lat/lon to tile indices for load/unload.

## Performance targets

- 60 FPS strategic orbit on M4 (MetalFX OK for tactical fill rate).
- Chunk swap without hitch > 100 ms (async Addressables).
- Peak RAM < 8GB with 5×5 tactical-ready window in one metro region.

## Licensing notes

- **OSM:** ODbL attribution required in UI/credits.
- **ArcGIS Maps SDK:** free for development; commercial production needs Esri licensing review.
- **Game code in this repo:** MIT.

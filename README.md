# OSM 4X Grand Strategy

**OSM-powered 4X grand strategy** for macOS (Apple Silicon / M4, 16GB RAM target).

Seamless **strategic globe ↔ tactical battle** zoom over real-world OpenStreetMap data, built on **Unity 6 + Metal**.

| | |
|---|---|
| **Engine** | Unity 6.x LTS (URP) |
| **GIS** | ArcGIS Maps SDK for Unity (primary) · Online Maps fallback |
| **Platform** | macOS Apple Silicon (Metal) |
| **Language** | C# · Burst/Jobs for geo math |
| **Repo** | https://github.com/robdon3/osm-4x-grand-strategy |

---

## Vision

- **Strategic layer** — 3D globe, abstracted tiles/hexes on OSM basemaps, major cities, simplified roads, elevation tint. Civ-style free orbit.
- **Tactical transition** — Fly-in to real GPS coords; extruded OSM buildings, road splines, water, elevation terrain. Total War–style battles on NavMesh chunks.
- **Key enabler** — Dynamic LOD + lat/long chunk streaming so global exploration stays fluid and battles load rich local detail under **~8GB peak RAM**.

## Repo layout

```
docs/                  Architecture, roadmap, Unity setup
Assets/Scripts/
  Core/                Geo math, game modes
  Camera/              Strategic orbit + cinematic tactical transition
  Chunks/              Lat/long tile load/unload manager
  GIS/                 ArcGIS bootstrap stubs, coordinates
  Strategy/            Hex overlay helpers
  Tactical/            Battle bootstrap / NavMesh hooks
```

This repo ships **production-oriented C# prototypes** you drop into a Unity 6 project. Full Unity project files (Library/, etc.) are not committed — generate them locally via Unity Hub (see [docs/SETUP.md](docs/SETUP.md)).

## Quick start

1. Install **Unity Hub** + **Unity 6.x LTS** (Apple Silicon editor).
2. Create a new **3D (URP)** project, then copy `Assets/Scripts/` into it.
3. Import **ArcGIS Maps SDK for Unity** (Esri free dev account).
4. Follow [docs/SETUP.md](docs/SETUP.md) and Phase 0 in [docs/ROADMAP.md](docs/ROADMAP.md).

## Documentation

- [Architecture](docs/ARCHITECTURE.md) — stack, LOD, chunking, RAM budget
- [Roadmap](docs/ROADMAP.md) — Phase 0 → MVP map (3–6 weeks solo)
- [Setup](docs/SETUP.md) — Unity + ArcGIS on macOS

## Status

**Phase 0 kickoff** — architecture locked, core scripts scaffolded, GitHub project live.

## License

MIT — see [LICENSE](LICENSE). OSM data remains under [ODbL](https://www.openstreetmap.org/copyright); ArcGIS SDK usage is subject to Esri terms.

<<<<<<< HEAD
# Setup — local Mac (no Unity)

## Prerequisites

- macOS
- Python 3.11+ (`python3 --version`)

```bash
# if needed:
brew install python
```

## Install & run

```bash
cd ~/osm-4x-grand-strategy
chmod +x run.sh
./run.sh
```

Or:

```bash
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
python -m proc4x.app 42
```

## Troubleshooting

**Black window / no display**  
- You’re on a machine with a GUI session (not pure SSH without display).

**`ModuleNotFoundError: pygame`**  
```bash
source .venv/bin/activate
pip install -r requirements.txt
```

**No on-screen text**  
Some pygame 3.14 wheels ship without the font module. Map still works; HUD falls back to color bars.

## Not used

- Unity
- ArcGIS / Mapbox / OSM
- Network (fully offline)
=======
# Setup — macOS Apple Silicon

## 1. Unity

1. Install [Unity Hub](https://unity.com/download).
2. Install **Unity 6.x LTS** with modules:
   - Mac Build Support (Apple Silicon)
   - Documentation (optional)
3. New project → **3D (URP)** → name e.g. `Osm4x`.

## 2. Import this repo’s scripts

```bash
git clone https://github.com/robdon3/osm-4x-grand-strategy.git
# Copy scripts into your Unity project:
cp -R osm-4x-grand-strategy/Assets/Scripts /path/to/Osm4x/Assets/
```

Or open this folder as a Unity project once you add `ProjectSettings` via Hub “Add project from disk” after creating a blank URP project and merging folders.

## 3. ArcGIS Maps SDK for Unity

1. Create a free account at [Esri Developers](https://developers.arcgis.com/).
2. Generate an **API key** with location services / basemaps enabled.
3. Install **ArcGIS Maps SDK for Unity** from the Asset Store or Esri docs (v2.2+ recommended for OSM 3D buildings).
4. Add API key via the SDK’s authentication / map component settings (do **not** commit keys — use `Secrets/` gitignored or UserSettings).

## 4. Recommended packages

Via Package Manager:

- **Cinemachine** (camera orbits + transitions)
- **AI Navigation** (NavMesh for tactical)
- **Burst** + **Collections** + **Mathematics** (Jobs path later)

## 5. Minimal scene checklist

1. Empty scene `StrategicGlobe`.
2. ArcGIS Map component — globe mode, OSM basemap, elevation, 3D buildings layer.
3. Main Camera + `StrategicCameraController`.
4. Empty `GameSystems` object with `GameModeController` + `ChunkManager`.
5. Optional: Cinemachine FreeLook; bind zoom to altitude thresholds that call `GameModeController.SetMode`.

## 6. OSM sample data (Phase 1)

- [Geofabrik](https://download.geofabrik.de/) — small region `.osm.pbf`
- Store under `Data/OSM/` (gitignored)
- Dev queries: [Overpass Turbo](https://overpass-turbo.eu/)

## 7. Fallback without ArcGIS

If ArcGIS is blocked, evaluate **Online Maps** (Infinity Code) for OSM tiles/globe/offline cache and keep the same `GeoMath` / `ChunkManager` interfaces.

## Hardware notes (M4, 16GB)

- Prefer URP over HDRP for RAM/GPU headroom.
- Enable Metal Graphics Jobs in Player Settings when stable.
- Profile early with Memory Profiler when enabling 3D buildings at city scale.
>>>>>>> 50e71d0 (Phase 0: architecture docs and Unity C# scaffold for OSM 4X grand strategy)

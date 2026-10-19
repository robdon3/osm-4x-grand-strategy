# Roadmap

Status of this tree. No delivery date is attached.

## Runnable prototype

- [x] Seeded noise, height, and biomes (`proc4x/world.py`)
- [x] 12×6 turn loop: territory, one army per side, orthogonal land movement, combat, enemy turn (`proc4x/game.py`)
- [x] pygame window, cursor, and input (`proc4x/app.py`)
- [x] `./run.sh` creates `.venv`, installs `requirements.txt`, and runs `python -m proc4x.app`

The pygame board is a fixed 12×6 sample. Chunk streaming is not part of that app.

## Unity Phase 0 scaffold

These files are in the repo. They are not a playable grand strategy game, and they do not load OpenStreetMap or ArcGIS.

- [x] `WorldConfig`, Perlin sampling, and biome colors
- [x] XZ chunk window, height meshes, and unload
- [x] Strategic pan/zoom camera
- [x] `Strategic` / `Transition` / `Tactical` mode switch and a camera lerp into `Tactical`
- [x] `HexGridOverlay` visibility toggle (no hex mesh)
- [x] Placeholder capsules or a prefab when `Tactical` starts
- [x] `Proc4x/VertexColorUnlit` shader

## Not in this repo

- [ ] Unity project (`ProjectSettings`, scenes, package manifest)
- [ ] OpenStreetMap loading
- [ ] ArcGIS Maps SDK, or any `Assets/Scripts/GIS/` code
- [ ] Latitude/longitude or GPS coordinates (`ChunkCoord` is world XZ)
- [ ] Hex mesh, NavMesh, pathfinding, or combat in the C# tactical script

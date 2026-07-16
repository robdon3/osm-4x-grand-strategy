<<<<<<< HEAD
# Roadmap — local procedural map

## Done

- [x] Drop Unity / real maps
- [x] Python world gen (seed, noise, biomes)
- [x] pygame viewer with chunk streaming
- [x] `./run.sh` one-command launch on Mac

## Next

- [ ] Strategic hex overlay
- [ ] Place city markers on land
- [ ] Simple fog of war
- [ ] Optional 3D terrain view (moderngl) still no Unity

## Later 4X

- [ ] Turns, resources, armies
- [ ] Save/load by seed + game state
=======
# Roadmap — Oneshot to basic 3D globe prototype

Solo MVP map target: **3–6 weeks**.

## Phase 0 — Setup (Day 0–3) ✅ scaffolded in repo

- [x] GitHub repository + architecture docs
- [x] Core C# prototypes (geo, camera, chunks, mode)
- [ ] Install Unity Hub + Unity 6.x LTS (Apple Silicon)
- [ ] Create URP 3D project; import `Assets/Scripts`
- [ ] Import ArcGIS Maps SDK; free Esri signup
- [ ] Scene: ArcGIS Map, globe projection, OSM basemap + 3D buildings
- [ ] Cinemachine FreeLook / orbit + zoom bound to altitude

## Phase 1 — Basic globe with OSM (Day 4–10)

- [ ] Small OSM extract (e.g. Arlington, VA via Geofabrik) for offline tests
- [ ] Layer stacking: terrain + OSM vectors via Map Creator / SDK
- [ ] Wire `GeoMath` + ArcGIS surface placement
- [ ] Hex/tile overlay (`HexGridOverlay`) on strategic view
- [ ] Flyover test: global → city scale without hard hitches

## Phase 2 — Chunking proof + tactical teaser (Week 2–3)

- [ ] `ChunkManager` 5×5 load window with unload
- [ ] Battle-start input → `TacticalTransitionController` fly-in to GPS
- [ ] Detailed chunk: building extrude / OSM height tags
- [ ] Placeholder units on roads + simple NavMesh bake per chunk

## Phase 3 — Polish & expand (ongoing)

- [ ] Procedural foliage/fill in tagged areas
- [ ] Profile on M4: Unity Profiler + Metal GPU capture
- [ ] Fog of war, city markers, basic 4X UI shell
- [ ] Addressables packaging for chunk prefabs

## Non-goals (MVP)

- Full economy / diplomacy / multiplayer
- Global offline entire-planet mesh
- Mapbox dependency

## Definition of done — “MVP map”

1. Orbit a 3D globe with real basemap.
2. Zoom/fly to a real lat/lon city.
3. See extruded buildings / roads at tactical scale.
4. Chunk stream stays under ~8GB RAM in profiler.
>>>>>>> 50e71d0 (Phase 0: architecture docs and Unity C# scaffold for OSM 4X grand strategy)

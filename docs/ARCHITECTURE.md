# Architecture

Two layers live in this repo. The runnable one is a Python and pygame skirmish on a fixed 12×6 board. The Unity C# under `Assets/Scripts/` is a Phase 0 scaffold for a later OpenStreetMap / ArcGIS grand strategy game. The scaffold does not load map data, and this repo has no Unity project.

## Runnable prototype

`proc4x/app.py` builds one 12×6 board and draws it. It does not stream chunks or pan a camera across a larger world. `WorldConfig.chunk_size` is unused by the app.

```
WorldConfig(seed)
    → WorldSampler (layered noise)
    → height + biome on a 12×6 grid
    → GameState (ownership, armies, turns)
    → pygame window
```

`Board` in `app.py` overrides the sampler scales and uses sea level 0.38. Cell centers are spaced 48 world units apart. The same seed rebuilds the same grid.

| Module | Role |
|--------|------|
| `proc4x/world.py` | Seed offsets, permutation-table noise, height, temperature, moisture, biomes |
| `proc4x/game.py` | Capitals, territory, orthogonal land movement, combat, enemy turn |
| `proc4x/app.py` | Window, input, ownership tint, reachable overlay, armies |

Biomes are ocean, beach, plains, forest, desert, savanna, tundra, snow, mountains, and swamp. Ocean is not walkable. Each side has one army (10 HP, 1 orthogonal move). Moving claims land. Stepping onto the other army removes it, costs the attacker 3 HP (never below 1), and ends the move. Pressing **E** runs the enemy once: it steps toward your army and prefers an attack. Win, loss, and draw depend on which armies remain.

Dependencies are `pygame` and `numpy`. The app opens a window and does not use the network.

## Unity Phase 0 scaffold

Namespaces are `Osm4x.*`. There are no scenes, `ProjectSettings`, or a package manifest. Nothing references the ArcGIS SDK or reads OpenStreetMap data. There is no `Assets/Scripts/GIS/` directory and no `GeoMath` type. Chunk coordinates are a local XZ grid, not latitude and longitude. `GameModeController` stores a world XZ focus, not GPS.

```
WorldConfig (seed, noise scales, chunk size, cell size)
    → NoiseSampler + BiomeClassifier
    → ChunkMeshBuilder (colored height mesh)
    → ChunkManager (square window around the camera)
    → GameModeController (Strategic | Transition | Tactical)
```

| Path | Behavior in the script |
|------|------------------------|
| `Core/WorldConfig.cs` | One singleton. `ApplySeed` derives three noise offsets from the seed. Defaults include chunk size 16, cell size 1, sea level 0.42. |
| `Core/GameModeController.cs` | Modes `Strategic`, `Transition`, `Tactical`. `BeginBattleAt` stores a world position and enters `Transition`. |
| `WorldGen/NoiseSampler.cs` | `Mathf.PerlinNoise` layers for continental, elevation, temperature, moisture, and surface height. |
| `WorldGen/BiomeClassifier.cs` | Same biome list as the Python sampler, with vertex colors. |
| `Chunks/ChunkCoord.cs` | `Floor` of world X/Z divided by chunk size times cell size. |
| `Chunks/ChunkManager.cs` | Keeps a square of chunks around the follow target or the main camera. Default `loadRadius` is 3, refreshed every 0.25s. Missing `WorldConfig` logs a warning and skips the build. |
| `Chunks/ChunkMeshBuilder.cs` | One height mesh per chunk, vertex colors from the biome classifier. |
| `Chunks/MapChunk.cs` | Mesh filter, renderer, and mesh collider. `Unload` destroys the mesh and the object. |
| `Camera/StrategicCameraController.cs` | Pans on the Horizontal/Vertical axes, speeds up with Left Shift, zooms with the mouse wheel, pitch fixed at 50°. No input during `Transition`. Height is clamped from 8 to 400. |
| `Camera/TacticalTransitionController.cs` | On `Transition`, lerps the camera to the focus (default height 25, duration 1.8s) and then enters `Tactical`. In the editor or a development build, **B** starts that fly-in at the camera and **N** returns to `Strategic`. |
| `Strategy/HexGridOverlay.cs` | Activates its own object in `Strategic` and deactivates it otherwise. It does not build a hex mesh. |
| `Tactical/TacticalBattleBootstrap.cs` | On entering `Tactical`, spawns the assigned prefab, or capsules, in a disk around the focus. Default count is 6 and radius is 20. No pathfinding and no combat. |
| `Assets/Shaders/VertexColorUnlit.shader` | Shader name `Proc4x/VertexColorUnlit`. `ChunkManager` uses it when no terrain material is assigned, otherwise `Sprites/Default` or `Universal Render Pipeline/Unlit`. |

The mode names and the fly-in are the scaffold’s stand-in for a later strategic map and a tactical battle. They are not a globe, a basemap, extruded buildings, or a NavMesh battle.

## License

Code in this repo is MIT. See [LICENSE](../LICENSE).

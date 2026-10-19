# Proc4x

Two things are in this repo. The one you can run is a Python and pygame skirmish on a fixed **12×6** procedural board (`proc4x/`, `run.sh`). The Unity C# under `Assets/Scripts/` and the notes in `docs/` are a Phase 0 scaffold for an OpenStreetMap / ArcGIS grand strategy game. That scaffold is not a game you can play.

## Playable prototype

`python -m proc4x.app` opens a window titled **Proc4x — 12×6 Skirmish**. A seed builds height and biomes. You and the enemy each start with one army and a small territory. Destroy the enemy army to win.

The board is always 12 columns by 6 rows. Ocean tiles are not walkable. Biomes are ocean, beach, plains, forest, desert, savanna, tundra, snow, mountains, and swamp. The same seed rebuilds the same map.

### Run

From the repo root, with Python 3.10 or newer and a display:

```bash
./run.sh
./run.sh 12345
```

`run.sh` creates `.venv` on first launch, installs `requirements.txt` (`pygame>=2.5.0`, `numpy>=1.26.0`), and runs `python -m proc4x.app`. An optional integer argument is the map seed. If you omit it, the seed is **42**. A non-integer argument prints usage and exits with status 2.

The same steps by hand:

```bash
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
python -m proc4x.app 42
```

### Controls

| Key | Action |
|-----|--------|
| **WASD** / arrows | Move the cursor (it wraps) |
| **Click** / Enter / Space | Select your army, or move it to that tile |
| **E** | End your turn; the enemy moves immediately |
| **R** | New random seed and a new game |
| **[** / **]** | Seed minus or plus 1, then a new game |
| **F** | Toggle height shading (on at start) |
| **ESC** | Quit |

The window is resizable.

### What a turn does

1. Capitals are the two land tiles farthest apart (Manhattan distance). Each side claims that tile and its orthogonal land neighbors, and places one army there (10 HP, 1 move). If a seed has fewer than two land tiles, the corners are forced to plains.
2. Your army starts selected. Green tiles are the orthogonal land tiles it can reach this turn, including the enemy army's tile. You cannot enter ocean or a tile occupied by your own army.
3. Moving onto an empty land tile claims it. Stepping onto the enemy army removes that army, costs the attacker 3 HP (never below 1), claims the tile, and ends the move.
4. **E** runs the enemy once. It steps toward your army and prefers a tile that attacks you. Then it is your turn again and both armies refresh their move.
5. If the enemy army is gone and yours remains, you win. If yours is gone and the enemy remains, you lose. If neither army remains, the game calls it a draw.

### On the board

- Blue circle — your army
- Red circle — enemy army
- Blue tint — your land
- Red tint — enemy land
- Green overlay — reachable this turn
- Yellow ring — selected army
- White outline — cursor

### Files

```
proc4x/world.py    seed, noise, height, biomes
proc4x/game.py     ownership, movement, combat, enemy turn
proc4x/app.py      pygame window, input, drawing
run.sh
requirements.txt
```

## Unity Phase 0 scaffold

`Assets/Scripts/` is C# (`Osm4x.*`) for a later grand strategy game that would use OpenStreetMap and ArcGIS. This repository does not contain a Unity project: there are no scenes, `ProjectSettings`, or package manifest, and nothing here loads OSM data or the ArcGIS SDK. There is no `Assets/Scripts/GIS/` directory. Dropping these scripts into an editor project would still not produce that game.

What the scripts actually contain:

| Path | What it does |
|------|----------------|
| `Core/WorldConfig.cs` | Seed and noise settings. The same seed sets the same noise offsets. |
| `Core/GameModeController.cs` | Modes `Strategic`, `Transition`, and `Tactical`. The focus point is world XZ, not GPS. |
| `WorldGen/` | Perlin height, temperature, moisture, and the same biome list as the Python map. |
| `Chunks/` | A square of chunks around the camera (default radius 3). Coordinates are an XZ grid, not latitude/longitude. Each chunk is a height mesh colored by biome. |
| `Camera/StrategicCameraController.cs` | Pan and zoom over that mesh. It does not orbit a globe. Input is skipped during `Transition`. |
| `Camera/TacticalTransitionController.cs` | Lerps the camera to the focus, then enters `Tactical`. In the editor or a development build, **B** starts that fly-in at the camera and **N** returns to `Strategic`. |
| `Strategy/HexGridOverlay.cs` | Shows or hides its own object when the mode changes. It does not build hexes. |
| `Tactical/TacticalBattleBootstrap.cs` | On entering `Tactical`, spawns placeholder capsules, or an assigned prefab, around the focus. No pathfinding and no combat. |
| `Assets/Shaders/VertexColorUnlit.shader` | Unlit vertex-color shader (`Proc4x/VertexColorUnlit`) used when no terrain material is assigned. |

`docs/ARCHITECTURE.md`, `docs/ROADMAP.md`, and `docs/SETUP.md` describe the runnable prototype and this Phase 0 scaffold. They match the code in `proc4x/` and `Assets/Scripts/`.

## License

MIT. See [LICENSE](LICENSE).

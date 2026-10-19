# Setup

The game you can run is the Python prototype. The Unity C# is source for a Phase 0 scaffold, not a project you can open and play.

## Prototype

Python 3.10 or newer, and a display. `requirements.txt` is `pygame>=2.5.0` and `numpy>=1.26.0`.

From the repo root:

```bash
./run.sh
./run.sh 12345
```

`run.sh` creates `.venv` if it is missing, installs `requirements.txt`, and runs `python -m proc4x.app`. The optional integer is the map seed. The default seed is 42. A non-integer argument prints `Usage: python -m proc4x.app [seed]` and exits with status 2.

The same steps by hand:

```bash
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
python -m proc4x.app 42
```

### If it does not start

**No window.** pygame calls `display.set_mode`. Run it in a session that has a display.

**`ModuleNotFoundError: pygame` or `numpy`.** The install went to a different interpreter than the one you are running. Use the venv:

```bash
source .venv/bin/activate
pip install -r requirements.txt
```

**No HUD text.** If `pygame.font` fails to import or init, `app.py` still draws the map. The top bar falls back to a status swatch and turn marks, and the legend is color squares without labels.

The prototype does not use Unity, ArcGIS, OpenStreetMap, or the network.

## Unity scripts

This repo does not contain `ProjectSettings`, scenes, or a package manifest, and it does not pin a Unity editor version. The scripts do not reference the ArcGIS SDK, Cinemachine, or a navigation package.

To compile them you create your own Unity project and copy both of these trees into its `Assets/` folder:

- `Assets/Scripts/`
- `Assets/Shaders/VertexColorUnlit.shader` (`Proc4x/VertexColorUnlit`)

`ChunkManager` looks up that shader when no terrain material is assigned. It also tries `Sprites/Default` and `Universal Render Pipeline/Unlit`.

A scene that exercises the scripts needs:

1. A `WorldConfig` object (chunks do not build without it).
2. A `GameModeController` object.
3. A `ChunkManager` object.
4. A camera with `StrategicCameraController`.

`TacticalTransitionController`, `HexGridOverlay`, and `TacticalBattleBootstrap` are optional. In the editor or a development build, **B** on the transition component calls `BeginBattleAt` with the camera position, and **N** returns to `Strategic`.

That scene is still the procedural mesh and the mode stubs. It does not add a globe, OSM tiles, or ArcGIS.

`.gitignore` already ignores Unity `Library/`, `Temp/`, and `Obj/`, plus `Secrets/`, `AuthConfig.asset`, and `Data/OSM` `.pbf` / `.osm` files. None of those data paths are populated in this repo.

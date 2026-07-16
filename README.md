# Proc4x — Procedural 4X Map (local Mac)

**Minecraft-style generative world** you run natively on macOS.  
No Unity. No real maps. No cloud APIs.

| | |
|---|---|
| **Runtime** | Python 3.11+ (tested 3.14 on Apple Silicon) |
| **Graphics** | pygame (SDL) |
| **World** | Seeded noise → height + biomes + chunk streaming |

---

## Run on your Mac

```bash
cd ~/osm-4x-grand-strategy
./run.sh
# or with a seed:
./run.sh 12345
```

First run creates `.venv` and installs `pygame` + `numpy`.

Manual:

```bash
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
python -m proc4x.app 42
```

### Controls

| Key | Action |
|-----|--------|
| **WASD** / arrows | Pan map |
| **Shift** | Faster pan |
| **Scroll** / `-` `=` | Zoom |
| **R** | Random new world (new seed) |
| **[** **]** | Seed −1 / +1 |
| **F** | Toggle height shading |
| **ESC** | Quit |

---

## How generation works

```
Seed
  ├─ continental noise → land vs ocean
  ├─ elevation noise   → mountains / plains
  ├─ temperature       → snow / desert bands
  └─ moisture          → forest / swamp / savanna
        │
        ▼
  Biome colors + height shading
        │
        ▼
  Chunks stream around the camera (Minecraft-style)
```

Same seed → same world every time.

---

## Project layout

```
proc4x/
  world.py      # noise, height, biomes
  app.py        # pygame viewer + camera
run.sh          # one-command launch
requirements.txt
docs/           # design notes
Assets/         # legacy Unity C# (optional / unused)
```

Unity is **not required**. The `Assets/` C# folder is leftover from an earlier experiment and can be ignored.

---

## License

MIT — see [LICENSE](LICENSE).

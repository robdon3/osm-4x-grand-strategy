# Proc4x — 12×6 procedural skirmish (local Mac)

A tiny **4X-style skirmish** on a fixed **12×6** board.  
No Unity. No real maps. Python + pygame only.

## What you do

1. A **procedural map** generates from a seed (biomes + ocean).
2. You and the **enemy** each get a **capital territory** and **one army**.
3. On your turn: **select your army** → **move** to a green highlighted tile (capture land).
4. Step onto the **enemy army** to **fight** (you win the fight and take the tile).
5. Press **E** to **end turn** — the enemy AI moves.
6. **Destroy the enemy army** to win.

## Run

```bash
cd ~/osm-4x-grand-strategy
./run.sh
# or:
./run.sh 12345
```

```bash
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
python -m proc4x.app 42
```

## Controls

| Key | Action |
|-----|--------|
| **WASD** / arrows | Move cursor |
| **Click** / Enter / Space | Select army or move there |
| **E** | End turn (enemy moves) |
| **R** | New random map + game |
| **[** **]** | Seed ±1 |
| **F** | Height shading |
| **ESC** | Quit |

## Legend

- **Blue circle** — your army  
- **Red circle** — enemy army  
- **Blue tint** — your land  
- **Red tint** — enemy land  
- **Green overlay** — reachable this turn  

## Layout

```
proc4x/
  world.py   # noise, biomes
  game.py    # turns, ownership, combat, AI
  app.py     # pygame UI
run.sh
requirements.txt
```

## License

MIT

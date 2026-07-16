# Architecture — Procedural 4X (local Python)

**No Unity.** Runs on macOS with Python + pygame.

## Goal

A **generative map** (Minecraft-style), not a real one:

- Single **seed** defines the world
- Deterministic noise → height, moisture, temperature, biomes
- **Chunk streaming** so only nearby terrain is generated

## Stack

| Piece | Choice |
|-------|--------|
| Language | Python 3 |
| Window / input | pygame (SDL) |
| Arrays / noise | numpy |
| Platform | macOS Apple Silicon |

## Pipeline

```
WorldConfig(seed)
    → WorldSampler (perlin noise layers)
    → height + biome per cell
    → ChunkCache (pygame surfaces)
    → camera pans; chunks load around view
```

## Modules

- `proc4x/world.py` — config, noise, biomes, height
- `proc4x/app.py` — game loop, camera, HUD, rendering

## Future (optional)

- True 3D heightmesh (moderngl) without Unity
- Hex strategic overlay for 4X systems
- Fog of war, cities, turn loop

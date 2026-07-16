# Setup — Procedural 4X (Unity)

## 1. Unity

1. Install Unity Hub + Unity 6.x LTS (Apple Silicon).
2. New project → **3D (URP)**.

## 2. Import scripts

```bash
git clone https://github.com/robdon3/osm-4x-grand-strategy.git
cp -R osm-4x-grand-strategy/Assets/Scripts /path/to/YourProject/Assets/
cp -R osm-4x-grand-strategy/Assets/Shaders /path/to/YourProject/Assets/
```

No map API keys required.

## 3. Minimal scene

1. Empty `GameSystems` with `WorldConfig`, `GameModeController`, `ChunkManager`.
2. Main Camera + `StrategicCameraController`.
3. Play. WASD move, scroll zoom. Chunks stream around the camera.

## 4. WorldConfig tunables

| Field | Meaning |
|-------|---------|
| Seed | World identity |
| Sea Level | Ocean threshold |
| Height Amplitude | Mountains |
| Chunk Size | 16 recommended |
| Load Radius | Streaming ring |

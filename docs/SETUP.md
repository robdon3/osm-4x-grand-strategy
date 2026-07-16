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

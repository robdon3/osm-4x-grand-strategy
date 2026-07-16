#!/usr/bin/env bash
# Run the procedural map viewer on macOS.
set -euo pipefail
cd "$(dirname "$0")"

if [[ ! -d .venv ]]; then
  python3 -m venv .venv
  source .venv/bin/activate
  pip install -r requirements.txt
else
  source .venv/bin/activate
fi

# Optional seed: ./run.sh 12345
exec python -m proc4x.app "$@"

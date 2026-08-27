#!/usr/bin/env sh
set -eu
GAME_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
dotnet run --project "$GAME_DIR/Vendors/Vecxy/tools/Vecxy.Cli" -- "$@"

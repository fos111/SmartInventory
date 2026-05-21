#!/usr/bin/env bash
set -a
source "$(dirname "$0")/.env"
set +a

cd "$(dirname "$0")"
exec dotnet run --project src/SmartInventory.Api/SmartInventory.Api.csproj

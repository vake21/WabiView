#!/bin/bash
# Run WabiView in demo mode for local preview

cd "$(dirname "$0")/src/WabiView"

echo "Starting WabiView in demo mode..."
echo "Open http://localhost:8080 in your browser"
echo ""
echo "Press Ctrl+C to stop"
echo ""

export ASPNETCORE_ENVIRONMENT=Development
export WABIVIEW_DEMO=1

dotnet run

#!/bin/bash

# Health check script for StartOS
# Returns exit code 0 if healthy, non-zero otherwise

WABIVIEW_PORT="${WABIVIEW_PORT:-8080}"
HEALTH_URL="http://localhost:${WABIVIEW_PORT}/health"

# Check if the web interface is responding
response=$(curl -s -o /dev/null -w "%{http_code}" "$HEALTH_URL" 2>/dev/null)

if [ "$response" = "200" ]; then
    # Application is healthy
    echo '{"result": {"version": 2, "status": "running", "message": "WabiView is operational"}}'
    exit 0
else
    # Application is not responding
    echo '{"result": {"version": 2, "status": "starting", "message": "WabiView is starting up..."}}'
    exit 1
fi

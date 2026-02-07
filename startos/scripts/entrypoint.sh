#!/bin/bash
set -e

echo "========================================"
echo "  WabiView - Starting Up"
echo "========================================"

# Data directory
DATA_DIR="${WABIVIEW_DATA_PATH:-/data}"
mkdir -p "$DATA_DIR"

echo "Data directory: $DATA_DIR"

# Configure Bitcoin RPC from StartOS environment
if [ -n "$BITCOIND_RPC_HOST" ]; then
    export BitcoinRpc__Host="$BITCOIND_RPC_HOST"
fi

if [ -n "$BITCOIND_RPC_PORT" ]; then
    export BitcoinRpc__Port="$BITCOIND_RPC_PORT"
fi

if [ -n "$BITCOIND_RPC_USER" ]; then
    export BitcoinRpc__User="$BITCOIND_RPC_USER"
fi

if [ -n "$BITCOIND_RPC_PASSWORD" ]; then
    export BitcoinRpc__Password="$BITCOIND_RPC_PASSWORD"
fi

# Configure Electrs from StartOS environment
if [ -n "$ELECTRS_HOST" ]; then
    export Electrs__Host="$ELECTRS_HOST"
fi

if [ -n "$ELECTRS_PORT" ]; then
    export Electrs__Port="$ELECTRS_PORT"
fi

echo "Bitcoin RPC: ${BitcoinRpc__Host:-bitcoind.embassy}:${BitcoinRpc__Port:-8332}"
echo "Electrs: ${Electrs__Host:-electrs.embassy}:${Electrs__Port:-50001}"

# Wait for dependencies
echo "Waiting for Bitcoin Core..."
until curl -s --user "${BitcoinRpc__User}:${BitcoinRpc__Password}" \
    --data-binary '{"jsonrpc":"1.0","method":"getblockcount","params":[]}' \
    -H 'content-type: text/plain;' \
    "http://${BitcoinRpc__Host:-bitcoind.embassy}:${BitcoinRpc__Port:-8332}/" > /dev/null 2>&1; do
    echo "  Bitcoin Core not ready, waiting..."
    sleep 5
done
echo "  Bitcoin Core is ready!"

echo "Waiting for Electrs..."
until curl -s "http://${Electrs__Host:-electrs.embassy}:${Electrs__Port:-50001}/blocks/tip/height" > /dev/null 2>&1; do
    echo "  Electrs not ready, waiting..."
    sleep 5
done
echo "  Electrs is ready!"

echo "========================================"
echo "  Starting WabiView Application"
echo "========================================"

# Start the application
exec dotnet /app/WabiView.dll

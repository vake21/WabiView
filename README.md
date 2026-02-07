# WabiView

A self-hosted WabiSabi coordinator monitor and coinjoin explorer for StartOS.

## Overview

WabiView monitors known WabiSabi coordinators and provides:
- Coordinator uptime and availability monitoring
- Round status monitoring
- Coinjoin transaction listing
- Coinjoin confirmation status
- Coordinator attribution per coinjoin

## Design Principles

- **No Nostr**: No dynamic coordinator discovery. Coordinators are hardcoded.
- **Local Data Only**: Uses Bitcoin Core and Electrs. No external APIs.
- **Privacy First**: Observational only. No clustering, tracking, or deanonymization.
- **Minimal**: Only essential features. No bloat.

## Monitored Coordinators

| Name | URL |
|------|-----|
| Kruw | https://coinjoin.kruw.io/ |
| OpenCoordinator | https://api.opencoordinator.org/ |

## Requirements

- Bitcoin Core (required)
- Electrs (required)

## Project Structure

```
WabiView/
├── src/
│   └── WabiView/           # ASP.NET Core application
│       ├── Models/         # Data models
│       ├── Services/       # Business logic
│       ├── Data/           # Database context & migrations
│       └── Pages/          # Razor Pages UI
├── startos/                # StartOS packaging
│   ├── manifest.yaml       # Package manifest
│   ├── Dockerfile          # Container definition
│   ├── docker-compose.yaml # Local development
│   ├── scripts/            # Entrypoint & health check
│   └── instructions.md     # User documentation
├── WabiView.sln            # Solution file
├── Makefile                # Build automation
└── README.md               # This file
```

## Building

### Prerequisites

- .NET 8 SDK
- Docker
- StartOS SDK (`start-sdk`)

### Local Development

```bash
# Build and run locally
make dev

# View logs
make logs

# Stop
make stop
```

### Building for StartOS

```bash
# Build the .s9pk package
make package

# Verify the package
make verify
```

## Building the .s9pk Package

1. Install the StartOS SDK:
   ```bash
   # Follow instructions at https://docs.start9.com/latest/developer-docs/packaging
   ```

2. Clone this repository:
   ```bash
   git clone https://github.com/example/wabi-view.git
   cd wabi-view
   ```

3. Build the package:
   ```bash
   make package
   ```

4. The resulting `wabi-view.s9pk` can be sideloaded onto StartOS.

## Configuration

WabiView auto-configures connections to Bitcoin Core and Electrs when running on StartOS.

For local development, edit `startos/docker-compose.yaml` or set environment variables:

| Variable | Default | Description |
|----------|---------|-------------|
| `BitcoinRpc__Host` | `bitcoind.embassy` | Bitcoin Core RPC host |
| `BitcoinRpc__Port` | `8332` | Bitcoin Core RPC port |
| `BitcoinRpc__User` | (empty) | RPC username |
| `BitcoinRpc__Password` | (empty) | RPC password |
| `Electrs__Host` | `electrs.embassy` | Electrs host |
| `Electrs__Port` | `50001` | Electrs REST port |

## Coinjoin Discovery

Coordinators do not currently expose final coinjoin TxIds via their API. Coinjoin discovery is performed by scanning the Bitcoin Core mempool for transactions matching a coinjoin heuristic (many inputs, many equal-value outputs). Discovered coinjoins are attributed to a coordinator by matching timing with recently completed rounds.

A round-based discovery path also exists but is dormant unless coordinators add TxId support in the future.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         WabiView                             │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐  │
│  │  Coordinator    │  │  Coinjoin       │  │  Web UI     │  │
│  │  Monitor        │  │  Scanner        │  │  (Razor)    │  │
│  └────────┬────────┘  └────────┬────────┘  └──────┬──────┘  │
│           │                    │                  │         │
│  ┌────────▼────────────────────▼──────────────────▼──────┐  │
│  │                    Service Layer                      │  │
│  └────────┬─────────────────────┬───────────────┬────────┘  │
└───────────┼─────────────────────┼───────────────┼───────────┘
            │                     │               │
   ┌────────▼────────┐   ┌────────▼────────┐  ┌───▼───┐
   │  Bitcoin Core   │   │    Electrs      │  │SQLite │
   │  (RPC)          │   │  (REST API)     │  │/data  │
   └─────────────────┘   └─────────────────┘  └───────┘
```

## License

MIT

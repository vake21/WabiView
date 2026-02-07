# WabiView

WabiView is a self-hosted WabiSabi coordinator monitor and coinjoin explorer.

## Overview

WabiView monitors known WabiSabi coordinators and provides visibility into:
- Coordinator availability and health
- Active coinjoin rounds and their progress
- Completed coinjoin transactions
- Transaction confirmation status

## Requirements

WabiView requires the following services to be installed and running on your StartOS:

- **Bitcoin Core** - Provides authoritative block, transaction, and mempool data
- **Electrs** - Provides indexed lookups for efficient transaction queries

Both services must be fully synced before WabiView can function properly.

## Getting Started

1. Ensure Bitcoin Core and Electrs are installed and fully synced
2. Start WabiView from the StartOS dashboard
3. Access the web interface via the provided Tor address or LAN URL

## Features

### Dashboard
The main dashboard provides an at-a-glance view of:
- Number of coordinators online
- Active coinjoin rounds
- Total coinjoins tracked
- Pending (unconfirmed) transactions

### Coordinators
View detailed status for each monitored coordinator:
- Online/offline status
- Last seen timestamp
- Current round phase and participant count
- Fee rate

### Coinjoins
Browse all tracked coinjoin transactions:
- Transaction ID
- Input/output counts
- Confirmation status
- Coordinator attribution

## Monitored Coordinators

WabiView monitors the following coordinators:

| Name | URL |
|------|-----|
| Kruw | https://coinjoin.kruw.io/ |
| OpenCoordinator | https://api.opencoordinator.org/ |

New coordinators are added through application updates.

## Privacy

WabiView is designed as an **observational explorer**, not a surveillance tool:

- ✅ Monitors coordinator availability
- ✅ Tracks coinjoin transactions
- ✅ Shows confirmation status
- ❌ No address clustering
- ❌ No wallet tracking
- ❌ No deanonymization heuristics
- ❌ No external API calls (all data from local Bitcoin Core and Electrs)

## Data Storage

All data is stored locally in SQLite at `/data/wabiview.db`.

## Troubleshooting

### WabiView won't start
- Ensure Bitcoin Core is running and synced
- Ensure Electrs is running and synced
- Check the logs for connection errors

### Coordinators showing as offline
- The coordinator may genuinely be down
- Check your internet/Tor connectivity
- The coordinator API endpoint may have changed

### No coinjoins appearing
- Coinjoins are recorded as rounds complete
- New installations will only track future coinjoins
- Historical coinjoins are not retroactively imported

## Support

For issues and feature requests, please visit the project repository.

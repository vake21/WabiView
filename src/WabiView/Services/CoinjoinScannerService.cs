using Microsoft.EntityFrameworkCore;
using WabiView.Data;
using WabiView.Models;

namespace WabiView.Services;

/// <summary>
/// Background service that scans for coinjoin transactions from completed rounds
/// and by scanning the mempool for coinjoin-pattern transactions.
/// </summary>
public class CoinjoinScannerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CoinjoinScannerService> _logger;

    private static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(1);

    public CoinjoinScannerService(
        IServiceProvider serviceProvider,
        ILogger<CoinjoinScannerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Coinjoin scanner service starting");

        // Initial delay to allow coordinator polling to populate rounds
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanForCoinjoinsFromRoundsAsync(stoppingToken);
                await ScanMempoolForCoinjoinsAsync(stoppingToken);
                await UpdateConfirmationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during coinjoin scanning");
            }

            await Task.Delay(ScanInterval, stoppingToken);
        }
    }

    /// <summary>
    /// Record coinjoins from rounds that have a known TxId.
    /// </summary>
    // NOTE:
    // Coordinators do not currently expose final coinjoin TxIds.
    // Coinjoin discovery is performed via mempool scanning.
    // This method is dormant unless coordinators add txid support.
    private async Task ScanForCoinjoinsFromRoundsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WabiViewDbContext>();
        var coinjoinService = scope.ServiceProvider.GetRequiredService<CoinjoinService>();

        var completedRounds = await db.Rounds
            .Where(r => r.Phase == RoundPhase.Ended &&
                       r.IsSuccessful &&
                       r.TxId != null)
            .Include(r => r.Coordinator)
            .ToListAsync(ct);

        foreach (var round in completedRounds)
        {
            if (string.IsNullOrEmpty(round.TxId)) continue;

            var existing = await db.CoinjoinTransactions
                .FirstOrDefaultAsync(c => c.TxId == round.TxId, ct);

            if (existing == null)
            {
                try
                {
                    await coinjoinService.RecordCoinjoinAsync(
                        round.TxId,
                        round.CoordinatorId,
                        round.RoundId);

                    _logger.LogInformation(
                        "Recorded coinjoin from round {RoundId}: {TxId}",
                        round.RoundId, round.TxId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to record coinjoin from round {RoundId}", round.RoundId);
                }
            }
        }
    }

    /// <summary>
    /// Scan the Bitcoin mempool for transactions that look like coinjoins.
    /// This catches coinjoins even when the coordinator API doesn't provide the TxId.
    /// </summary>
    private async Task ScanMempoolForCoinjoinsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WabiViewDbContext>();
        var bitcoinRpc = scope.ServiceProvider.GetRequiredService<BitcoinRpcService>();
        var coinjoinService = scope.ServiceProvider.GetRequiredService<CoinjoinService>();

        var mempoolTxIds = await bitcoinRpc.GetRawMempoolAsync();
        if (mempoolTxIds == null) return;

        foreach (var txId in mempoolTxIds)
        {
            // Skip if we already know about this transaction
            var existing = await db.CoinjoinTransactions
                .FirstOrDefaultAsync(c => c.TxId == txId, ct);
            if (existing != null) continue;

            var txInfo = await bitcoinRpc.GetRawTransactionAsync(txId, true);
            if (txInfo == null) continue;

            if (CoinjoinService.LooksLikeCoinjoin(txInfo.Value))
            {
                try
                {
                    // Try to attribute to a coordinator by matching timing with
                    // recently successful rounds that don't have a TxId yet
                    var recentSuccessfulRound = await db.Rounds
                        .Where(r => r.Phase == RoundPhase.Ended &&
                                   r.IsSuccessful &&
                                   r.TxId == null &&
                                   r.EndedAt > DateTime.UtcNow.AddMinutes(-10))
                        .OrderByDescending(r => r.EndedAt)
                        .FirstOrDefaultAsync(ct);

                    int? coordinatorId = recentSuccessfulRound?.CoordinatorId;
                    string? roundId = recentSuccessfulRound?.RoundId;

                    await coinjoinService.RecordCoinjoinAsync(txId, coordinatorId, roundId);

                    // Link the TxId back to the round if we matched one
                    if (recentSuccessfulRound != null)
                    {
                        recentSuccessfulRound.TxId = txId;
                        await db.SaveChangesAsync(ct);
                    }

                    _logger.LogInformation(
                        "Recorded coinjoin from mempool scan: {TxId} (coordinator: {Coordinator})",
                        txId, coordinatorId.HasValue ? "matched" : "unknown");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to record mempool coinjoin {TxId}", txId);
                }
            }
        }
    }

    private async Task UpdateConfirmationsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var coinjoinService = scope.ServiceProvider.GetRequiredService<CoinjoinService>();

        await coinjoinService.UpdateConfirmationsAsync();
    }
}

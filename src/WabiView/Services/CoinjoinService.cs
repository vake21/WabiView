using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WabiView.Data;
using WabiView.Models;

namespace WabiView.Services;

/// <summary>
/// Service for managing and querying coinjoin transactions.
/// </summary>
public class CoinjoinService
{
    private readonly WabiViewDbContext _db;
    private readonly BitcoinRpcService _bitcoinRpc;
    private readonly ElectrsService _electrs;
    private readonly ILogger<CoinjoinService> _logger;

    public CoinjoinService(
        WabiViewDbContext db,
        BitcoinRpcService bitcoinRpc,
        ElectrsService electrs,
        ILogger<CoinjoinService> logger)
    {
        _db = db;
        _bitcoinRpc = bitcoinRpc;
        _electrs = electrs;
        _logger = logger;
    }

    /// <summary>
    /// Get recent coinjoins with optional filtering.
    /// </summary>
    public async Task<List<CoinjoinTransaction>> GetRecentAsync(
        int limit = 50,
        int? coordinatorId = null,
        bool? confirmedOnly = null)
    {
        var query = _db.CoinjoinTransactions
            .Include(c => c.Coordinator)
            .AsQueryable();

        if (coordinatorId.HasValue)
        {
            query = query.Where(c => c.CoordinatorId == coordinatorId);
        }

        if (confirmedOnly == true)
        {
            query = query.Where(c => c.BlockHeight != null);
        }

        return await query
            .OrderByDescending(c => c.FirstSeen)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Get a specific coinjoin by transaction ID.
    /// </summary>
    public async Task<CoinjoinTransaction?> GetByTxIdAsync(string txId)
    {
        return await _db.CoinjoinTransactions
            .Include(c => c.Coordinator)
            .FirstOrDefaultAsync(c => c.TxId == txId);
    }

    /// <summary>
    /// Record a new coinjoin transaction.
    /// </summary>
    public async Task<CoinjoinTransaction> RecordCoinjoinAsync(
        string txId,
        int? coordinatorId = null,
        string? roundId = null)
    {
        var existing = await _db.CoinjoinTransactions.FirstOrDefaultAsync(c => c.TxId == txId);
        if (existing != null)
        {
            return existing;
        }

        var txInfo = await _bitcoinRpc.GetRawTransactionAsync(txId, true);
        if (txInfo == null)
        {
            throw new InvalidOperationException($"Transaction {txId} not found");
        }

        var coinjoin = new CoinjoinTransaction
        {
            TxId = txId,
            CoordinatorId = coordinatorId,
            RoundId = roundId,
            FirstSeen = DateTime.UtcNow,
            InputCount = txInfo.Value.GetProperty("vin").GetArrayLength(),
            OutputCount = txInfo.Value.GetProperty("vout").GetArrayLength(),
            VSize = txInfo.Value.GetProperty("vsize").GetInt32()
        };

        // Check confirmation status
        if (txInfo.Value.TryGetProperty("blockhash", out var blockHashProp))
        {
            coinjoin.BlockHash = blockHashProp.GetString();

            var block = await _bitcoinRpc.GetBlockAsync(coinjoin.BlockHash!);
            if (block.HasValue)
            {
                coinjoin.BlockHeight = block.Value.GetProperty("height").GetInt32();
                coinjoin.ConfirmedAt = DateTime.UtcNow;
            }
        }

        // Calculate values (simplified - real implementation would sum inputs/outputs)
        long totalOutput = 0;
        foreach (var vout in txInfo.Value.GetProperty("vout").EnumerateArray())
        {
            if (vout.TryGetProperty("value", out var valueProp))
            {
                totalOutput += (long)(valueProp.GetDouble() * 100_000_000);
            }
        }
        coinjoin.TotalOutputValue = totalOutput;

        // Fee calculation requires input values from previous transactions
        // This is a placeholder - full implementation would look up prevouts
        coinjoin.FeePaid = 0;
        coinjoin.FeeRate = 0;

        _db.CoinjoinTransactions.Add(coinjoin);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Recorded coinjoin: {TxId}", txId);
        return coinjoin;
    }

    /// <summary>
    /// Update confirmation status for unconfirmed coinjoins.
    /// </summary>
    public async Task UpdateConfirmationsAsync()
    {
        var currentHeight = await _bitcoinRpc.GetBlockCountAsync();
        if (!currentHeight.HasValue) return;

        var unconfirmed = await _db.CoinjoinTransactions
            .Where(c => c.BlockHeight == null)
            .ToListAsync();

        foreach (var coinjoin in unconfirmed)
        {
            var txInfo = await _bitcoinRpc.GetRawTransactionAsync(coinjoin.TxId, true);
            if (txInfo?.TryGetProperty("blockhash", out var blockHashProp) == true)
            {
                coinjoin.BlockHash = blockHashProp.GetString();

                var block = await _bitcoinRpc.GetBlockAsync(coinjoin.BlockHash!);
                if (block.HasValue)
                {
                    coinjoin.BlockHeight = block.Value.GetProperty("height").GetInt32();
                    coinjoin.ConfirmedAt = DateTime.UtcNow;
                }
            }
        }

        // Update confirmation counts for confirmed transactions
        var confirmed = await _db.CoinjoinTransactions
            .Where(c => c.BlockHeight != null)
            .ToListAsync();

        foreach (var coinjoin in confirmed)
        {
            coinjoin.Confirmations = currentHeight.Value - coinjoin.BlockHeight!.Value + 1;
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Get filtered coinjoins with pagination and optional text search.
    /// </summary>
    public async Task<(List<CoinjoinTransaction> Items, int TotalCount)> GetFilteredAsync(
        int page = 1,
        int pageSize = 20,
        int? coordinatorId = null,
        string? status = null,
        string? searchTxId = null)
    {
        var query = _db.CoinjoinTransactions
            .Include(c => c.Coordinator)
            .AsQueryable();

        if (coordinatorId.HasValue)
            query = query.Where(c => c.CoordinatorId == coordinatorId);

        if (status == "confirmed")
            query = query.Where(c => c.BlockHeight != null);
        else if (status == "unconfirmed")
            query = query.Where(c => c.BlockHeight == null);

        if (!string.IsNullOrWhiteSpace(searchTxId))
        {
            var search = searchTxId.Trim().ToLower();
            query = query.Where(c => c.TxId.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.FirstSeen)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Get aggregate statistics for header display.
    /// </summary>
    public async Task<(int TotalCoinjoins, int Last24hCount, decimal Volume24hBtc)> GetStatsAsync()
    {
        var total = await _db.CoinjoinTransactions.CountAsync();
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var last24h = await _db.CoinjoinTransactions
            .Where(c => c.FirstSeen >= cutoff)
            .ToListAsync();

        var last24hCount = last24h.Count;
        var volume24hBtc = last24h.Sum(c => c.TotalInputValue) / 100_000_000m;

        return (total, last24hCount, volume24hBtc);
    }

    /// <summary>
    /// Get count of coinjoins for a coordinator in the last 24 hours.
    /// </summary>
    public async Task<int> GetLast24hCountForCoordinatorAsync(int coordinatorId)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);
        return await _db.CoinjoinTransactions
            .CountAsync(c => c.CoordinatorId == coordinatorId && c.FirstSeen >= cutoff);
    }

    /// <summary>
    /// Check if a transaction looks like a WabiSabi coinjoin.
    /// Basic heuristic: many equal-value outputs, many inputs.
    /// </summary>
    public static bool LooksLikeCoinjoin(JsonElement tx)
    {
        if (!tx.TryGetProperty("vin", out var vin) ||
            !tx.TryGetProperty("vout", out var vout))
        {
            return false;
        }

        var inputCount = vin.GetArrayLength();
        var outputCount = vout.GetArrayLength();

        // WabiSabi coinjoins typically have many inputs and outputs
        if (inputCount < 5 || outputCount < 5)
        {
            return false;
        }

        // Check for equal-value outputs (coinjoin signature)
        var outputValues = new Dictionary<long, int>();
        foreach (var output in vout.EnumerateArray())
        {
            if (output.TryGetProperty("value", out var valueProp))
            {
                var satoshis = (long)(valueProp.GetDouble() * 100_000_000);
                outputValues[satoshis] = outputValues.GetValueOrDefault(satoshis) + 1;
            }
        }

        // If there are multiple outputs with the same value, likely a coinjoin
        var maxSameValue = outputValues.Values.Max();
        return maxSameValue >= 3;
    }
}

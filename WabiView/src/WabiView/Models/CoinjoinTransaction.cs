namespace WabiView.Models;

/// <summary>
/// Represents a detected WabiSabi coinjoin transaction.
/// </summary>
public class CoinjoinTransaction
{
    public int Id { get; set; }

    /// <summary>
    /// Transaction ID.
    /// </summary>
    public required string TxId { get; set; }

    /// <summary>
    /// Block hash (null if unconfirmed).
    /// </summary>
    public string? BlockHash { get; set; }

    /// <summary>
    /// Block height (null if unconfirmed).
    /// </summary>
    public int? BlockHeight { get; set; }

    /// <summary>
    /// When the transaction was first seen.
    /// </summary>
    public DateTime FirstSeen { get; set; }

    /// <summary>
    /// When the transaction was confirmed (if applicable).
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// Number of inputs.
    /// </summary>
    public int InputCount { get; set; }

    /// <summary>
    /// Number of outputs.
    /// </summary>
    public int OutputCount { get; set; }

    /// <summary>
    /// Total input value in satoshis.
    /// </summary>
    public long TotalInputValue { get; set; }

    /// <summary>
    /// Total output value in satoshis.
    /// </summary>
    public long TotalOutputValue { get; set; }

    /// <summary>
    /// Fee paid in satoshis.
    /// </summary>
    public long FeePaid { get; set; }

    /// <summary>
    /// Virtual size in vbytes.
    /// </summary>
    public int VSize { get; set; }

    /// <summary>
    /// Fee rate in sat/vB.
    /// </summary>
    public decimal FeeRate { get; set; }

    /// <summary>
    /// Associated coordinator (if known).
    /// </summary>
    public int? CoordinatorId { get; set; }
    public Coordinator? Coordinator { get; set; }

    /// <summary>
    /// Associated round ID (if known).
    /// </summary>
    public string? RoundId { get; set; }

    /// <summary>
    /// Number of confirmations (computed).
    /// </summary>
    public int Confirmations { get; set; }

    /// <summary>
    /// Whether this transaction is confirmed.
    /// </summary>
    public bool IsConfirmed => BlockHeight.HasValue;
}

namespace WabiView.Models;

/// <summary>
/// Represents a known WabiSabi coordinator.
/// </summary>
public class Coordinator
{
    public int Id { get; set; }

    /// <summary>
    /// Unique identifier/name for the coordinator.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Base URL of the coordinator API.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// Whether the coordinator is currently reachable.
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Last successful connection time.
    /// </summary>
    public DateTime? LastSeen { get; set; }

    /// <summary>
    /// Last time we attempted to contact this coordinator.
    /// </summary>
    public DateTime? LastChecked { get; set; }

    /// <summary>
    /// Consecutive failed connection attempts.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Coordinator fee rate (if known).
    /// </summary>
    public decimal? FeeRate { get; set; }

    /// <summary>
    /// Minimum input count requirement.
    /// </summary>
    public int? MinInputCount { get; set; }

    /// <summary>
    /// Associated coinjoin transactions.
    /// </summary>
    public ICollection<CoinjoinTransaction> Coinjoins { get; set; } = new List<CoinjoinTransaction>();

    /// <summary>
    /// Associated rounds.
    /// </summary>
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}

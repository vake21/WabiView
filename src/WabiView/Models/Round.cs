namespace WabiView.Models;

/// <summary>
/// Represents a WabiSabi coinjoin round.
/// </summary>
public class Round
{
    public int Id { get; set; }

    /// <summary>
    /// Round identifier from the coordinator.
    /// </summary>
    public required string RoundId { get; set; }

    /// <summary>
    /// Associated coordinator.
    /// </summary>
    public int CoordinatorId { get; set; }
    public Coordinator? Coordinator { get; set; }

    /// <summary>
    /// Current phase of the round.
    /// </summary>
    public RoundPhase Phase { get; set; }

    /// <summary>
    /// When this round was first observed.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this round was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// When this round ended (if applicable).
    /// </summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>
    /// Number of registered inputs.
    /// </summary>
    public int InputCount { get; set; }

    /// <summary>
    /// Resulting transaction ID (if successful).
    /// </summary>
    public string? TxId { get; set; }

    /// <summary>
    /// Whether the round completed successfully.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Failure reason if the round failed.
    /// </summary>
    public string? FailureReason { get; set; }
}

public enum RoundPhase
{
    InputRegistration = 0,
    ConnectionConfirmation = 1,
    OutputRegistration = 2,
    TransactionSigning = 3,
    Ended = 4
}

namespace WabiView.Models;

/// <summary>
/// DTO for coordinator status response from WabiSabi API.
/// </summary>
public class CoordinatorStatusResponse
{
    public string? RoundId { get; set; }
    public int Phase { get; set; }
    public int InputCount { get; set; }
    public long MaxSuggestedAmount { get; set; }
    public CoordinatorParameters? CoordinatorParameters { get; set; }
}

public class CoordinatorParameters
{
    public decimal CoordinationFeeRate { get; set; }
    public int MinInputCountByRound { get; set; }
    public int MaxInputCountByRound { get; set; }
    public long MinRegistrableAmount { get; set; }
    public long MaxRegistrableAmount { get; set; }
}

/// <summary>
/// View model for displaying coordinator info.
/// </summary>
public class CoordinatorViewModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
    public TimeSpan? Uptime { get; set; }
    public int TotalCoinjoins { get; set; }
    public int Last24hCoinjoins { get; set; }
    public decimal? FeeRate { get; set; }
    public RoundViewModel? CurrentRound { get; set; }
}

/// <summary>
/// View model for displaying round info.
/// </summary>
public class RoundViewModel
{
    public required string RoundId { get; set; }
    public RoundPhase Phase { get; set; }
    public int InputCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? TxId { get; set; }

    public string PhaseDisplay => Phase switch
    {
        RoundPhase.InputRegistration => "Input Registration",
        RoundPhase.ConnectionConfirmation => "Connection Confirmation",
        RoundPhase.OutputRegistration => "Output Registration",
        RoundPhase.TransactionSigning => "Transaction Signing",
        RoundPhase.Ended => "Ended",
        _ => "Unknown"
    };
}

using Microsoft.EntityFrameworkCore;
using WabiView.Data;
using WabiView.Models;

namespace WabiView.Services;

/// <summary>
/// Background service that periodically polls coordinators for status updates.
/// </summary>
public class CoordinatorPollingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ManualCoordinatorRegistry _registry;
    private readonly ILogger<CoordinatorPollingService> _logger;

    private static readonly TimeSpan BaseInterval = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan MaxInterval = TimeSpan.FromSeconds(360);

    // Per-coordinator next poll time and current interval
    private readonly Dictionary<int, DateTime> _nextPollTime = new();
    private readonly Dictionary<int, TimeSpan> _currentInterval = new();

    public CoordinatorPollingService(
        IServiceProvider serviceProvider,
        ManualCoordinatorRegistry registry,
        ILogger<CoordinatorPollingService> logger)
    {
        _serviceProvider = serviceProvider;
        _registry = registry;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Coordinator polling service starting (base interval: {Seconds}s)", BaseInterval.TotalSeconds);

        // Initialize coordinators in database
        await InitializeCoordinatorsAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollAllCoordinatorsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during coordinator polling");
            }

            // Wait until the next coordinator is due
            var now = DateTime.UtcNow;
            var nextDue = _nextPollTime.Values.Any()
                ? _nextPollTime.Values.Min()
                : now.Add(BaseInterval);
            var delay = nextDue - now;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, stoppingToken);
            }
        }
    }

    private async Task InitializeCoordinatorsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WabiViewDbContext>();

        foreach (var entry in _registry.GetCoordinators())
        {
            var existing = await db.Coordinators.FirstOrDefaultAsync(c => c.Url == entry.Url);
            if (existing == null)
            {
                db.Coordinators.Add(new Coordinator
                {
                    Name = entry.Name,
                    Url = entry.Url,
                    IsOnline = false
                });
                _logger.LogInformation("Added coordinator: {Name} ({Url})", entry.Name, entry.Url);
            }
        }

        await db.SaveChangesAsync();
    }

    private async Task PollAllCoordinatorsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WabiViewDbContext>();
        var monitor = scope.ServiceProvider.GetRequiredService<CoordinatorMonitorService>();

        var coordinators = await db.Coordinators.ToListAsync(ct);
        var now = DateTime.UtcNow;

        foreach (var coordinator in coordinators)
        {
            // Initialize tracking if first time seeing this coordinator
            if (!_nextPollTime.ContainsKey(coordinator.Id))
            {
                _nextPollTime[coordinator.Id] = now;
                _currentInterval[coordinator.Id] = BaseInterval;
            }

            // Skip if not yet due
            if (now < _nextPollTime[coordinator.Id])
                continue;

            await PollCoordinatorAsync(db, monitor, coordinator, ct);

            // Schedule next poll based on success/failure
            _nextPollTime[coordinator.Id] = DateTime.UtcNow + _currentInterval[coordinator.Id];
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task PollCoordinatorAsync(
        WabiViewDbContext db,
        CoordinatorMonitorService monitor,
        Coordinator coordinator,
        CancellationToken ct)
    {
        coordinator.LastChecked = DateTime.UtcNow;

        var isOnline = await monitor.IsOnlineAsync(coordinator.Url);

        if (isOnline)
        {
            coordinator.IsOnline = true;
            coordinator.LastSeen = DateTime.UtcNow;
            coordinator.FailureCount = 0;

            // Get current status
            var status = await monitor.GetStatusAsync(coordinator.Url);
            if (status?.CoordinatorParameters != null)
            {
                coordinator.FeeRate = status.CoordinatorParameters.CoordinationFeeRate;
                coordinator.MinInputCount = status.CoordinatorParameters.MinInputCountByRound;
            }

            // Get active rounds
            var rounds = await monitor.GetRoundsAsync(coordinator.Url);
            if (rounds != null)
            {
                foreach (var roundInfo in rounds)
                {
                    if (string.IsNullOrEmpty(roundInfo.RoundId)) continue;

                    var existingRound = await db.Rounds
                        .FirstOrDefaultAsync(r => r.CoordinatorId == coordinator.Id && r.RoundId == roundInfo.RoundId, ct);

                    var isEnded = roundInfo.Phase == 4;
                    var isBlame = roundInfo.IsBlameRound || !string.IsNullOrEmpty(roundInfo.BlameOf);

                    if (existingRound == null)
                    {
                        db.Rounds.Add(new Round
                        {
                            RoundId = roundInfo.RoundId,
                            CoordinatorId = coordinator.Id,
                            Phase = (RoundPhase)roundInfo.Phase,
                            InputCount = roundInfo.InputCount,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            EndedAt = isEnded ? DateTime.UtcNow : null,
                            IsSuccessful = isEnded && !isBlame,
                            FailureReason = isEnded && isBlame ? "Blame round" : null
                        });
                    }
                    else
                    {
                        existingRound.Phase = (RoundPhase)roundInfo.Phase;
                        existingRound.InputCount = roundInfo.InputCount;
                        existingRound.UpdatedAt = DateTime.UtcNow;

                        if (isEnded)
                        {
                            existingRound.EndedAt ??= DateTime.UtcNow;
                            existingRound.IsSuccessful = !isBlame;
                            if (isBlame)
                                existingRound.FailureReason ??= "Blame round";
                        }
                    }
                }
            }

            // Reset to base interval on success
            _currentInterval[coordinator.Id] = BaseInterval;
            _logger.LogDebug("Coordinator {Name} is online", coordinator.Name);
        }
        else
        {
            coordinator.IsOnline = false;
            coordinator.FailureCount++;

            // Double the interval on failure, up to max
            var current = _currentInterval.GetValueOrDefault(coordinator.Id, BaseInterval);
            var backed = TimeSpan.FromTicks(current.Ticks * 2);
            _currentInterval[coordinator.Id] = backed > MaxInterval ? MaxInterval : backed;

            _logger.LogWarning("Coordinator {Name} is offline (failures: {Count}, next poll in {Seconds}s)",
                coordinator.Name, coordinator.FailureCount, _currentInterval[coordinator.Id].TotalSeconds);
        }
    }
}

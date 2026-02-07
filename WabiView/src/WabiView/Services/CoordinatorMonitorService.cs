using System.Text.Json;
using WabiView.Models;

namespace WabiView.Services;

/// <summary>
/// Service for monitoring coordinator health and fetching round status.
/// </summary>
public class CoordinatorMonitorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CoordinatorMonitorService> _logger;

    public CoordinatorMonitorService(
        HttpClient httpClient,
        ILogger<CoordinatorMonitorService> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _logger = logger;
    }

    /// <summary>
    /// Check if a coordinator is online and reachable.
    /// </summary>
    public async Task<bool> IsOnlineAsync(string coordinatorUrl)
    {
        try
        {
            var url = NormalizeUrl(coordinatorUrl) + "/wabisabi/status";
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Coordinator {Url} is offline", coordinatorUrl);
            return false;
        }
    }

    /// <summary>
    /// Get the current status/round information from a coordinator.
    /// </summary>
    public async Task<CoordinatorStatusResponse?> GetStatusAsync(string coordinatorUrl)
    {
        try
        {
            var url = NormalizeUrl(coordinatorUrl) + "/wabisabi/status";
            var response = await _httpClient.GetStringAsync(url);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<CoordinatorStatusResponse>(response, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status from coordinator {Url}", coordinatorUrl);
            return null;
        }
    }

    /// <summary>
    /// Get all active rounds from a coordinator.
    /// </summary>
    public async Task<List<RoundInfo>?> GetRoundsAsync(string coordinatorUrl)
    {
        try
        {
            var url = NormalizeUrl(coordinatorUrl) + "/wabisabi/human-monitor";
            var response = await _httpClient.GetStringAsync(url);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var monitor = JsonSerializer.Deserialize<HumanMonitorResponse>(response, options);
            return monitor?.Rounds;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get rounds from coordinator {Url}", coordinatorUrl);
            return null;
        }
    }

    private static string NormalizeUrl(string url)
    {
        return url.TrimEnd('/');
    }
}

public class HumanMonitorResponse
{
    public List<RoundInfo>? Rounds { get; set; }
}

public class RoundInfo
{
    public string? RoundId { get; set; }
    public int Phase { get; set; }
    public int InputCount { get; set; }
    public long MaxSuggestedAmount { get; set; }
    public string? BlameOf { get; set; }
    public bool IsBlameRound { get; set; }
}

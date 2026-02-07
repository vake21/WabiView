using System.Text.Json;
using Microsoft.Extensions.Options;

namespace WabiView.Services;

/// <summary>
/// Service for interacting with Electrs REST API.
/// Used for indexed lookups (tx history, scripthash queries, etc.).
/// </summary>
public class ElectrsService
{
    private readonly HttpClient _httpClient;
    private readonly ElectrsSettings _settings;
    private readonly ILogger<ElectrsService> _logger;

    public ElectrsService(
        HttpClient httpClient,
        IOptions<ElectrsSettings> settings,
        ILogger<ElectrsService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    private string BaseUrl => _settings.GetBaseUrl();

    public async Task<JsonElement?> GetTransactionAsync(string txId)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{BaseUrl}/tx/{txId}");
            return JsonDocument.Parse(response).RootElement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get transaction {TxId} from Electrs", txId);
            return null;
        }
    }

    public async Task<string?> GetRawTransactionAsync(string txId)
    {
        try
        {
            return await _httpClient.GetStringAsync($"{BaseUrl}/tx/{txId}/hex");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get raw transaction {TxId} from Electrs", txId);
            return null;
        }
    }

    public async Task<JsonElement?> GetTransactionStatusAsync(string txId)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{BaseUrl}/tx/{txId}/status");
            return JsonDocument.Parse(response).RootElement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get transaction status {TxId} from Electrs", txId);
            return null;
        }
    }

    public async Task<JsonElement[]?> GetAddressTransactionsAsync(string address)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{BaseUrl}/address/{address}/txs");
            var doc = JsonDocument.Parse(response);
            return doc.RootElement.EnumerateArray().ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get address transactions for {Address} from Electrs", address);
            return null;
        }
    }

    public async Task<int?> GetBlockHeightAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{BaseUrl}/blocks/tip/height");
            return int.Parse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get block height from Electrs");
            return null;
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var height = await GetBlockHeightAsync();
            return height.HasValue;
        }
        catch
        {
            return false;
        }
    }
}

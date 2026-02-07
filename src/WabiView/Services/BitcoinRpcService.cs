using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace WabiView.Services;

/// <summary>
/// Service for interacting with Bitcoin Core via JSON-RPC.
/// This is the authoritative source for block, transaction, and mempool data.
/// </summary>
public class BitcoinRpcService
{
    private readonly HttpClient _httpClient;
    private readonly BitcoinRpcSettings _settings;
    private readonly ILogger<BitcoinRpcService> _logger;

    public BitcoinRpcService(
        IHttpClientFactory httpClientFactory,
        IOptions<BitcoinRpcSettings> settings,
        ILogger<BitcoinRpcService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _settings = settings.Value;
        _logger = logger;

        // Configure authentication
        if (!string.IsNullOrEmpty(_settings.User))
        {
            var authBytes = Encoding.ASCII.GetBytes($"{_settings.User}:{_settings.Password}");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
        }
    }

    public async Task<JsonDocument?> CallAsync(string method, params object[] parameters)
    {
        var request = new
        {
            jsonrpc = "1.0",
            id = Guid.NewGuid().ToString(),
            method,
            @params = parameters
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        try
        {
            var response = await _httpClient.PostAsync(_settings.GetRpcUrl(), content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("error", out var error) && error.ValueKind != JsonValueKind.Null)
            {
                _logger.LogError("Bitcoin RPC error: {Error}", error.GetRawText());
                return null;
            }

            return doc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bitcoin RPC call failed: {Method}", method);
            return null;
        }
    }

    public async Task<int?> GetBlockCountAsync()
    {
        var result = await CallAsync("getblockcount");
        return result?.RootElement.GetProperty("result").GetInt32();
    }

    public async Task<string?> GetBlockHashAsync(int height)
    {
        var result = await CallAsync("getblockhash", height);
        return result?.RootElement.GetProperty("result").GetString();
    }

    public async Task<JsonElement?> GetBlockAsync(string blockHash, int verbosity = 1)
    {
        var result = await CallAsync("getblock", blockHash, verbosity);
        return result?.RootElement.GetProperty("result");
    }

    public async Task<JsonElement?> GetRawTransactionAsync(string txId, bool verbose = true)
    {
        var result = await CallAsync("getrawtransaction", txId, verbose);
        return result?.RootElement.GetProperty("result");
    }

    public async Task<JsonElement?> GetMempoolInfoAsync()
    {
        var result = await CallAsync("getmempoolinfo");
        return result?.RootElement.GetProperty("result");
    }

    public async Task<string[]?> GetRawMempoolAsync()
    {
        var result = await CallAsync("getrawmempool");
        if (result == null) return null;

        var array = result.RootElement.GetProperty("result");
        return array.EnumerateArray()
            .Select(e => e.GetString()!)
            .ToArray();
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var blockCount = await GetBlockCountAsync();
            return blockCount.HasValue;
        }
        catch
        {
            return false;
        }
    }
}

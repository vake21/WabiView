namespace WabiView.Services;

public class BitcoinRpcSettings
{
    public string Host { get; set; } = "bitcoind.embassy";
    public int Port { get; set; } = 8332;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";

    public string GetRpcUrl() => $"http://{Host}:{Port}";
}

public class ElectrsSettings
{
    public string Host { get; set; } = "electrs.embassy";
    public int Port { get; set; } = 50001;

    public string GetBaseUrl() => $"http://{Host}:{Port}";
}

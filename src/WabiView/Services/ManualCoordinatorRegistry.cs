namespace WabiView.Services;

/// <summary>
/// Hardcoded list of known WabiSabi coordinators.
/// NO NOSTR. NO DYNAMIC DISCOVERY.
/// New coordinators are added via application updates only.
/// </summary>
public class ManualCoordinatorRegistry
{
    /// <summary>
    /// The canonical list of known coordinators.
    /// To add a new coordinator, update this list and release a new version.
    /// </summary>
    private static readonly CoordinatorEntry[] KnownCoordinators =
    [
        new CoordinatorEntry
        {
            Name = "Kruw",
            Url = "https://coinjoin.kruw.io/",
            Description = "Kruw Coordinator"
        },
        new CoordinatorEntry
        {
            Name = "OpenCoordinator",
            Url = "https://api.opencoordinator.org/",
            Description = "Open Coordinator"
        }
    ];

    public IReadOnlyList<CoordinatorEntry> GetCoordinators()
    {
        return KnownCoordinators;
    }

    public CoordinatorEntry? GetByUrl(string url)
    {
        var normalizedUrl = NormalizeUrl(url);
        return KnownCoordinators.FirstOrDefault(c =>
            NormalizeUrl(c.Url).Equals(normalizedUrl, StringComparison.OrdinalIgnoreCase));
    }

    public CoordinatorEntry? GetByName(string name)
    {
        return KnownCoordinators.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeUrl(string url)
    {
        url = url.TrimEnd('/');
        return url;
    }
}

public class CoordinatorEntry
{
    public required string Name { get; init; }
    public required string Url { get; init; }
    public string? Description { get; init; }
}

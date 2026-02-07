using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WabiView.Data;
using WabiView.Models;
using WabiView.Services;

namespace WabiView.Pages;

public class IndexModel : PageModel
{
    private readonly WabiViewDbContext _db;
    private readonly CoinjoinService _coinjoinService;

    public IndexModel(WabiViewDbContext db, CoinjoinService coinjoinService)
    {
        _db = db;
        _coinjoinService = coinjoinService;
    }

    // Header stats
    public int TotalCoinjoins { get; set; }
    public int Last24hCount { get; set; }
    public decimal Volume24hBtc { get; set; }

    // Coordinator cards
    public List<CoordinatorViewModel> Coordinators { get; set; } = new();

    // Filter parameters
    [BindProperty(SupportsGet = true)]
    public int? CoordinatorFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public new int Page { get; set; } = 1;

    // Transaction list
    public List<CoinjoinTransaction> Transactions { get; set; } = new();
    public int TransactionTotalCount { get; set; }
    public int PageSize { get; set; } = 20;
    public int TotalPages => (int)Math.Ceiling(TransactionTotalCount / (double)PageSize);

    // Modal detail
    [BindProperty(SupportsGet = true)]
    public string? TxId { get; set; }

    public CoinjoinTransaction? SelectedTransaction { get; set; }
    public bool ShowModal => SelectedTransaction != null;

    public async Task OnGetAsync()
    {
        if (Page < 1) Page = 1;

        // Header stats
        var stats = await _coinjoinService.GetStatsAsync();
        TotalCoinjoins = stats.TotalCoinjoins;
        Last24hCount = stats.Last24hCount;
        Volume24hBtc = stats.Volume24hBtc;

        // Coordinator cards
        var coordinators = await _db.Coordinators.ToListAsync();
        foreach (var coord in coordinators)
        {
            var currentRound = await _db.Rounds
                .Where(r => r.CoordinatorId == coord.Id && r.Phase != RoundPhase.Ended)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            Coordinators.Add(new CoordinatorViewModel
            {
                Id = coord.Id,
                Name = coord.Name,
                Url = coord.Url,
                IsOnline = coord.IsOnline,
                LastSeen = coord.LastSeen,
                FeeRate = coord.FeeRate,
                TotalCoinjoins = await _db.CoinjoinTransactions.CountAsync(c => c.CoordinatorId == coord.Id),
                Last24hCoinjoins = await _coinjoinService.GetLast24hCountForCoordinatorAsync(coord.Id),
                CurrentRound = currentRound != null ? new RoundViewModel
                {
                    RoundId = currentRound.RoundId,
                    Phase = currentRound.Phase,
                    InputCount = currentRound.InputCount,
                    CreatedAt = currentRound.CreatedAt
                } : null
            });
        }

        // Filtered transaction list
        var result = await _coinjoinService.GetFilteredAsync(
            page: Page,
            pageSize: PageSize,
            coordinatorId: CoordinatorFilter,
            status: StatusFilter,
            searchTxId: Search);

        Transactions = result.Items;
        TransactionTotalCount = result.TotalCount;

        // Modal
        if (!string.IsNullOrEmpty(TxId))
        {
            SelectedTransaction = await _coinjoinService.GetByTxIdAsync(TxId);
        }
    }

    public string BuildUrl(string? txId = null, int? page = null)
    {
        var parts = new List<string>();
        if (CoordinatorFilter.HasValue)
            parts.Add($"coordinatorFilter={CoordinatorFilter}");
        if (!string.IsNullOrEmpty(StatusFilter))
            parts.Add($"statusFilter={Uri.EscapeDataString(StatusFilter)}");
        if (!string.IsNullOrEmpty(Search))
            parts.Add($"search={Uri.EscapeDataString(Search)}");
        if (page.HasValue && page.Value > 1)
            parts.Add($"page={page}");
        else if (!page.HasValue && Page > 1)
            parts.Add($"page={Page}");
        if (!string.IsNullOrEmpty(txId))
            parts.Add($"txId={Uri.EscapeDataString(txId)}");

        return parts.Count > 0 ? "/?" + string.Join("&", parts) : "/";
    }
}

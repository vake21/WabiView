using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WabiView.Pages;

public class CoinjoinDetailModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string TxId { get; set; } = "";

    public IActionResult OnGet()
    {
        if (!string.IsNullOrEmpty(TxId))
        {
            return RedirectToPage("/Index", new { txId = TxId });
        }
        return RedirectToPage("/Index");
    }
}

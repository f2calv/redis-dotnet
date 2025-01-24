using Microsoft.AspNetCore.Mvc.RazorPages;
namespace CasCap.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        HttpContext.Session.SetString("name", "fred bloggs");
        HttpContext.Session.SetInt32("age", 99);
    }
}

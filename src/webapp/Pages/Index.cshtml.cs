using Microsoft.AspNetCore.Mvc.RazorPages;
namespace CasCap.Pages;

public class IndexModel : PageModel
{
    public void OnGet()
    {
        HttpContext.Session.SetString("name", "fred bloggs");
        HttpContext.Session.SetInt32("age", 99);
    }
}

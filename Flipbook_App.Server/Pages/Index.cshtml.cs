using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flipbook_App.Pages;

public class IndexModel : PageModel
{
	public IActionResult OnGet()
	{
		if (User.Identity?.IsAuthenticated ?? false)
		{
			// Redirect logged-in users to Projects
			return RedirectToPage("/Projects");
		}

		return Page();
	}

	public async Task<IActionResult> OnPostLogout()
	{
		await HttpContext.SignOutAsync();
		return RedirectToPage("/Index");
	}
}
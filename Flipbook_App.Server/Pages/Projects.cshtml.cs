using FlipbookApp.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Flipbook_App.Pages;

public class ProjectsModel : PageModel
{
	public List<string> PlaceholderProjects { get; set; } = new List<string>();

	public void OnGet()
	{
		// Create 6 placeholder projects
		for (int i = 1; i <= 6; i++)
		{
			PlaceholderProjects.Add($"Project {i}");
		}
	}
}

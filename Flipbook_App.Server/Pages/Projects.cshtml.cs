using FlipbookApp.Data;
using FlipbookApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flipbook_App.Pages;

public class ProjectsModel : PageModel
{
	RepositoryManager repositoryManager;

	public ProjectsModel(RepositoryManager repositoryManager)
	{
		this.repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
	}

	public IList<AnimationReference> UserProjects { get; set; } = [];

	public void OnGet()
	{
		if (User.Identity?.IsAuthenticated ?? false)
		{
			var username = User.Identity.Name;
			var userAnimations = repositoryManager.Users.GetUserAnimations(username);

			UserProjects = userAnimations.OrderByDescending(a => a.CreatedAt).ToList() ?? [];
		}
	}
}

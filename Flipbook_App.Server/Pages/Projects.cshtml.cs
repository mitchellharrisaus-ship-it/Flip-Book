using Flipbook_App.Repositories;
using Flipbook_App.Services;
using FlipBook_App.Shared.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flipbook_App.Pages;

public class ProjectsModel : PageModel
{
	RepositoryManager repositoryManager;
	IBlobStorageService blobStorageService;

	public ProjectsModel(RepositoryManager repositoryManager, IBlobStorageService blobStorageService)
	{
		this.repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
		this.blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
	}

	public IList<DashboardEntry> UserProjects { get; set; } = [];

	public async Task OnGet()
	{
		if (User.Identity?.IsAuthenticated ?? false)
		{
			var username = User.Identity.Name;
			var userAnimations = repositoryManager.Users.GetUserAnimations(username);

			var animations = userAnimations.OrderByDescending(a => a.CreatedAt).ToList() ?? [];
			foreach (var animation in animations)
			{
				UserProjects.Add(new(animation));
			}

			// Attach thumbnails
			foreach (var project in UserProjects)
			{
				// get from BlobStorageService

				var thumbnails = await blobStorageService.GetThumbnails(project.AnimationID);

				// store as plain string URIs
				project.ThumbnailUrls = thumbnails.Select(u => u.ToString()).ToList();
			}
		}
	}

	[BindProperty]
	public Guid AnimationID { get; set; }

	public async Task<IActionResult> OnPostDeleteProjectAsync()
	{
		await blobStorageService.DeleteAnimation(AnimationID);
		repositoryManager.Animations.DeleteAnimation(AnimationID);

		await repositoryManager.SaveChangesAsync();

		return new JsonResult(new { success = true });
	}
}

public class DashboardEntry
{
	public DashboardEntry(AnimationReference animation)
	{
		AnimationID = animation.AnimationID;
		Title = animation.Title;
		CreatedAt = animation.CreatedAt;
	}
	public Guid AnimationID { get; set; }
	public string Title { get; set; }
	public DateTime CreatedAt { get; set; }

	public List<string> ThumbnailUrls { get; set; } = [];
}
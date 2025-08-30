using Flipbook_App.Models.DTOs;
using Flipbook_App.Repositories;
using Flipbook_App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flipbook_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanvasController : ControllerBase
{
	IBlobStorageService blobService;
	RepositoryManager repositoryManager;

	public CanvasController(IBlobStorageService blobService, RepositoryManager repositoryManager)
	{
		this.blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
		this.repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
	}

	[Route("save/{animationTitle}")]
	[HttpPost]
	[Authorize]
	public async Task<IActionResult> SaveAnimation([FromBody] IEnumerable<Frame> animationFrames, string animationTitle)
	{
		var loggedInUser = User?.Identity?.Name;
		if (loggedInUser == null) return Unauthorized("User must be logged in to save animations.");
		
		try
		{
			var user = repositoryManager.Users.GetByUsername(loggedInUser);
			if (user == null) return NotFound($"Couldn't find user by name {loggedInUser}");

			var existingAnimationReference = repositoryManager.Animations.GetByTitleAndUserID(animationTitle, user.Id);

			var animation = new Animation
			{
				AnimationID = existingAnimationReference?.AnimationID ?? Guid.NewGuid(),
				Frames = animationFrames.ToList(),
				MetaData = new AnimationMetaData
				{
					UserID = user.Id,
				}
			};

			await blobService.UploadAnimation(animation);
			repositoryManager.Animations.CreateIfNotExists(animation, animationTitle);

			await repositoryManager.SaveChangesAsync();
		}
		catch (Exception ex)
		{
			return BadRequest($"Failed to upload animation: {ex.Message}");
		}

		return Ok("Successfully saved animation");
	}

	[Route("load/{animationID}")]
	[HttpGet]
	[Authorize]
	public async Task<IActionResult> LoadAnimation(string animationID)
	{
		try
		{
			var downloadedAnimation = await blobService.DownloadAnimation(Guid.Parse(animationID));
		
			return Ok(downloadedAnimation);
		}
		catch (Exception ex)
		{
			return BadRequest($"Failed to download animation: {ex.Message}");
		}
	}

	[Route("title/{currentTitle}")]
	[HttpGet]
	[Authorize]
	public IActionResult EnsureValidTitle(string currentTitle)
	{
		var loggedInUser = User?.Identity?.Name;
		if (loggedInUser == null) return Unauthorized("User must be logged in to generate animation titles.");

		try
		{
			var user = repositoryManager.Users.GetByUsername(loggedInUser);
			if (user == null) return NotFound($"Couldn't find user by name {loggedInUser}");

			var validTitle = currentTitle;
			var suffix = 1;

			while (repositoryManager.Animations.GetByTitleAndUserID(validTitle, user.Id) != null)
			{
				validTitle = $"{currentTitle} ({suffix})";
				suffix++;
			}

			return Ok(validTitle);
		}
		catch (Exception ex)
		{
			return BadRequest($"Failed to generate valid title: {ex.Message}");
		}
	}

	[Route("title/{animationID}/{newTitle}")]
	[HttpPost]
	[Authorize]
	public async Task<IActionResult> RenameAnimation(string animationID, string newTitle)
	{
		throw new NotImplementedException();
	}

}

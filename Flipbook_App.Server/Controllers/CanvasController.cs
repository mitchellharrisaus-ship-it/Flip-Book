using Flipbook_App.Repositories;
using Flipbook_App.Services;
using FlipBook_App.Shared.DTOs;
using FlipBook_Library.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flipbook_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanvasController : ControllerBase
{
	IBlobStorageService blobService;
	IExportService exportService;
	RepositoryManager repositoryManager;

	public CanvasController(IBlobStorageService blobService, IExportService exportService, RepositoryManager repositoryManager)
	{
		this.blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
		this.exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
		this.repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
	}

	[Route("save/{animationTitle}")]
	[HttpPost]
	[Authorize]
	public async Task<IActionResult> SaveAnimation([FromBody] IEnumerable<Frame> animationFrames, string animationTitle)
	{
		var loggedInUser = User?.Identity?.Name;
		if (loggedInUser == null) return Unauthorized("User must be logged in to save animations.");
		
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

		return Ok("Successfully saved animation");
	}

	[Route("load/{animationID}")]
	[HttpGet]
	[Authorize]
	public async Task<IActionResult> LoadAnimation(string animationID)
	{
		var downloadedAnimation = await blobService.DownloadAnimation(Guid.Parse(animationID));

		// Query the animation reference to get the title
		var animationReference = repositoryManager.Animations.GetById(Guid.Parse(animationID));

		if (animationReference == null)
		{
			return NotFound($"Animation reference not found for ID {animationID}");
		}

		return Ok(new AnimationLoadResponse
		{
			Title = animationReference.Title,
			Animation = downloadedAnimation
		});
	}

	[Route("title/{currentTitle}")]
	[HttpGet]
	[Authorize]
	public IActionResult EnsureValidTitle(string currentTitle)
	{
		var loggedInUser = User?.Identity?.Name;
		if (loggedInUser == null) return Unauthorized("User must be logged in to generate animation titles.");

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

	[Route("title/{animationID}/{newTitle}")]
	[HttpPost]
	[Authorize]
	public async Task<IActionResult> RenameAnimation(string animationID, string newTitle)
	{
		throw new NotImplementedException();
	}

	[Route("export/{animationTitle}")]
	[HttpPost]
	[Authorize]
	public async Task<IActionResult> ExportAnimation([FromBody] byte[] compressedFrames, string animationTitle, [FromQuery] ExportOptions options)
	{
		if (compressedFrames == null || compressedFrames.Length == 0 || string.IsNullOrWhiteSpace(animationTitle))
		{
			return BadRequest("No frames provided for export.");
		}

		var exportedBytes = await exportService.ExportAnimationAsync(compressedFrames, animationTitle, options);

		// Send as downloadable file
		return File(exportedBytes, "application/octet-stream");
	}

	[Route("thumbnail/{animationTitle}")]
	[HttpPost]
	[Authorize]
	public async Task<IActionResult> UploadThumbnails([FromBody] List<byte[]> thumbnails, string animationTitle)
	{
		var loggedInUser = User?.Identity?.Name; 
		if (loggedInUser == null) return Unauthorized("User must be logged in to upload thumbnails."); 
		
		var user = repositoryManager.Users.GetByUsername(loggedInUser); 
		if (user == null) return NotFound($"Couldn't find user by name {loggedInUser}"); 
		
		var existingAnimationReference = repositoryManager.Animations.GetByTitleAndUserID(animationTitle, user.Id); 
		if (existingAnimationReference == null) return NotFound($"Couldn't find animation by title {animationTitle} for user {loggedInUser}"); 
		
		await blobService.UploadThumbnails(existingAnimationReference.AnimationID, thumbnails); return Ok("Successfully uploaded thumbnails");
	}
}

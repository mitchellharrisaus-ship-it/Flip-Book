using Microsoft.AspNetCore.Mvc;

namespace Flipbook_App.Server.Controllers;

[Route("api/animations")]
[ApiController]
public class AnimationsController : ControllerBase
{
	private static readonly string AnimationsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Animations");
	private const string ThumbnailsFolderName = "Thumbnails";

	[HttpGet("{animationId}/thumbnails/{fileName}")]
	public IActionResult GetThumbnail(Guid animationId, string fileName)
	{
		var thumbnailPath = Path.Combine(AnimationsFolder, animationId.ToString(), ThumbnailsFolderName, fileName);

		if (!System.IO.File.Exists(thumbnailPath))
			return NotFound();

		var mimeType = "image/webp";
		return PhysicalFile(thumbnailPath, mimeType);
	}
}

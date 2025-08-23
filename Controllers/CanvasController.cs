using Flipbook_App.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Flipbook_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanvasController : ControllerBase
{
	readonly string animationsFolderPath = "Animations";

	[Route("write-to-file")]
	[HttpPost]
	public IActionResult WriteCanvas([FromBody] ImageDataDTO imageData)
	{
		if (string.IsNullOrEmpty(imageData.EncodedImage))
		{
			return BadRequest("Canvas data cannot be empty.");
		}

		var imageDataBytes = imageData.EncodedImage.Split(",")[1];
		var imageBytes = Convert.FromBase64String(imageDataBytes);

		var animationPath = $"{animationsFolderPath}/{imageData.ImageName}";

		Directory.CreateDirectory(animationPath);
		System.IO.File.WriteAllBytes($"{animationPath}/Frame_{imageData.FrameNumber}.{imageData.FileExtension}", imageBytes);

		return Ok("Canvas data received successfully.");
	}

	[Route("write-action-to-file")]
	[HttpPost]
	public IActionResult WriteActionToFile([FromBody] DrawActionDTO drawAction)
	{
		if (drawAction == null || drawAction.Vertices == null || drawAction.Vertices.Length == 0 || drawAction.BrushColour == null)
		{
			return BadRequest("Draw action data missing required data.");
		}



		return Ok("Draw action data received successfully.");
	}

}

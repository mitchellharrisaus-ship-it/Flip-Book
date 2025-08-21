using Flipbook_App.Models;
using Microsoft.AspNetCore.Mvc;

namespace Flipbook_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanvasController : ControllerBase
{
	readonly string imagesFolderPath = "Images";

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

		Directory.CreateDirectory(imagesFolderPath);
		System.IO.File.WriteAllBytes($"{imagesFolderPath}/{imageData.ImageName}.{imageData.FileExtension}", imageBytes);

		// Here you would typically save the canvas data to a database or file.
		// For demonstration purposes, we'll just return a success message.
		return Ok("Canvas data received successfully.");
	}

}

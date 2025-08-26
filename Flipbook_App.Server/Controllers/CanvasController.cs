using Flipbook_App.Models.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Flipbook_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanvasController : ControllerBase
{
	readonly string animationsFolderPath = "Animations";
	private readonly IWebHostEnvironment _env;

	public CanvasController(IWebHostEnvironment env)
	{
		_env = env;
	}

	[HttpPost("save")]
	[IgnoreAntiforgeryToken]
	public async Task<IActionResult> Save()
	{
		try
		{
			using var reader = new StreamReader(Request.Body);
			var body = await reader.ReadToEndAsync();

			var doc = JsonDocument.Parse(body);
			if (!doc.RootElement.TryGetProperty("imageData", out var imageDataElement))
				return BadRequest("Missing imageData property.");

			var base64Data = imageDataElement.GetString();
			if (string.IsNullOrWhiteSpace(base64Data))
				return BadRequest("imageData is empty.");

			var base64 = base64Data.Substring(base64Data.IndexOf(",") + 1);
			var bytes = Convert.FromBase64String(base64);

			var folderPath = Path.Combine(_env.WebRootPath, "Animations", "CanvasUploads");
			Directory.CreateDirectory(folderPath);

			var filePath = Path.Combine(folderPath, "saved-drawing.png");
			await System.IO.File.WriteAllBytesAsync(filePath, bytes);

			return Ok(new { success = true });
		}
		catch (Exception ex)
		{
			return BadRequest($"Exception: {ex.Message}");
		}
	}

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

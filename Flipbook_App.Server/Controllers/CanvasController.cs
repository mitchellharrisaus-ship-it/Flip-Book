using Flipbook_App.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Flipbook_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanvasController : ControllerBase
{
	readonly string animationsFolderPath = "Animations";
	readonly string actionsPathName = "Actions";
	//readonly IWebHostEnvironment _env;

	//public CanvasController(IWebHostEnvironment env)
	//{
	//	_env = env;
	//}

	//[HttpPost("save")]
	//[IgnoreAntiforgeryToken]
	//public async Task<IActionResult> Save()
	//{
	//	try
	//	{
	//		using var reader = new StreamReader(Request.Body);
	//		var body = await reader.ReadToEndAsync();

	//		var doc = JsonDocument.Parse(body);
	//		if (!doc.RootElement.TryGetProperty("imageData", out var imageDataElement))
	//			return BadRequest("Missing imageData property.");

	//		var base64Data = imageDataElement.GetString();
	//		if (string.IsNullOrWhiteSpace(base64Data))
	//			return BadRequest("imageData is empty.");

	//		var base64 = base64Data.Substring(base64Data.IndexOf(",") + 1);
	//		var bytes = Convert.FromBase64String(base64);

	//		var folderPath = Path.Combine(_env.WebRootPath, "Animations", "CanvasUploads");
	//		Directory.CreateDirectory(folderPath);

	//		var filePath = Path.Combine(folderPath, "saved-drawing.png");
	//		await System.IO.File.WriteAllBytesAsync(filePath, bytes);

	//		return Ok(new { success = true });
	//	}
	//	catch (Exception ex)
	//	{
	//		return BadRequest($"Exception: {ex.Message}");
	//	}
	//}

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

		var animationPath = Path.Combine(animationsFolderPath, imageData.ImageName);
		Directory.CreateDirectory(animationPath);

		var filePath = Path.Combine(animationPath, $"Frame_{imageData.FrameNumber}.{imageData.FileExtension}");
		System.IO.File.WriteAllBytes(filePath, imageBytes);

		return Ok("Canvas data received successfully.");
	}

	[Route("write-action-to-file/{animationName}")]
	[HttpPost]
	public async Task<IActionResult> WriteActionToFile([FromBody] DrawActionDTO drawAction, string animationName)
	{
		if (drawAction == null || drawAction.Vertices == null || drawAction.Vertices.Length == 0 || drawAction.BrushColour == null)
		{
			return BadRequest("Draw action data missing required data.");
		}

		try
		{
			var animationPath = Path.Combine(animationsFolderPath, animationName);
			var actionsPath = Path.Combine(animationPath, actionsPathName);
			Directory.CreateDirectory(actionsPath);

			var actionFilePath = Path.Combine(actionsPath, $"Action_Frame_{drawAction.ActionFrame}.json");

			if (!System.IO.File.Exists(actionFilePath))
			{
				var drawActionJson = JsonSerializer.SerializeToUtf8Bytes(new List<DrawActionDTO> { drawAction });

				await System.IO.File.WriteAllBytesAsync(actionFilePath, drawActionJson);
			}
			else
			{
				var existingFile = await System.IO.File.ReadAllBytesAsync(actionFilePath);
				var deserializedData = JsonSerializer.Deserialize<IEnumerable<DrawActionDTO>>(existingFile);
			
				await System.IO.File.WriteAllBytesAsync(actionFilePath, JsonSerializer.SerializeToUtf8Bytes(deserializedData?.Append(drawAction)));
			}
		}
		catch
		{
			return BadRequest("Failed to write draw action data to file.");
		}

		return Ok("Draw action data received successfully.");
	}

	[Route("get-actions/{animationName}")]
	[HttpGet]
	public async Task<IActionResult> GetActions(string animationName)
	{
		var animationPath = Path.Combine(animationsFolderPath, animationName);
		var actionsPath = Path.Combine(animationPath, actionsPathName);
		if (!Directory.Exists(actionsPath))
		{
			return NotFound("No actions found for the specified animation.");
		}

		var actionFiles = Directory.GetFiles(actionsPath, "Action_Frame_*.json");
		var allActions = new List<DrawActionDTO>();
		foreach (var file in actionFiles)
		{
			var fileContent = await System.IO.File.ReadAllBytesAsync(file);
			var actions = JsonSerializer.Deserialize<IEnumerable<DrawActionDTO>>(fileContent);
			if (actions != null)
			{
				allActions.AddRange(actions);
			}
		}

		return Ok(allActions);
	}

}

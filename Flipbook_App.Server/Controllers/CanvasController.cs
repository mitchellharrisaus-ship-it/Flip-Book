using FlipBook_Library.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Flipbook_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanvasController : ControllerBase
{
	readonly string animationsFolderPath = "Animations";
	readonly string actionsPathName = "Actions";

	[Route("actions/{animationName}")]
	[HttpPost]
	public async Task<IActionResult> WriteActionToFile([FromBody] DrawActionDTO drawAction, string animationName)
	{
		if (drawAction == null || drawAction.Vertices == null || drawAction.Vertices.Count == 0)
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

	[Route("actions/{animationName}")]
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

	[Route("undo/{animationName}/{frameNumber}")]
	[HttpPost]
	public async Task<IActionResult> Undo(string animationName, int frameNumber)
	{
		var animationPath = Path.Combine(animationsFolderPath, animationName);
		var actionsPath = Path.Combine(animationPath, actionsPathName);
		Directory.CreateDirectory(actionsPath);

		var actionFile = Directory.GetFiles(actionsPath, $"Action_Frame_{frameNumber}.json").FirstOrDefault();
		if (actionFile == null)
		{
			return NotFound($"No actions found for animation: {animationName} at frame: {frameNumber}");
		}

		try
		{
			var fileContent = System.IO.File.ReadAllBytes(actionFile);
			var actionStack = JsonSerializer.Deserialize<Stack<DrawActionDTO>>(fileContent);

			if (actionStack == null || actionStack.Count == 0)
			{
				return BadRequest("No actions to undo.");
			}

			var undoneAction = actionStack.Pop();
			await System.IO.File.WriteAllBytesAsync(actionFile, JsonSerializer.SerializeToUtf8Bytes(actionStack));
			return Ok("Successfully undid last draw action");
		}
		catch
		{
			return BadRequest("Failed to undo the last action.");
		}
	}

	[Route("redo/{animationName}/{frameNumber}")]
	[HttpPost]
	public async Task<IActionResult> Redo([FromBody] DrawActionDTO redoneAction, string animationName, int frameNumber)
	{
		if (redoneAction == null || redoneAction.Vertices == null || redoneAction.Vertices.Count == 0)
		{
			return BadRequest("Redone action data missing required data.");
		}

		var animationPath = Path.Combine(animationsFolderPath, animationName);
		var actionsPath = Path.Combine(animationPath, actionsPathName);
		Directory.CreateDirectory(actionsPath);

		var actionFile = Directory.GetFiles(actionsPath, $"Action_Frame_{frameNumber}.json").FirstOrDefault();
		if (actionFile == null)
		{
			return NotFound($"No actions found for animation: {animationName} at frame: {frameNumber}");
		}

		try
		{
			var fileContent = await System.IO.File.ReadAllBytesAsync(actionFile);
			var actionStack = JsonSerializer.Deserialize<Stack<DrawActionDTO>>(fileContent) ?? new Stack<DrawActionDTO>();
			actionStack.Push(redoneAction);

			await System.IO.File.WriteAllBytesAsync(actionFile, JsonSerializer.SerializeToUtf8Bytes(actionStack));
			return Ok("Successfully redid the draw action");
		}
		catch
		{
			return BadRequest("Failed to redo the action.");
		}
	}

	//[Route("GenerateFrames")]
	//[HttpPost]
	//public async Task<IActionResult> GenerateFrames([FromBody] List<Frame> Frames, PhysicsSettings physicsSettings)
	//{
	//	if (physicsSettings == null || Frames == null || Frames.Count == 0)
	//	{
	//		return BadRequest("Missing required data.");
	//	}

	//	//Calls Frame Generator Service to generate frames with physics applied
	//}
}

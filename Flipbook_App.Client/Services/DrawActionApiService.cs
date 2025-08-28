using Flipbook_App.Client.Models.DTOs;
using System.Net.Http.Json;

namespace Flipbook_App.Client.Services;

public class DrawActionApiService
{
	const string apiPath = "/api/canvas";
	const string actionsPath = $"{apiPath}/actions";
	const string undoPath = $"{apiPath}/undo";
	const string redoPath = $"{apiPath}/redo";

	readonly HttpClient httpClient;

	public DrawActionApiService(HttpClient httpClient)
	{
		this.httpClient = httpClient;
	}

	public void SendDrawAction(DrawActionDTO drawAction, string animationName)
	{
		httpClient.PostAsJsonAsync($"{actionsPath}/{animationName}", drawAction);
	}

	public async Task<IEnumerable<DrawActionDTO>> LoadAnimation(string animationName)
	{
		var response = await httpClient.GetAsync($"{actionsPath}/{animationName}");

		if (response.IsSuccessStatusCode)
		{
			var actions = await response.Content.ReadFromJsonAsync<IEnumerable<DrawActionDTO>>();
			return actions ?? throw new Exception("Failed to load animation data.");
		}

		throw new Exception("Failed to load animation data.");
	}

	public void Undo(string animationName, int frameNumber)
	{
		httpClient.PostAsync($"{undoPath}/{animationName}/{frameNumber}", null);
	}

	public void Redo(string animationName, int frameNumber, DrawActionDTO redoneAction)
	{
		 httpClient.PostAsJsonAsync($"{redoPath}/{animationName}/{frameNumber}", redoneAction);
	}
}

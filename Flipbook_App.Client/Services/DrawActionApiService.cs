using Flipbook_App.Client.Models.DTOs;
using System.Net.Http.Json;

namespace Flipbook_App.Client.Services;

public class DrawActionApiService
{
	const string apiPath = "/api/canvas";
	const string writeActionToFilePath = $"{apiPath}/write-action-to-file";
	const string getActionsPath = $"{apiPath}/get-actions";

	readonly HttpClient httpClient;

	public DrawActionApiService(HttpClient httpClient)
	{
		this.httpClient = httpClient;
	}

	public void SendDrawAction(DrawActionDTO? drawAction, string animationName)
	{
		httpClient.PostAsJsonAsync($"{writeActionToFilePath}/{animationName}", drawAction);
	}

	public async Task<IEnumerable<DrawActionDTO>> LoadAnimation(string animationName)
	{
		var response = await httpClient.GetAsync($"{getActionsPath}/{animationName}");

		if (response.IsSuccessStatusCode)
		{
			var actions = await response.Content.ReadFromJsonAsync<IEnumerable<DrawActionDTO>>();
			return actions ?? throw new Exception("Failed to load animation data.");
		}

		throw new Exception("Failed to load animation data.");
	}
}

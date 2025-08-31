using FlipBook_Library.DTOs;
using System.Net.Http.Json;

namespace Flipbook_App.Client.Services;

public class DrawActionApiService : IDrawActionApiService
{
	const string apiPath = "/api/canvas";

	const string savePath = $"{apiPath}/save";
	const string titlePath = $"{apiPath}/title";
	const string loadPath = $"{apiPath}/load";

	readonly HttpClient httpClient;

	public DrawActionApiService(HttpClient httpClient)
	{
		this.httpClient = httpClient;
	}

	public async Task<string> EnsureValidTitle(string currentTitle)
	{
		var response = await httpClient.GetStringAsync($"{titlePath}/{currentTitle}");

		return response;
	}

	public async Task Save(IEnumerable<Frame> animationFrames, string animationTitle)
	{
		await httpClient.PostAsJsonAsync($"{savePath}/{animationTitle}", animationFrames);
	}

	public async Task<Animation> Load(Guid animationID)
	{
		var response = await httpClient.GetAsync($"{loadPath}/{animationID}");

		if (!response.IsSuccessStatusCode)
		{
			throw new Exception("Failed to load animation data.");
		}

		var animation = await response.Content.ReadFromJsonAsync<Animation>();
		return animation ?? throw new NullReferenceException("Animation data was null.");
	}
}

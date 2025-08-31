using FlipBook_Library.DTOs;
using SkiaSharp;
using System.IO.Compression;
using System.Net.Http.Json;

namespace Flipbook_App.Client.Services;

public class DrawActionApiService : IDrawActionApiService
{
	const string apiPath = "/api/canvas";

	const string savePath = $"{apiPath}/save";
	const string titlePath = $"{apiPath}/title";
	const string loadPath = $"{apiPath}/load";
	const string exportPath = $"{apiPath}/export";

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

	public async Task Export(IList<SKData> renderedFrames, string animationTitle)
	{
		var zipData = await ZipFramesAsync(renderedFrames);

		await httpClient.PostAsJsonAsync($"{exportPath}/{animationTitle}", zipData);
	}


	static async Task<byte[]> ZipFramesAsync(IList<SKData> frames)
	{
		using var ms = new MemoryStream();
		using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
		{
			for (var i = 0; i < frames.Count; i++)
			{
				var entry = archive.CreateEntry($"Action_Frame_{i}.png");
				using var entryStream = entry.Open();
				var data = frames[i].ToArray();
				await entryStream.WriteAsync(data);
			}
		}

		return ms.ToArray();
	}
}

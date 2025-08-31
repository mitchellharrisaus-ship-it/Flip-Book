using FlipBook_App.Shared.DTOs;
using FlipBook_App.Shared.Enums;
using FlipBook_Library.DTOs;
using Microsoft.JSInterop;
using SkiaSharp;
using System.IO.Compression;
using System.Net.Http.Json;

namespace Flipbook_App.Client.Services;

public class AnimationApiService : IAnimationApiService
{
	const string apiPath = "/api/canvas";

	const string savePath = $"{apiPath}/save";
	const string titlePath = $"{apiPath}/title";
	const string loadPath = $"{apiPath}/load";
	const string exportPath = $"{apiPath}/export";

	readonly HttpClient httpClient;
	readonly IJSRuntime jsRuntime;

	public AnimationApiService(HttpClient httpClient, IJSRuntime jsRuntime)
	{
		this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		this.jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
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

	public async Task Export(IList<SKData> renderedFrames, string animationTitle, ExportOptions exportOptions)
	{
		var zipData = await ZipFramesAsync(renderedFrames);

		var exportOptionsQuery = new Dictionary<string, string>
		{
			{ "frameRate", exportOptions.FrameRate.ToString() },
			{ "width", exportOptions.Width.ToString() },
			{ "height", exportOptions.Height.ToString() },
			{ "format", exportOptions.Format.ToString() },
			{ "quality", exportOptions.Quality.ToString() },
			{ "includeBackground", exportOptions.IncludeBackground.ToString() },
			{ "backgroundColor", exportOptions.BackgroundColor },
			{ "spriteSheetColumns", exportOptions.SpriteSheetColumns.ToString() },
			{ "optimize", exportOptions.Optimise.ToString() }
		};

		var queryString = string.Join("&", exportOptionsQuery
			.Where(kv => !string.IsNullOrEmpty(kv.Value))
			.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

		var response = await httpClient.PostAsJsonAsync($"{exportPath}/{animationTitle}?{queryString}", zipData);
		response.EnsureSuccessStatusCode();

		var fileBytes = await response.Content.ReadAsByteArrayAsync();
		var base64 = Convert.ToBase64String(fileBytes);
		string fileExtension = exportOptions.Format switch
		{
			ExportFormats.PNGSequence => "zip",
			ExportFormats.SpriteSheet => "png",
			ExportFormats.GIF => "gif",
			ExportFormats.MP4 => "mp4",
			ExportFormats.WebP => "webp",
			_ => "bin"
		};
		string fileName = $"{animationTitle}.{fileExtension}";

		await jsRuntime.InvokeVoidAsync("saveAsFile", fileName, base64);
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

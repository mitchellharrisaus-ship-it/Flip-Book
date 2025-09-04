using FlipBook_Library.DTOs;
using System.Text.Json;

namespace Flipbook_App.Services;

public class LocalStorageService
{
	private static readonly string AnimationsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Animations");
	private const string MetadataFilePrefix = "Metadata";
	private const string ActionsFolderName = "Actions";
	private const string ThumbnailsFolderName = "Thumbnails";

	public LocalStorageService()
	{
		Directory.CreateDirectory(AnimationsFolder);
	}

	public async Task UploadAnimation(Animation animation)
	{
		var animationFolder = Path.Combine(AnimationsFolder, animation.AnimationID.ToString());
		var actionsFolder = Path.Combine(animationFolder, ActionsFolderName);

		// Delete existing animation folder if exists
		if (Directory.Exists(animationFolder))
			Directory.Delete(animationFolder, true);

		Directory.CreateDirectory(actionsFolder);

		// Save frames
		foreach (var frame in animation.Frames)
		{
			var framePath = Path.Combine(actionsFolder, $"Action_Frame_{frame.FrameIndex}.json");
			await using var frameStream = File.Create(framePath);
			await JsonSerializer.SerializeAsync(frameStream, frame);
		}

		// Save metadata
		var metadataPath = Path.Combine(animationFolder, $"{MetadataFilePrefix}.json");
		await using var metadataStream = File.Create(metadataPath);
		await JsonSerializer.SerializeAsync(metadataStream, animation.MetaData);
	}

	public async Task<Animation> DownloadAnimation(Guid animationID)
	{
		var animationFolder = Path.Combine(AnimationsFolder, animationID.ToString());
		var actionsFolder = Path.Combine(animationFolder, ActionsFolderName);

		if (!Directory.Exists(animationFolder) || !Directory.Exists(actionsFolder))
			throw new Exception("Animation not found.");

		// Load metadata
		var metadataPath = Path.Combine(animationFolder, $"{MetadataFilePrefix}.json");
		if (!File.Exists(metadataPath))
			throw new Exception("Metadata not found.");

		await using var metadataStream = File.OpenRead(metadataPath);
		var metaData = await JsonSerializer.DeserializeAsync<AnimationMetaData>(metadataStream)
			?? throw new NullReferenceException("Null metadata received from local storage.");

		// Load frames
		var frames = new List<Frame>();
		foreach (var frameFile in Directory.GetFiles(actionsFolder, "Action_Frame_*.json"))
		{
			await using var frameStream = File.OpenRead(frameFile);
			var frame = await JsonSerializer.DeserializeAsync<Frame>(frameStream)
				?? throw new NullReferenceException("Null frame received from local storage.");
			frames.Add(frame);
		}

		if (frames.Count == 0)
			throw new Exception("No frames found.");

		return new Animation
		{
			AnimationID = animationID,
			MetaData = metaData,
			Frames = frames
		};
	}

	public async Task DeleteAnimation(Guid animationID)
	{
		var animationFolder = Path.Combine(AnimationsFolder, animationID.ToString());
		if (Directory.Exists(animationFolder))
			Directory.Delete(animationFolder, true);
		await Task.CompletedTask;
	}

	public async Task UploadThumbnails(Guid animationID, List<byte[]> thumbnails)
	{
		var thumbnailsFolder = Path.Combine(AnimationsFolder, animationID.ToString(), ThumbnailsFolderName);
		Directory.CreateDirectory(thumbnailsFolder);

		for (var i = 0; i < thumbnails.Count; i++)
		{
			var thumbnailPath = Path.Combine(thumbnailsFolder, $"Thumbnail_{i}.webp");
			await File.WriteAllBytesAsync(thumbnailPath, thumbnails[i]);
		}
	}

	public async Task<List<Uri>> GetThumbnails(Guid animationID)
	{
		var thumbnailsFolder = Path.Combine(AnimationsFolder, animationID.ToString(), ThumbnailsFolderName);
		var uris = new List<Uri>();

		if (Directory.Exists(thumbnailsFolder))
		{
			foreach (var file in Directory.GetFiles(thumbnailsFolder, "Thumbnail_*.webp"))
			{
				var fileName = Path.GetFileName(file);
				// Generate a URL to the controller endpoint
				var url = $"/api/animations/{animationID}/thumbnails/{fileName}";
				uris.Add(new Uri(url, UriKind.Relative));
			}
		}

		await Task.CompletedTask;
		return uris;
	}

}

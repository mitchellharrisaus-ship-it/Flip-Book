using Azure.Storage.Blobs;
using System.Text.Json;
using FlipBook_Library.DTOs;
using Azure.Storage.Sas;

namespace Flipbook_App.Services;

public class BlobStorageService : IBlobStorageService
{
	const string metadataFilePrefix = "Metadata";
	const string actionsFolderName = "Actions";
	const string thumbnailsFolderName = "Thumbnails";

	readonly BlobContainerClient containerClient;

	public BlobStorageService(string connectionString, string containerName)
	{
		containerClient = new BlobContainerClient(connectionString, containerName);
	}

	public async Task UploadAnimation(Animation animation)
	{
		// Delete existing blobs for the animation
		await foreach (var blob in containerClient.GetBlobsAsync(prefix: animation.AnimationID.ToString()))
		{
			var blobClient = containerClient.GetBlobClient(blob.Name);
			await blobClient.DeleteIfExistsAsync();
		}

		foreach (var frame in animation.Frames)
		{
			var blobName = $"{animation.AnimationID}/{actionsFolderName}/Action_Frame_{frame.FrameIndex}.json";

			using var actionStream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(frame));
			await containerClient.UploadBlobAsync(blobName, actionStream);
		}

		var metaDataName = $"{animation.AnimationID}/{metadataFilePrefix}.json";
		using var metadataStream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(animation.MetaData));
		await containerClient.UploadBlobAsync(metaDataName, metadataStream);
	}

	public async Task<Animation> DownloadAnimation(Guid animationID)
	{
		var animation = new Animation
		{
			AnimationID = animationID,
			Frames = [],
			MetaData = new AnimationMetaData()
		};

		var foundMetadata = false;
		var foundFrames = false;

		await foreach (var entry in containerClient.GetBlobsAsync(prefix: animationID.ToString()))
		{
			var fileName = entry.Name.Split(animationID.ToString() + "/")[1];
			var blobClient = containerClient.GetBlobClient(entry.Name);
			
			var content = await blobClient.DownloadContentAsync();
			var contentData = content.Value.Content.ToStream();

			if (fileName.StartsWith(metadataFilePrefix))
			{
				var deserializedData = await JsonSerializer.DeserializeAsync<AnimationMetaData>(contentData);
				animation.MetaData = deserializedData ?? throw new NullReferenceException("Null metadata received from blob storage.");
				foundMetadata = true;
			}

			if (fileName.StartsWith(actionsFolderName))
			{
				var deserializedData = await JsonSerializer.DeserializeAsync<Frame>(contentData);
				animation.Frames.Add(deserializedData ?? throw new NullReferenceException("Null frame received from blob storage."));
				foundFrames = true;
			}
		}

		if (!foundMetadata || !foundFrames)
		{
			throw new Exception("Incomplete animation data in blob storage.");
		}

		return animation;
	}

	public async Task DeleteAnimation(Guid animationID)
	{
		await foreach (var blob in containerClient.GetBlobsAsync(prefix: animationID.ToString()))
		{
			var blobClient = containerClient.GetBlobClient(blob.Name);
			await blobClient.DeleteIfExistsAsync();
		}
	}

	public async Task UploadThumbnails(Guid animationID, List<byte[]> thumbnails)
	{
		for (var thumbnailIndex = 0; thumbnailIndex < thumbnails.Count; thumbnailIndex++)
		{
			var blobName = $"{animationID}/{thumbnailsFolderName}/Thumbnail_{thumbnailIndex}.webp";
			var blobClient = containerClient.GetBlobClient(blobName);

			using var stream = new MemoryStream(thumbnails[thumbnailIndex]);
			await blobClient.UploadAsync(stream, overwrite: true);
		}
	}

	public async Task<List<Uri>> GetThumbnails(Guid animationID)
	{
		var uris = new List<Uri>();

		await foreach (var blob in containerClient.GetBlobsAsync(prefix: $"{animationID}/{thumbnailsFolderName}/"))
		{
			var blobClient = containerClient.GetBlobClient(blob.Name);

			// Generate a SAS token valid for 1 hour
			var sasUri = blobClient.GenerateSasUri(
				BlobSasPermissions.Read,
				DateTimeOffset.UtcNow.AddHours(1)
			);

			uris.Add(sasUri);
		}

		return uris;
	}
}

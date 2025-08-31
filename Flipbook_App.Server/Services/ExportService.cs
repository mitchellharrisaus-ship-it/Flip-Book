using FlipBook_App.Shared.DTOs;
using FlipBook_App.Shared.Enums;
using ImageMagick;
using ImageMagick.Formats;
using OpenCvSharp;
using System.IO.Compression;
using Size = OpenCvSharp.Size;

namespace Flipbook_App.Services;

public class ExportService : IExportService
{
	public async Task<byte[]> ExportAnimationAsync(byte[] compressedFrames, string animationTitle, ExportOptions options)
	{
		var frames = UnzipFrames(compressedFrames);

		return options.Format switch
		{
			ExportFormats.PNGSequence => compressedFrames,
			ExportFormats.GIF => ExportGif(frames, animationTitle, options),
			ExportFormats.MP4 => await ExportMP4(frames, animationTitle, options),
			ExportFormats.SpriteSheet => ExportSpriteSheet(frames, animationTitle, options),
			ExportFormats.WebP => ExportWebP(frames, animationTitle, options),

			_ => throw new NotImplementedException($"Export format {options.Format} is not supported."),
		};
	}

	#region exports

	byte[] ExportWebP(List<byte[]> frames, string animationTitle, ExportOptions options)
	{
		var imageCollection = BuildImageCollection(frames, options);

		// Export to animated WebP
		var settings = new WebPWriteDefines
		{
			Lossless = false,
			AlphaQuality = options.Quality
		};

		return imageCollection.ToByteArray(settings);
	}

	byte[] ExportSpriteSheet(List<byte[]> frames, string animationTitle, ExportOptions options)
	{
		var imageCollection = BuildImageCollection(frames, options);

		var columns = Math.Max(1, options.SpriteSheetColumns);
		var rows = (uint)Math.Ceiling((double)imageCollection.Count / columns);

		using var spriteSheet = new MagickImage(MagickColors.Transparent, (uint)(options.Width * columns), (uint)(options.Height * rows));

		for (var imageIndex = 0; imageIndex < imageCollection.Count; imageIndex++)
		{
			var x = (imageIndex % columns) * options.Width;
			var y = (imageIndex / columns) * options.Height;

			spriteSheet.Composite(imageCollection[imageIndex], x, y, CompositeOperator.Over);
		}

		return spriteSheet.ToByteArray(MagickFormat.Png);
	}

	async Task<byte[]> ExportMP4(List<byte[]> frames, string animationTitle, ExportOptions options)
	{
		// Create a unique temp file
		var fileName = $"{animationTitle}_{Guid.NewGuid()}.mp4";
		var outputPath = Path.Combine(Path.GetTempPath(), fileName);

		try
		{
			// H.264 codec
			var fourcc = FourCC.X264;

			using var writer = new VideoWriter(outputPath, fourcc, options.FrameRate, new Size(options.Width, options.Height));

			if (!writer.IsOpened())
			{
				throw new Exception("Failed to open MP4 writer");
			}

			foreach (var frameBytes in frames)
			{
				using var ms = new MemoryStream(frameBytes);
				using var mat = Mat.FromStream(ms, ImreadModes.Color);

				writer.Write(mat);
			}

			writer.Release();

			return await File.ReadAllBytesAsync(outputPath);
		}
		finally
		{
			// Ensure temp file is deleted even if an error occurs
			if (File.Exists(outputPath))
			{
				try { File.Delete(outputPath); } catch {}
			}
		}
	}

	byte[] ExportGif(List<byte[]> frames, string animationTitle, ExportOptions options)
	{
		var imageCollection = BuildImageCollection(frames, options, isGif: true);

		return imageCollection.ToByteArray(MagickFormat.Gif);
	}

	#endregion

	static MagickImageCollection BuildImageCollection(List<byte[]> frames, ExportOptions options, bool isGif = false)
	{
		var animationDelay = Math.Max(1, 100.0 / options.FrameRate);
		var collection = new MagickImageCollection();

		foreach (var frameBytes in frames)
		{
			var image = new MagickImage(frameBytes);

			if (options.IncludeBackground)
			{
				var bgColor = new MagickColor(
					string.IsNullOrWhiteSpace(options.BackgroundColor)
						? "#FFFFFF"
						: options.BackgroundColor);

				var bg = new MagickImage(bgColor, image.Width, image.Height);
				bg.Composite(image, CompositeOperator.Over);
				image = bg;
			}
			else
			{
				image.Alpha(AlphaOption.Set); // ensure transparency
			}

			if (isGif)
			{
				image.GifDisposeMethod = GifDisposeMethod.Background;
			}

			image.Quality = (uint)options.Quality;
			image.AnimationDelay = (uint)animationDelay;

			collection.Add(image);
		}

		if (options.Optimise)
		{
			collection.Optimize();
		}

		return collection;
	}

	static List<byte[]> UnzipFrames(byte[] zipData)
	{
		var frames = new List<byte[]>();
		using var ms = new MemoryStream(zipData);
		using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
		foreach (var entry in archive.Entries)
		{
			using var entryStream = entry.Open(); // already decompressed / unzipped here  
			using var entryMs = new MemoryStream();

			entryStream.CopyTo(entryMs);
			frames.Add(entryMs.ToArray());
		}
		return frames;
	}
}


using System.IO.Compression;

namespace Flipbook_App.Services;

public class ExportService : IExportService
{
	public async Task ExportAnimation(byte[] compressedFrames, string animationTitle)
	{
		var frames = UnzipFrames(compressedFrames);

		var exportPath = Path.Combine("Exports", animationTitle);
		Directory.CreateDirectory(exportPath);

		for (var i = 0; i < frames.Count; i++)
		{
			var framePath = Path.Combine(exportPath, $"frame_{i + 1:D4}.png");
			await File.WriteAllBytesAsync(framePath, frames[i]);
		}
	}

	static List<byte[]> UnzipFrames(byte[] zipData)
	{
		var frames = new List<byte[]>();
		using var ms = new MemoryStream(zipData);
		using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
		foreach (var entry in archive.Entries)
		{
			using var entryStream = entry.Open(); // already decompressed here
			using var entryMs = new MemoryStream();

			entryStream.CopyTo(entryMs);
			frames.Add(entryMs.ToArray());
		}
		return frames;
	}
}

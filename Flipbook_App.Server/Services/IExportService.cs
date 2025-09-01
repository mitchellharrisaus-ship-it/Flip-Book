using FlipBook_App.Shared.DTOs;

namespace Flipbook_App.Services;

public interface IExportService
{
	Task<byte[]> ExportAnimationAsync(byte[] compressedFrames, string animationTitle, ExportOptions options);
}

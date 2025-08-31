namespace Flipbook_App.Services;

public interface IExportService
{
	Task ExportAnimation(byte[] compressedFrames, string animationTitle);
}

namespace Flipbook_App.Models.DTOs;

public class ImageDataDTO
{
	public int CanvasWidth { get; set; }

	public int CanvasHeight { get; set; }

	public string EncodedImage { get; set; }

	public string ImageName { get; set; }

	public string FileExtension { get; set; }

	public int FrameNumber { get; set; }
}

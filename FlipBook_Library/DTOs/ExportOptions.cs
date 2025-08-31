using FlipBook_App.Shared.Enums;

namespace FlipBook_App.Shared.DTOs;

public class ExportOptions
{
	public int FrameRate { get; set; } = 12;

	public int Width { get; set; } = 800;

	public int Height { get; set; } = 600;

	public ExportFormats Format { get; set; } = ExportFormats.PNGSequence;

	public int Quality { get; set; } = 80; // For formats that support quality settings (e.g., GIF, MP4)

	public bool IncludeBackground { get; set; } = true;

	public string BackgroundColor { get; set; } = "#FFFFFF"; // Hex color code

	public int SpriteSheetColumns { get; set; } = 5; // For SpriteSheet format

	public bool Optimise { get; set; } = false; // For formats that support optimization (e.g Gif, WebP)

}

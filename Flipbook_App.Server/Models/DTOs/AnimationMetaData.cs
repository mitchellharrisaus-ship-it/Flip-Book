namespace Flipbook_App.Models.DTOs;

public class AnimationMetaData
{
	public Guid UserID { get; set; }

	public int AnimationWidth { get; set; } = 800;

	public int AnimationHeight { get; set; } = 600;

	public int FrameRate { get; set; } = 12;
}

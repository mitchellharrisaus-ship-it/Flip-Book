namespace FlipBook_Library.DTOs;

public class AnimationMetaData
{
	public Guid UserID { get; set; }

	public int AnimationWidth { get; set; } = 700;

	public int AnimationHeight { get; set; } = 700;

	public int FrameRate { get; set; } = 12;
}

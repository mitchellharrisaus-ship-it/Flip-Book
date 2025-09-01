using FlipBook_Library.DTOs;

namespace FlipBook_App.Shared.DTOs;

public class AnimationLoadResponse
{
	public string Title { get; set; } = string.Empty;
	public Animation? Animation { get; set; }
}

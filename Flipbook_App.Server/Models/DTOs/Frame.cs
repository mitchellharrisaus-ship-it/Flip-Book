namespace Flipbook_App.Models.DTOs;

public class Frame
{
	public required int FrameIndex { get; set; }

	public required Stack<DrawActionDTO> Actions { get; set; }
}

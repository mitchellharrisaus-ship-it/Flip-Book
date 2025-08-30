namespace Flipbook_App.Models.DTOs;

public class Frame
{
	public required int FrameIndex { get; set; }

	public Stack<DrawActionDTO> Actions { get; set; } = [];
}

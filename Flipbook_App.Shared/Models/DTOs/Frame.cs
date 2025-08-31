namespace Flipbook_App.Shared.Models.DTOs;

public class Frame
{
    public int FrameIndex { get; set; }
    public Stack<DrawActionDTO> Actions { get; set; } = new();
}
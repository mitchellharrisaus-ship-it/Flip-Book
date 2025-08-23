namespace Flipbook_App.Models.DTOs;

public class DrawActionDTO
{
	public required Vertex[] Vertices { get; set; }

	public BrushType Brush { get; set; }

	public required Colour BrushColour { get; set; }

	public float BrushSize { get; set; }

	public int ActionFrame { get; set; }
}

using FlipBook_Library.Core;

namespace FlipBook_Library.DTOs;

public class DrawActionDTO
{
	public IList<Vertex> Vertices { get; set; } = [];

	public BrushType Brush { get; set; }

	public required Colour BrushColour { get; set; }

	public int BrushSize { get; set; }

	public int ActionFrame { get; set; }

	public bool IsPhysicsObject { get; set; } = false;

	public PhysicsObjectSettings? PhysicsSettings { get; set; }
}

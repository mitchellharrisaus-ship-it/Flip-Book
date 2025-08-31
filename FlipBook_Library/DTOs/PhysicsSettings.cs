using FlipBook_Library.Core;

namespace FlipBook_Library.DTOs;

public class PhysicsSettings
{
	public float TimeToMap { get; set; }

	public int NumberOfFrames { get; set; }

	public bool HasBoarder { get; set; }

	public float Gravity { get; set; }

	public float Width { get; set; }

	public float Height { get; set; }
	public Vertex TopLeftCanvasCoordinates { get; set; }
	public int HeightofCanvasInPixels { get; set; }
	public int WidthofCanvasInPixels { get; set; }
}

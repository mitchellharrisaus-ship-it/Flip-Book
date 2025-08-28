using SkiaSharp;

namespace Flipbook_App.Client.Models;

public struct Colour(SKColor color)
{
	public int R { get; set; } = color.Red;

	public int G { get; set; } = color.Green;

	public int B { get; set; } = color.Blue;

	public int A { get; set; } = color.Alpha;
}

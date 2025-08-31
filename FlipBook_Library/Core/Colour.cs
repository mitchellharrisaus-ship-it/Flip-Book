using SkiaSharp;

namespace FlipBook_Library.Core;

public struct Colour
{
	public int R { get; set; }

	public int G { get; set; }

	public int B { get; set; }
	
	public int A { get; set; }

	public Colour(SKColor skColor)
	{
		R = skColor.Red;
		G = skColor.Green;
		B = skColor.Blue;
		A = skColor.Alpha;
	}
}

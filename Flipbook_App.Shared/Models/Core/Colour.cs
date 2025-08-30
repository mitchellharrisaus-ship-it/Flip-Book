namespace Flipbook_App.Shared.Models.Core;

public class Colour
{
    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }
    public int A { get; set; } = 255;

    public Colour() { }

    public Colour(int r, int g, int b, int a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
}
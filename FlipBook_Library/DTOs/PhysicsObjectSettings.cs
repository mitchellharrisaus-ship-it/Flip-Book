using FlipBook_Library.Enums;
using System.Reflection.Metadata;

namespace FlipBook_Library.DTOs;

public class PhysicsObjectSettings
{
	public int ActionFrame { get; set; }
	public float Density { get; set; }
	public float Friction { get; set; }
	public float InitialVelocityX { get; set; }
	public float InitialVelocityY { get; set; }
	public float Mass { get; set; }
	public bool IsStatic { get; set; }

	public PhysicsShape Shape { get; set; }
}

using FlipBook_Library.Enums;

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
	public float Elasticity { get; set; } = 0.8f; // Default elasticity (coefficient of restitution)

	public PhysicsShape Shape { get; set; }
}

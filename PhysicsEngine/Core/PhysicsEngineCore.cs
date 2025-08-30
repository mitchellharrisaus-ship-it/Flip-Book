using FlipBook_Library.Core;
using FlipBook_Library.DTOs;
using FlipBook_Library.Enums;
using FlipBook_Library.Models;

namespace PhysicsEngine.Core;
internal class PhysicsEngineCore
{
	float Height { get; set; }
	float Width { get; set; } // Fixed typo: was "Hidth"

	List<PhysicsObject> physicsObjects = new();
	PhysicsSettings worldSettings;

	public PhysicsEngineCore(Frame frame, PhysicsSettings settings)
	{
		Height = settings.Height;
		Width = settings.Width;
		worldSettings = settings;

		int counter = 0;
		foreach (var shape in frame.Actions.Where(s => s.IsPhysicsObject))
		{
			if (shape.PhysicsSettings != null)
			{
				var vertices = shape.Vertices.ToList();
				physicsObjects.Add(new PhysicsObject
				(
					counter,
					shape.PhysicsSettings,
					FindMiddlePoint(shape.PhysicsSettings.Shape, vertices),
					FindRadius(shape.PhysicsSettings.Shape, vertices)
				));
				counter++;
			}
		}
	}

	public List<TrajectoryFunction> GenerateTrajectories()
	{
		var trajectories = new List<TrajectoryFunction>();

		foreach (var physicsObject in physicsObjects)
		{
			var trajectory = GenerateTrajectory(physicsObject);
			trajectories.Add(trajectory);
		}

		return trajectories;
	}

	public TrajectoryFunction GenerateTrajectory(PhysicsObject physicsObject)
	{
		// For static objects, return a function that always returns the initial position
		if (physicsObject.Settings.IsStatic)
		{
			return new TrajectoryFunction
			{
				ObjectId = physicsObject.Id,
				OriginalAction = physicsObject,
				StartTime = 0,
				EndTime = worldSettings.TimeToMap,
				PositionFunction = (time) => physicsObject.InitialCentreOfObject
			};
		}

		// For dynamic objects, calculate projectile motion
		return CreateProjectileMotionTrajectory(physicsObject);
	}

	private TrajectoryFunction CreateProjectileMotionTrajectory(PhysicsObject physicsObject)
	{
		var initialPos = physicsObject.InitialCentreOfObject;
		var initialVelX = physicsObject.Settings.InitialVelocityX;
		var initialVelY = physicsObject.Settings.InitialVelocityY;
		var gravity = worldSettings.Gravity;
		var radius = physicsObject.Radius;

		// Calculate when object goes off-canvas (if it does)
		var actualEndTime = CalculateOffCanvasTime(initialPos, initialVelX, initialVelY, gravity, radius);
		var endTime = Math.Min(worldSettings.TimeToMap, actualEndTime);

		return new TrajectoryFunction
		{
			ObjectId = physicsObject.Id,
			OriginalAction = physicsObject,
			StartTime = 0,
			EndTime = endTime,
			PositionFunction = (time) =>
			{
				// Only calculate position if within valid time range
				if (time > endTime)
				{
					return new Vertex { X = -9999, Y = -9999 }; // Off-screen indicator
				}

				// Basic projectile motion equations
				var x = initialPos.X + initialVelX * time;
				var y = initialPos.Y + initialVelY * time + 0.5f * gravity * time * time;

				return new Vertex
				{
					X = x,
					Y = y
				};
			}
		};
	}

	private float CalculateOffCanvasTime(Vertex initialPos, float velX, float velY, float gravity, float radius)
	{
		var maxTime = worldSettings.TimeToMap;

		// Sample the trajectory at small intervals to find when it goes off-canvas
		// This is a simple approach - for more precision you could solve the equations analytically
		for (float t = 0; t <= maxTime; t += 0.01f) // Check every 10ms
		{
			var x = initialPos.X + velX * t;
			var y = initialPos.Y + velY * t + 0.5f * gravity * t * t;

			if (IsOffCanvas(x, y, radius))
			{
				return t;
			}
		}

		return maxTime; // Object stays on canvas for the entire duration
	}

	private bool IsOffCanvas(float x, float y, float radius)
	{
		return x + radius < 0 ||
			   x - radius > Width ||
			   y + radius < 0 ||
			   y - radius > Height;
	}

	// Your existing methods remain the same
	public Vertex FindMiddlePoint(PhysicsShape shape, List<Vertex> shapeCoordinates)
	{
		var furthestRightPoint = shapeCoordinates.Max(v => v.X);
		var furthestLeftPoint = shapeCoordinates.Min(v => v.X);

		var highestPoint = shapeCoordinates.Max(v => v.Y);
		var lowestPoint = shapeCoordinates.Min(v => v.Y);

		return new Vertex
		{
			X = (furthestRightPoint + furthestLeftPoint) / 2,
			Y = (highestPoint + lowestPoint) / 2
		};
	}

	public float FindRadius(PhysicsShape shape, List<Vertex> shapeCoordinates)
	{
		var radius = 0f;
		if (shape == PhysicsShape.Circle && shapeCoordinates.Count >= 2)
		{
			var furthestRightPoint = shapeCoordinates.Max(v => v.X);
			var furthestLeftPoint = shapeCoordinates.Min(v => v.X);

			radius = (furthestRightPoint - furthestLeftPoint) / 2;
		}
		return radius;
	}
}

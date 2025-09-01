using FlipBook_App.Shared.Core;
using FlipBook_Library.Core;
using FlipBook_Library.DTOs;
using FlipBook_Library.Enums;
using FlipBook_Library.Models;

namespace PhysicsEngine.Core;

public class PhysicsEngineCore
{
	float Height { get; set; }
	float Width { get; set; }

	List<PhysicsObject> physicsObjects = new();
	PhysicsSettings worldSettings;
	int topLeftCanvasX;
	int topLeftCanvasY;
	int bottomLeftCanvasY;
	int topRightCanvasX;
	int bottomRightCanvasX;
	int bottomRightCanvasY;
	float metresToPixelsYConversion;
	float metresToPixelsXConversion;

	public PhysicsEngineCore(Frame frame, PhysicsSettings settings)
	{
		Height = settings.Height;
		Width = settings.Width;
		worldSettings = settings;

		int counter = 0;
		foreach (var shape in frame.Actions)
		{
			if (shape.PhysicsSettings != null && shape.IsPhysicsObject)
			{
				var vertices = shape.Vertices.ToList();
				physicsObjects.Add(new PhysicsObject
				(
					counter,
					shape.PhysicsSettings,
					FindMiddlePoint(shape.PhysicsSettings.Shape, vertices),
					FindRadius(shape.PhysicsSettings.Shape, vertices)
				));
			}
			counter++;
		}
		
		// Calculate all canvas boundaries - NOTE: These should be 0 for canvas-relative coordinates
		topLeftCanvasX = 0; // Canvas coordinates start at 0
		topLeftCanvasY = 0; // Canvas coordinates start at 0
		topRightCanvasX = worldSettings.WidthofCanvasInPixels;
		bottomLeftCanvasY = worldSettings.HeightofCanvasInPixels;
		bottomRightCanvasX = topRightCanvasX;
		bottomRightCanvasY = bottomLeftCanvasY;
		metresToPixelsXConversion = worldSettings.WidthofCanvasInPixels / worldSettings.Width;
		metresToPixelsYConversion = worldSettings.HeightofCanvasInPixels / worldSettings.Height;
		
		// Debug output
		Console.WriteLine($"Physics Engine Setup:");
		Console.WriteLine($"  Canvas bounds: (0,0) to ({worldSettings.WidthofCanvasInPixels},{worldSettings.HeightofCanvasInPixels})");
		Console.WriteLine($"  Physics world: {worldSettings.Width}m x {worldSettings.Height}m");
		Console.WriteLine($"  Conversion: {metresToPixelsXConversion} pixels/meter");
		Console.WriteLine($"  Physics objects: {physicsObjects.Count}");
		foreach (var obj in physicsObjects)
		{
			Console.WriteLine($"    Object {obj.Id}: Center=({obj.InitialCentreOfObject.X:F1},{obj.InitialCentreOfObject.Y:F1}), Radius={obj.Radius:F1}");
		}
	}

	public List<List<PhysicsShapeInstance>> GenerateCoordinatesFromPhysics()
	{
		var trajectories = GenerateTrajectories();
		return GenerateCoordinatesFromProjectileMotionFunctions(trajectories);
	}

	public Dictionary<int, List<TrajectoryFunction>> GenerateTrajectories()
	{
		var trajectories = new Dictionary<int, List<TrajectoryFunction>>();
		
		foreach (var physicsObject in physicsObjects)
		{
			var objectTrajectories = GenerateTrajectoriesForObject(physicsObject);
			trajectories[physicsObject.Id] = objectTrajectories;
		}

		return trajectories;
	}

	public List<TrajectoryFunction> GenerateTrajectoriesForObject(PhysicsObject physicsObject)
	{
		var trajectories = new List<TrajectoryFunction>();
		
		// For static objects, return a single stationary trajectory
		if (physicsObject.Settings.IsStatic)
		{
			trajectories.Add(new TrajectoryFunction
			{
				ObjectId = physicsObject.Id,
				OriginalAction = physicsObject,
				StartTime = 0,
				EndTime = worldSettings.TimeToMap,
				PositionFunction = (time) => physicsObject.InitialCentreOfObject
			});
			return trajectories;
		}

		// For dynamic objects, calculate trajectories with possible collisions
		if (worldSettings.HasBoarder)
		{
			trajectories = GenerateTrajectoriesWithBorderCollisions(physicsObject);
		}
		else
		{
			// No borders - use the original single trajectory
			trajectories.Add(CreateProjectileMotionTrajectory(physicsObject, 
				physicsObject.InitialCentreOfObject, 
				physicsObject.Settings.InitialVelocityX, 
				physicsObject.Settings.InitialVelocityY, 
				0));
		}

		return trajectories;
	}

	private List<TrajectoryFunction> GenerateTrajectoriesWithBorderCollisions(PhysicsObject physicsObject)
	{
		var trajectories = new List<TrajectoryFunction>();
		
		// Start with initial conditions
		var currentPosition = physicsObject.InitialCentreOfObject;
		var currentVelX = physicsObject.Settings.InitialVelocityX;
		var currentVelY = physicsObject.Settings.InitialVelocityY;
		var currentTime = 0f;
		var maxCollisions = 10; // Prevent infinite loops
		var collisionCount = 0;

		Console.WriteLine($"Starting collision simulation for object {physicsObject.Id}:");
		Console.WriteLine($"  Initial position: ({currentPosition.X:F1},{currentPosition.Y:F1})");
		Console.WriteLine($"  Initial velocity: ({currentVelX:F1},{currentVelY:F1}) m/s");

		while (currentTime < worldSettings.TimeToMap && collisionCount < maxCollisions)
		{
			// Create trajectory segment from current state
			var trajectory = CreateProjectileMotionTrajectory(physicsObject, currentPosition, currentVelX, currentVelY, currentTime);
			
			// Check if this trajectory hits a border
			var collisionInfo = DetectBorderCollision(physicsObject, currentPosition, currentVelX, currentVelY, currentTime);
			
			if (collisionInfo.HasCollision && collisionInfo.CollisionTime < worldSettings.TimeToMap)
			{
				// Collision detected - truncate trajectory at collision point
				trajectory.EndTime = collisionInfo.CollisionTime;
				trajectories.Add(trajectory);
				
				// Calculate post-collision state
				var collisionPosition = trajectory.GetPositionAtTime(collisionInfo.CollisionTime);
				var (newVelX, newVelY) = CalculatePostCollisionVelocity(
					currentVelX, currentVelY, collisionInfo.CollisionSide, physicsObject.Settings.Elasticity, 
					collisionInfo.CollisionTime - currentTime);
				
				Console.WriteLine($"  Collision {collisionCount + 1} at t={collisionInfo.CollisionTime:F2}s:");
				Console.WriteLine($"    Side: {collisionInfo.CollisionSide}");
				Console.WriteLine($"    Position: ({collisionPosition.X:F1},{collisionPosition.Y:F1})");
				Console.WriteLine($"    New velocity: ({newVelX:F1},{newVelY:F1}) m/s");
				
				// Update for next trajectory segment
				currentPosition = collisionPosition;
				currentVelX = newVelX;
				currentVelY = newVelY;
				currentTime = collisionInfo.CollisionTime;
				collisionCount++;
			}
			else
			{
				// No collision - this trajectory segment goes to the end
				trajectories.Add(trajectory);
				break;
			}
		}

		return trajectories;
	}

	private CollisionInfo DetectBorderCollision(PhysicsObject physicsObject, Vertex startPosition, float velX, float velY, float startTime)
	{
		var radiusPixels = physicsObject.Radius;
		var gravity = worldSettings.Gravity;
		
		// startPosition is already in canvas coordinates (0 to 700)
		var startPosCanvas = startPosition;

		// Define collision boundaries in canvas pixels (accounting for radius)
		// These represent where the CENTER of the circle would be when the EDGE touches the boundary
		var leftBoundary = radiusPixels;
		var rightBoundary = worldSettings.WidthofCanvasInPixels - radiusPixels;
		var topBoundary = radiusPixels;
		var bottomBoundary = worldSettings.HeightofCanvasInPixels - radiusPixels;

		// Convert velocities to pixels per second
		var velXPixels = velX * metresToPixelsXConversion;
		var velYPixels = velY * metresToPixelsYConversion;
		var gravityPixels = gravity * metresToPixelsYConversion;

		var earliestCollision = new CollisionInfo { HasCollision = false, CollisionTime = float.MaxValue };

		// Check X boundaries
		if (Math.Abs(velXPixels) > 0.001f)
		{
			float timeToXCollision;
			CollisionSide xSide;
			
			if (velXPixels > 0) // Moving right
			{
				timeToXCollision = (rightBoundary - startPosCanvas.X) / velXPixels;
				xSide = CollisionSide.Right;
			}
			else // Moving left
			{
				timeToXCollision = (leftBoundary - startPosCanvas.X) / velXPixels;
				xSide = CollisionSide.Left;
			}

			if (timeToXCollision > 0 && timeToXCollision < earliestCollision.CollisionTime)
			{
				// Check if Y position is still within bounds at this time
				var yAtCollision = startPosCanvas.Y + velYPixels * timeToXCollision + 0.5f * gravityPixels * timeToXCollision * timeToXCollision;
				if (yAtCollision >= topBoundary && yAtCollision <= bottomBoundary)
				{
					earliestCollision = new CollisionInfo
					{
						HasCollision = true,
						CollisionTime = startTime + timeToXCollision,
						CollisionSide = xSide
					};
				}
			}
		}

		// Check Y boundaries
		if (Math.Abs(velYPixels) > 0.001f || Math.Abs(gravityPixels) > 0.001f)
		{
			// Check top boundary (if moving upward or upward gravity)
			if (velYPixels < 0 || gravityPixels < 0)
			{
				var timeToTopCollision = CalculateQuadraticTimeToPosition(startPosCanvas.Y, velYPixels, gravityPixels, topBoundary);
				if (timeToTopCollision > 0 && timeToTopCollision < earliestCollision.CollisionTime)
				{
					// Check if X position is still within bounds
					var xAtCollision = startPosCanvas.X + velXPixels * timeToTopCollision;
					if (xAtCollision >= leftBoundary && xAtCollision <= rightBoundary)
					{
						earliestCollision = new CollisionInfo
						{
							HasCollision = true,
							CollisionTime = startTime + timeToTopCollision,
							CollisionSide = CollisionSide.Top
						};
					}
				}
			}

			// Check bottom boundary (if moving downward or downward gravity)
			if (velYPixels > 0 || gravityPixels > 0)
			{
				var timeToBottomCollision = CalculateQuadraticTimeToPosition(startPosCanvas.Y, velYPixels, gravityPixels, bottomBoundary);
				if (timeToBottomCollision > 0 && timeToBottomCollision < earliestCollision.CollisionTime)
				{
					// Check if X position is still within bounds
					var xAtCollision = startPosCanvas.X + velXPixels * timeToBottomCollision;
					if (xAtCollision >= leftBoundary && xAtCollision <= rightBoundary)
					{
						earliestCollision = new CollisionInfo
						{
							HasCollision = true,
							CollisionTime = startTime + timeToBottomCollision,
							CollisionSide = CollisionSide.Bottom
						};
					}
				}
			}
		}

		return earliestCollision;
	}

	private (float newVelX, float newVelY) CalculatePostCollisionVelocity(float velX, float velY, CollisionSide collisionSide, float elasticity, float timeFromStart)
	{
		var gravity = worldSettings.Gravity;
		
		// Calculate velocity at collision time (accounting for gravity effect on Y velocity)
		var velYAtCollision = velY + gravity * timeFromStart;
		
		switch (collisionSide)
		{
			case CollisionSide.Left:
			case CollisionSide.Right:
				// Horizontal collision - reverse X velocity with elasticity, keep Y velocity
				return (-velX * elasticity, velYAtCollision);
				
			case CollisionSide.Top:
			case CollisionSide.Bottom:
				// Vertical collision - keep X velocity, reverse Y velocity with elasticity
				return (velX, -velYAtCollision * elasticity);
				
			default:
				return (velX, velYAtCollision);
		}
	}

	public TrajectoryFunction GenerateTrajectory(PhysicsObject physicsObject)
	{
		// This method is kept for backward compatibility
		var trajectories = GenerateTrajectoriesForObject(physicsObject);
		return trajectories.FirstOrDefault() ?? new TrajectoryFunction
		{
			ObjectId = physicsObject.Id,
			OriginalAction = physicsObject,
			StartTime = 0,
			EndTime = worldSettings.TimeToMap,
			PositionFunction = (time) => physicsObject.InitialCentreOfObject
		};
	}

	private TrajectoryFunction CreateProjectileMotionTrajectory(PhysicsObject physicsObject, Vertex startPosition, float velX, float velY, float startTime)
	{
		// startPosition is already in canvas coordinates (0-700 pixels)
		var initialPosCanvas = startPosition;

		var gravity = worldSettings.Gravity; // in m/s²
		var radiusPixels = physicsObject.Radius;

		// Convert velocities to pixels per second for consistent calculations
		var velXPixels = velX * metresToPixelsXConversion;
		var velYPixels = velY * metresToPixelsYConversion;
		var gravityPixels = gravity * metresToPixelsYConversion;

		// Calculate when object goes off-canvas (if no borders or collision system handles it)
		var actualEndTime = worldSettings.HasBoarder ? 
			worldSettings.TimeToMap : 
			CalculateOffCanvasTimeUsingSUVAT(initialPosCanvas, velXPixels, velYPixels, gravityPixels, radiusPixels);
		var endTime = Math.Min(worldSettings.TimeToMap, actualEndTime);

		return new TrajectoryFunction
		{
			ObjectId = physicsObject.Id,
			OriginalAction = physicsObject,
			StartTime = startTime,
			EndTime = endTime,
			PositionFunction = (time) =>
			{
				// Only calculate position if within valid time range
				if (time > endTime)
				{
					return new Vertex { X = -9999, Y = -9999 }; // Off-screen indicator
				}

				// Time relative to this trajectory segment start
				var relativeTime = time - startTime;

				// Basic projectile motion equations (in canvas pixels)
				var xPixels = initialPosCanvas.X + velXPixels * relativeTime;
				var yPixels = initialPosCanvas.Y + velYPixels * relativeTime + 0.5f * gravityPixels * relativeTime * relativeTime;

				return new Vertex
				{
					X = xPixels,
					Y = yPixels
				};
			}
		};
	}

	private float CalculateOffCanvasTimeUsingSUVAT(Vertex initialPosPixels, float velXPixels, float velYPixels, float gravityPixels, float radiusPixels)
	{
		var maxTime = worldSettings.TimeToMap;
		var minTimeToExit = maxTime;
		const float tolerancePixels = 1f; // 1 pixel tolerance

		// Canvas boundaries in pixels (relative to canvas origin)
		var leftBoundaryPixels = radiusPixels + tolerancePixels;
		var rightBoundaryPixels = worldSettings.WidthofCanvasInPixels - radiusPixels - tolerancePixels;
		var topBoundaryPixels = radiusPixels + tolerancePixels;
		var bottomBoundaryPixels = worldSettings.HeightofCanvasInPixels - radiusPixels - tolerancePixels;

		// Check X direction movement
		if (Math.Abs(velXPixels) > 0.001f) // Only calculate if there's significant X velocity
		{
			float timeToXBoundary;
			
			if (velXPixels > 0) // Moving right
			{
				// Calculate time to reach right boundary
				timeToXBoundary = (rightBoundaryPixels - initialPosPixels.X) / velXPixels;
			}
			else // Moving left
			{
				// Calculate time to reach left boundary
				timeToXBoundary = (leftBoundaryPixels - initialPosPixels.X) / velXPixels;
			}

			if (timeToXBoundary > 0 && timeToXBoundary < minTimeToExit)
			{
				minTimeToExit = timeToXBoundary;
			}
		}

		// Check Y direction movement
		if (Math.Abs(velYPixels) > 0.001f || Math.Abs(gravityPixels) > 0.001f) // Y movement due to velocity or gravity
		{
			// Check top boundary
			if (velYPixels < 0 || gravityPixels < 0) // Moving up or gravity pulling up
			{
				var timeToTopBoundary = CalculateQuadraticTimeToPosition(initialPosPixels.Y, velYPixels, gravityPixels, topBoundaryPixels);
				if (timeToTopBoundary > 0 && timeToTopBoundary < minTimeToExit)
				{
					minTimeToExit = timeToTopBoundary;
				}
			}

			// Check bottom boundary
			if (velYPixels > 0 || gravityPixels > 0) // Moving down or gravity pulling down
			{
				var timeToBottomBoundary = CalculateQuadraticTimeToPosition(initialPosPixels.Y, velYPixels, gravityPixels, bottomBoundaryPixels);
				if (timeToBottomBoundary > 0 && timeToBottomBoundary < minTimeToExit)
				{
					minTimeToExit = timeToBottomBoundary;
				}
			}
		}

		return minTimeToExit;
	}

	private float CalculateQuadraticTimeToPosition(float initialPosPixels, float velocityPixels, float accelerationPixels, float targetPosPixels)
	{
		// Solving: targetPos = initialPos + velocity*t + 0.5*acceleration*t²
		// Rearranged: 0.5*acceleration*t² + velocity*t + (initialPos - targetPos) = 0
		// All calculations in pixels

		var a = 0.5f * accelerationPixels;
		var b = velocityPixels;
		var c = initialPosPixels - targetPosPixels;

		// If acceleration is effectively zero, use linear equation
		if (Math.Abs(a) < 0.001f)
		{
			if (Math.Abs(b) < 0.001f) return float.MaxValue; // No movement
			return -c / b;
		}

		// Use quadratic formula: t = (-b ± √(b² - 4ac)) / 2a
		var discriminant = b * b - 4 * a * c;
		
		if (discriminant < 0) return float.MaxValue; // No real solution

		var sqrtDiscriminant = (float)Math.Sqrt(discriminant);
		var t1 = (-b + sqrtDiscriminant) / (2 * a);
		var t2 = (-b - sqrtDiscriminant) / (2 * a);

		// Return the smallest positive time
		var positiveT1 = t1 > 0 ? t1 : float.MaxValue;
		var positiveT2 = t2 > 0 ? t2 : float.MaxValue;

		return Math.Min(positiveT1, positiveT2);
	}

	public Vertex FindMiddlePoint(PhysicsShape shape, List<Vertex> shapeCoordinates)
	{
		if (shape == PhysicsShape.Circle && shapeCoordinates.Count >= 2)
		{
			// For circles, the first vertex IS the center point
			// No need to calculate center from bounding box
			return shapeCoordinates[0];
		}
		
		// For other shapes, use bounding box center
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
			// Calculate the actual radius using the center and circumference point
			var center = shapeCoordinates[0];
			var circumferencePoint = shapeCoordinates[1];
			
			// Use distance formula to get the actual radius
			radius = (float)Math.Sqrt(
				Math.Pow(circumferencePoint.X - center.X, 2) + 
				Math.Pow(circumferencePoint.Y - center.Y, 2)
			);
		}
		return radius;
	}

	public List<List<PhysicsShapeInstance>> GenerateCoordinatesFromProjectileMotionFunctions(Dictionary<int, List<TrajectoryFunction>> projectileMotionFunctions)
	{
		var result = new List<List<PhysicsShapeInstance>>();
		
		// Calculate time increment per frame
		var timePerFrame = worldSettings.TimeToMap / worldSettings.NumberOfFrames;
		
		// Generate coordinates for each frame
		for (int frameIndex = 0; frameIndex < worldSettings.NumberOfFrames; frameIndex++)
		{
			var currentTime = frameIndex * timePerFrame;
			var frameShapes = new List<PhysicsShapeInstance>();
			
			// Process each physics object
			foreach (var objectTrajectories in projectileMotionFunctions)
			{
				var objectId = objectTrajectories.Key;
				var trajectories = objectTrajectories.Value;
				
				// Find the active trajectory for this time
				var activeTrajectory = trajectories.FirstOrDefault(t => 
					currentTime >= t.StartTime && currentTime <= t.EndTime);
				
				if (activeTrajectory != null)
				{
					// Get the center position at this time (already in pixels)
					var centerPosition = activeTrajectory.GetPositionAtTime(currentTime);
					
					// Skip if object is off-screen
					if (centerPosition.X == -9999 && centerPosition.Y == -9999)
						continue;
					
					// Get the physics object to determine shape and radius
					var physicsObject = physicsObjects.FirstOrDefault(po => po.Id == objectId);
					if (physicsObject == null) continue;
					
					var shape = physicsObject.Settings.Shape;
					var radius = physicsObject.Radius; // Keep radius in pixels for display
					
					// Create PhysicsShapeInstance with the center point
					var physicsShapeInstance = new PhysicsShapeInstance(
						objectId: objectId,
						shape: shape,
						radius: radius,
						centerVertice: centerPosition
					);
					
					frameShapes.Add(physicsShapeInstance);
				}
			}
			
			// Add the frame (even if empty - represents a frame with no physics objects)
			result.Add(frameShapes);
		}
		
		return result;
	}
}

// Supporting classes for collision detection
public class CollisionInfo
{
	public bool HasCollision { get; set; }
	public float CollisionTime { get; set; }
	public CollisionSide CollisionSide { get; set; }
}

public enum CollisionSide
{
	Top,
	Bottom,
	Left,
	Right
}

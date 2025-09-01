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
		
		// Calculate all canvas boundaries
		topLeftCanvasX = (int)worldSettings.TopLeftCanvasCoordinates.X;
		topLeftCanvasY = (int)worldSettings.TopLeftCanvasCoordinates.Y;
		topRightCanvasX = topLeftCanvasX + worldSettings.WidthofCanvasInPixels;
		bottomLeftCanvasY = topLeftCanvasY + worldSettings.HeightofCanvasInPixels;
		bottomRightCanvasX = topRightCanvasX;
		bottomRightCanvasY = bottomLeftCanvasY;
		metresToPixelsXConversion = worldSettings.WidthofCanvasInPixels / worldSettings.Width;
		metresToPixelsYConversion = worldSettings.HeightofCanvasInPixels / worldSettings.Height;
	}

	public List<List<PhysicsShapeInstance>> GenerateCoordinatesFromPhysics()
	{
		var trajectories = GenerateTrajectories();
		return GenerateCoordinatesFromProjectileMotionFunctions(trajectories);
	}

	public Dictionary<int, List<TrajectoryFunction>> GenerateTrajectories()
	{
		var trajectories = new Dictionary<int, List<TrajectoryFunction>>();
		//Currently only supports one trajectory per object, but in future could support multiple (e.g. collisions)
		foreach (var physicsObject in physicsObjects)
		{
			var trajectory = GenerateTrajectory(physicsObject);
			if (!trajectories.ContainsKey(physicsObject.Id))
			{
				trajectories[physicsObject.Id] = new List<TrajectoryFunction>();
			}
			trajectories[physicsObject.Id].Add(trajectory);
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
		// Convert initial position from pixels to meters for physics calculations
		var initialPosMeters = new Vertex
		{
			X = (physicsObject.InitialCentreOfObject.X - topLeftCanvasX) / metresToPixelsXConversion,
			Y = (physicsObject.InitialCentreOfObject.Y - topLeftCanvasY) / metresToPixelsYConversion
		};

		var initialVelX = physicsObject.Settings.InitialVelocityX; // Already in m/s
		var initialVelY = physicsObject.Settings.InitialVelocityY; // Already in m/s
		var gravity = worldSettings.Gravity; // Already in m/s²
		var radiusMeters = physicsObject.Radius / metresToPixelsXConversion; // Convert radius to meters

		// Calculate when object goes off-canvas (if it does)
		var actualEndTime = CalculateOffCanvasTimeUsingSUVAT(initialPosMeters, initialVelX, initialVelY, gravity, radiusMeters);
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

				// Basic projectile motion equations (in meters)
				var xMeters = initialPosMeters.X + initialVelX * time;
				var yMeters = initialPosMeters.Y + initialVelY * time + 0.5f * gravity * time * time;

				// Convert back to pixels for display
				var xPixels = xMeters * metresToPixelsXConversion + topLeftCanvasX;
				var yPixels = yMeters * metresToPixelsYConversion + topLeftCanvasY;

				return new Vertex
				{
					X = xPixels,
					Y = yPixels
				};
			}
		};
	}

	private float CalculateOffCanvasTimeUsingSUVAT(Vertex initialPosMeters, float velX, float velY, float gravity, float radiusMeters)
	{
		var maxTime = worldSettings.TimeToMap;
		var minTimeToExit = maxTime;
		const float toleranceMeters = 0.01f; // 0.01m tolerance

		// Canvas boundaries in meters (relative to physics world origin)
		var leftBoundaryMeters = radiusMeters + toleranceMeters;
		var rightBoundaryMeters = worldSettings.Width - radiusMeters - toleranceMeters;
		var topBoundaryMeters = radiusMeters + toleranceMeters;
		var bottomBoundaryMeters = worldSettings.Height - radiusMeters - toleranceMeters;

		// Check X direction movement
		if (Math.Abs(velX) > 0.001f) // Only calculate if there's significant X velocity
		{
			float timeToXBoundary;
			
			if (velX > 0) // Moving right
			{
				// Calculate time to reach right boundary
				// Using SUVAT: s = ut + 0.5at²
				// For X direction: rightBoundary = initialPos.X + velX * t (no acceleration in X)
				timeToXBoundary = (rightBoundaryMeters - initialPosMeters.X) / velX;
			}
			else // Moving left
			{
				// Calculate time to reach left boundary
				timeToXBoundary = (leftBoundaryMeters - initialPosMeters.X) / velX;
			}

			if (timeToXBoundary > 0 && timeToXBoundary < minTimeToExit)
			{
				minTimeToExit = timeToXBoundary;
			}
		}

		// Check Y direction movement
		if (Math.Abs(velY) > 0.001f || Math.Abs(gravity) > 0.001f) // Y movement due to velocity or gravity
		{
			// For Y direction with gravity: s = ut + 0.5at²
			// Rearranged: 0.5*gravity*t² + velY*t + (initialPos.Y - boundary) = 0
			
			// Check top boundary
			if (velY < 0 || gravity < 0) // Moving up or gravity pulling up
			{
				var timeToTopBoundary = CalculateQuadraticTimeToPosition(initialPosMeters.Y, velY, gravity, topBoundaryMeters);
				if (timeToTopBoundary > 0 && timeToTopBoundary < minTimeToExit)
				{
					minTimeToExit = timeToTopBoundary;
				}
			}

			// Check bottom boundary
			if (velY > 0 || gravity > 0) // Moving down or gravity pulling down
			{
				var timeToBottomBoundary = CalculateQuadraticTimeToPosition(initialPosMeters.Y, velY, gravity, bottomBoundaryMeters);
				if (timeToBottomBoundary > 0 && timeToBottomBoundary < minTimeToExit)
				{
					minTimeToExit = timeToBottomBoundary;
				}
			}
		}

		return minTimeToExit;
	}

	private float CalculateQuadraticTimeToPosition(float initialPosMeters, float velocity, float acceleration, float targetPosMeters)
	{
		// Solving: targetPos = initialPos + velocity*t + 0.5*acceleration*t²
		// Rearranged: 0.5*acceleration*t² + velocity*t + (initialPos - targetPos) = 0
		// All calculations in meters

		var a = 0.5f * acceleration;
		var b = velocity;
		var c = initialPosMeters - targetPosMeters;

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
				
				// Currently only one trajectory per object, but designed for future collisions
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

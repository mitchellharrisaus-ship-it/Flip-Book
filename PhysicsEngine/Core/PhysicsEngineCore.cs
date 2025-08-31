using FlipBook_Library.Core;
using FlipBook_Library.DTOs;
using FlipBook_Library.Enums;
using FlipBook_Library.Models;

namespace PhysicsEngine.Core;
internal class PhysicsEngineCore
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
	static Dictionary<PhysicsShape, Func<int, Vertex, List<Vertex>>> centrePointToShapeMapping;

	public PhysicsEngineCore(Frame frame, PhysicsSettings settings)
	{
		Height = settings.Height;
		Width = settings.Width;
		worldSettings = settings;
		centrePointToShapeMapping = GenerateDictionaryOfCentrePointToShapeMapping();

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
		
		// Calculate all canvas boundaries
		topLeftCanvasX = (int)worldSettings.TopLeftCanvasCoordinates.X;
		topLeftCanvasY = (int)worldSettings.TopLeftCanvasCoordinates.Y;
		topRightCanvasX = topLeftCanvasX + worldSettings.WidthofCanvasInPixels;
		bottomLeftCanvasY = topLeftCanvasY + worldSettings.HeightofCanvasInPixels;
		bottomRightCanvasX = topRightCanvasX;
		bottomRightCanvasY = bottomLeftCanvasY;
	}

	public List<List<(PhysicsShape, int, List<Vertex>)>> GenerateCoordinatesFromPhysics()
	{
		var trajectories = GenerateTrajectories();
		return GenerateCoordinatesFromProjectileMotionFunctions(trajectories);
	}

	public static Dictionary<PhysicsShape, Func<int, Vertex, List<Vertex>>> GenerateDictionaryOfCentrePointToShapeMapping()
	{
		Dictionary<PhysicsShape, Func<int, Vertex, List<Vertex>>> centrePointToShapeMapping = new();
		
		centrePointToShapeMapping[PhysicsShape.Circle] = (radius, centre) =>
		{
			// Match the drawing system: only 2 vertices needed
			var points = new List<Vertex>
			{
				centre, // Vertex[0] = center point
				new Vertex { X = centre.X + radius, Y = centre.Y } // Vertex[1] = point on circumference (right edge)
			};
			return points;
		};

		return centrePointToShapeMapping;
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
		var initialPos = physicsObject.InitialCentreOfObject;
		var initialVelX = physicsObject.Settings.InitialVelocityX;
		var initialVelY = physicsObject.Settings.InitialVelocityY;
		var gravity = worldSettings.Gravity;
		var radius = physicsObject.Radius;

		// Calculate when object goes off-canvas (if it does)
		var actualEndTime = CalculateOffCanvasTimeUsingSUVAT(initialPos, initialVelX, initialVelY, gravity, radius);
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

	private float CalculateOffCanvasTimeUsingSUVAT(Vertex initialPos, float velX, float velY, float gravity, float radius)
	{
		var maxTime = worldSettings.TimeToMap;
		var minTimeToExit = maxTime;
		const float tolerance = 0.01f; // 0.01m tolerance as requested

		// Calculate canvas boundaries accounting for object radius
		var leftBoundary = topLeftCanvasX + radius + tolerance;
		var rightBoundary = topRightCanvasX - radius - tolerance;
		var topBoundary = topLeftCanvasY + radius + tolerance;
		var bottomBoundary = bottomLeftCanvasY - radius - tolerance;

		// Check X direction movement
		if (Math.Abs(velX) > 0.001f) // Only calculate if there's significant X velocity
		{
			float timeToXBoundary;
			
			if (velX > 0) // Moving right
			{
				// Calculate time to reach right boundary
				// Using SUVAT: s = ut + 0.5at²
				// For X direction: rightBoundary = initialPos.X + velX * t (no acceleration in X)
				timeToXBoundary = (rightBoundary - initialPos.X) / velX;
			}
			else // Moving left
			{
				// Calculate time to reach left boundary
				timeToXBoundary = (leftBoundary - initialPos.X) / velX;
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
				var timeToTopBoundary = CalculateQuadraticTimeToPosition(initialPos.Y, velY, gravity, topBoundary);
				if (timeToTopBoundary > 0 && timeToTopBoundary < minTimeToExit)
				{
					minTimeToExit = timeToTopBoundary;
				}
			}

			// Check bottom boundary
			if (velY > 0 || gravity > 0) // Moving down or gravity pulling down
			{
				var timeToBottomBoundary = CalculateQuadraticTimeToPosition(initialPos.Y, velY, gravity, bottomBoundary);
				if (timeToBottomBoundary > 0 && timeToBottomBoundary < minTimeToExit)
				{
					minTimeToExit = timeToBottomBoundary;
				}
			}
		}

		return minTimeToExit;
	}

	private float CalculateQuadraticTimeToPosition(float initialPos, float velocity, float acceleration, float targetPos)
	{
		// Solving: targetPos = initialPos + velocity*t + 0.5*acceleration*t²
		// Rearranged: 0.5*acceleration*t² + velocity*t + (initialPos - targetPos) = 0

		var a = 0.5f * acceleration;
		var b = velocity;
		var c = initialPos - targetPos;

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

	public List<List<(PhysicsShape, int, List<Vertex>)>> GenerateCoordinatesFromProjectileMotionFunctions(Dictionary<int, List<TrajectoryFunction>> projectileMotionFunctions)
	{
		var result = new List<List<(PhysicsShape, int, List<Vertex>)>>();
		
		// Calculate time increment per frame
		var timePerFrame = worldSettings.TimeToMap / worldSettings.NumberOfFrames;
		
		// Generate coordinates for each frame
		for (int frameIndex = 0; frameIndex < worldSettings.NumberOfFrames; frameIndex++)
		{
			var currentTime = frameIndex * timePerFrame;
			var frameShapes = new List<(PhysicsShape, int, List<Vertex>)>();
			
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
					// Get the center position at this time
					var centerPosition = activeTrajectory.GetPositionAtTime(currentTime);
					
					// Skip if object is off-screen
					if (centerPosition.X == -9999 && centerPosition.Y == -9999)
						continue;
					
					// Get the physics object to determine shape and radius
					var physicsObject = physicsObjects.FirstOrDefault(po => po.Id == objectId);
					if (physicsObject == null) continue;
					
					var shape = physicsObject.Settings.Shape;
					var radius = (int)physicsObject.Radius;
					
					// Generate shape vertices using the mapping function
					if (centrePointToShapeMapping.ContainsKey(shape))
					{
						var shapeVertices = centrePointToShapeMapping[shape](radius, centerPosition);
						
						// Add this object as a separate shape/stroke in the frame
						frameShapes.Add((shape, radius, shapeVertices));
					}
				}
			}
			
			// Add the frame (even if empty - represents a frame with no physics objects)
			result.Add(frameShapes);
		}
		
		return result;
	}
}

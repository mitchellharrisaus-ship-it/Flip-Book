using FlipBook_Library.Core;
using FlipBook_Library.DTOs;
using FlipBook_Library.Enums;
using SkiaSharp;

namespace FlipBook_Library.Services;

public class DrawShapeService : IDrawShapeService
{
	public void DrawShape(SKCanvas canvas, DrawActionDTO shape)
	{
		using var paint = CreateShapePaint(shape.BrushColour, shape.BrushSize);

		// Handle pen drawing directly without converting to PhysicsShape
		if (shape.Brush == BrushType.Pen)
		{
			DrawLines(canvas, shape.Vertices, paint, shape.IsPhysicsObject);
			return;
		}

		// Convert other BrushTypes to PhysicsShape for unified handling
		var shapeType = ConvertBrushTypeToPhysicsShape(shape.Brush);
		DrawShape(canvas, shapeType, shape.Vertices, shape.BrushSize, shape.BrushColour, shape.BrushSize, shape.IsPhysicsObject);
	}

	public void DrawShape(SKCanvas canvas, PhysicsShape shapeType, IList<Vertex> vertices, int radius, Colour color, int strokeWidth, bool isPhysicsObject = false)
	{
		using var paint = CreateShapePaint(color, strokeWidth);

		switch (shapeType)
		{
			case PhysicsShape.Circle:
				DrawCircle(canvas, vertices, paint, isPhysicsObject);
				break;

			case PhysicsShape.Square:
				DrawSquare(canvas, vertices, paint, isPhysicsObject);
				break;

			default:
				// Default to pen/line drawing for unknown shapes
				DrawLines(canvas, vertices, paint, isPhysicsObject);
				break;
		}
	}

	public SKPaint CreateShapePaint(Colour color, int strokeWidth)
	{
		var skColor = new SKColor((byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);

		return new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeCap = SKStrokeCap.Round,
			Color = skColor,
			StrokeWidth = strokeWidth,
			IsAntialias = true
		};
	}

	public IList<Vertex> GenerateCircleVertices(Vertex center, float radiusInPixels)
	{
		// Generate vertices compatible with the existing circle drawing system
		// The drawing system expects exactly 2 vertices:
		// - Vertex[0]: center point
		// - Vertex[1]: point on circumference (used to calculate radius)
		
		var vertices = new List<Vertex>
		{
			center, // Center point
			new Vertex 
			{ 
				X = center.X + radiusInPixels, // Point on the right edge of the circle
				Y = center.Y 
			}
		};

		return vertices;
	}

	public IList<Vertex> GenerateSquareVertices(Vertex center, float sideLength)
	{
		// Generate vertices compatible with square drawing
		// We use a similar approach to circles - center point and point defining size
		// - Vertex[0]: center point
		// - Vertex[1]: corner point (used to calculate side length)
		
		var halfSide = sideLength / 2;
		
		var vertices = new List<Vertex>
		{
			center, // Center point
			new Vertex 
			{ 
				X = center.X + halfSide,
				Y = center.Y + halfSide 
			}
		};

		return vertices;
	}

	#region Private Shape Drawing Methods

	private void DrawCircle(SKCanvas canvas, IList<Vertex> vertices, SKPaint paint, bool isPhysicsObject)
	{
		if (vertices.Count < 2) return;

		// Extract center and radius point from vertices
		var center = vertices[0];
		var radiusPoint = vertices[1];

		// Calculate radius using distance formula
		var radius = (float)Math.Sqrt(
			Math.Pow(radiusPoint.X - center.X, 2) +
			Math.Pow(radiusPoint.Y - center.Y, 2)
		);

		// Draw the main circle
		canvas.DrawCircle(center.X, center.Y, radius, paint);

		// Draw physics indicator if it's a physics object
		if (isPhysicsObject)
		{
			DrawPhysicsIndicator(canvas, center.X, center.Y, radius, isCircle: true);
		}
	}

	private void DrawSquare(SKCanvas canvas, IList<Vertex> vertices, SKPaint paint, bool isPhysicsObject)
	{
		if (vertices.Count < 2) return;

		// Extract center and corner point
		var center = vertices[0];
		var cornerPoint = vertices[1];

		// Calculate the side length using the distance from center to corner
		var distX = Math.Abs(cornerPoint.X - center.X);
		var distY = Math.Abs(cornerPoint.Y - center.Y);
		var halfSide = Math.Max(distX, distY);

		// Calculate the square's four corners
		var left = center.X - halfSide;
		var top = center.Y - halfSide;
		var right = center.X + halfSide;
		var bottom = center.Y + halfSide;

		// Draw the square
		var rect = new SKRect(left, top, right, bottom);
		canvas.DrawRect(rect, paint);

		// Draw physics indicator if it's a physics object
		if (isPhysicsObject)
		{
			DrawSquarePhysicsIndicator(canvas, center.X, center.Y, halfSide);
		}
	}

	private void DrawSquarePhysicsIndicator(SKCanvas canvas, float centerX, float centerY, float halfSide)
	{
		using var physicsPaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Orange,
			StrokeWidth = 1,
			PathEffect = SKPathEffect.CreateDash([3, 3], 0)
		};

		// Draw dashed rectangle slightly larger than the original square
		var padding = 2f;
		var rect = new SKRect(
			centerX - halfSide - padding,
			centerY - halfSide - padding,
			centerX + halfSide + padding,
			centerY + halfSide + padding
		);
		canvas.DrawRect(rect, physicsPaint);
	}

	private void DrawLines(SKCanvas canvas, IList<Vertex> vertices, SKPaint paint, bool isPhysicsObject)
	{
		if (vertices.Count < 2) return;

		// Draw lines connecting vertices (pen drawing)
		for (var i = 1; i < vertices.Count; i++)
		{
			var currentPoint = vertices[i];
			var previousPoint = vertices[i - 1];

			canvas.DrawLine(
				new SKPoint(previousPoint.X, previousPoint.Y),
				new SKPoint(currentPoint.X, currentPoint.Y),
				paint
			);
		}

		// Draw physics indicator for pen strokes if it's a physics object
		if (isPhysicsObject && vertices.Count > 1)
		{
			DrawPhysicsIndicatorForLines(canvas, vertices);
		}
	}

	private void DrawPhysicsIndicator(SKCanvas canvas, float centerX, float centerY, float radius, bool isCircle)
	{
		using var physicsPaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Orange,
			StrokeWidth = 1,
			PathEffect = SKPathEffect.CreateDash([3, 3], 0)
		};

		if (isCircle)
		{
			canvas.DrawCircle(centerX, centerY, radius + 2, physicsPaint);
		}
	}

	private void DrawPhysicsIndicatorForLines(SKCanvas canvas, IList<Vertex> vertices)
	{
		using var physicsPaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Orange,
			StrokeWidth = 1,
			PathEffect = SKPathEffect.CreateDash([2, 2], 0)
		};

		for (var i = 1; i < vertices.Count; i++)
		{
			var currentPoint = vertices[i];
			var previousPoint = vertices[i - 1];

			canvas.DrawLine(
				new SKPoint(previousPoint.X, previousPoint.Y),
				new SKPoint(currentPoint.X, currentPoint.Y),
				physicsPaint
			);
		}
	}

	private PhysicsShape ConvertBrushTypeToPhysicsShape(BrushType brushType)
	{
		return brushType switch
		{
			BrushType.Circle => PhysicsShape.Circle,
			BrushType.Square => PhysicsShape.Square,
			_ => PhysicsShape.Circle // Default fallback for other shapes
		};
	}

	#endregion
}
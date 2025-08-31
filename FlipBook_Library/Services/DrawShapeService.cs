using FlipBook_Library.Core;
using FlipBook_Library.DTOs;
using FlipBook_Library.Enums;
using SkiaSharp;

namespace FlipBook_Library.Services;  // Changed namespace

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

			// Future shapes can be added here
			// case PhysicsShape.Rectangle:
			//     DrawRectangle(canvas, vertices, paint, isPhysicsObject);
			//     break;
			// case PhysicsShape.Triangle:
			//     DrawTriangle(canvas, vertices, paint, isPhysicsObject);
			//     break;

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
			_ => PhysicsShape.Circle // Default fallback for other shapes
		};
	}

	#endregion

	#region Future Shape Methods (Template for expansion)

	// Template methods for future shapes
	/*
    private void DrawRectangle(SKCanvas canvas, IList<Vertex> vertices, SKPaint paint, bool isPhysicsObject)
    {
        if (vertices.Count < 2) return;
        
        var topLeft = vertices[0];
        var bottomRight = vertices[1];
        
        var rect = new SKRect(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
        canvas.DrawRect(rect, paint);
        
        if (isPhysicsObject)
        {
            // Draw physics indicator for rectangle
        }
    }

    private void DrawTriangle(SKCanvas canvas, IList<Vertex> vertices, SKPaint paint, bool isPhysicsObject)
    {
        if (vertices.Count < 3) return;
        
        using var path = new SKPath();
        path.MoveTo(vertices[0].X, vertices[0].Y);
        
        for (int i = 1; i < vertices.Count; i++)
        {
            path.LineTo(vertices[i].X, vertices[i].Y);
        }
        
        path.Close();
        canvas.DrawPath(path, paint);
        
        if (isPhysicsObject)
        {
            // Draw physics indicator for triangle
        }
    }
    */

	#endregion
}
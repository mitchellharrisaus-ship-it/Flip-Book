using Flipbook_App.Client.Models;
using Flipbook_App.Client.Models.DTOs;
using SkiaSharp;

namespace Flipbook_App.Client.Services;

public class SkiaDrawingService
{
	List<List<SKPoint>> Shapes { get; } = [];
	List<SKPoint> CurrentShape { get; set; } = [];
	
	public BrushType ActiveBrush { get; set; } = BrushType.Pen;
	public SKColor BrushColour { get; set; } = SKColors.Black;
	public int BrushSize { get; set; } = 2;

	bool isDrawing;

	public void HandlePointerDown(float x, float y)
	{
		isDrawing = true;
		var initialPoint = new SKPoint(x, y);

		CurrentShape = new List<SKPoint> { initialPoint };
	}

	public void HandlePointerMove(float x, float y)
	{
		if (!isDrawing)
		{
			return;
		}

		var currentPoint = new SKPoint(x, y);

		CurrentShape.Add(currentPoint);
	}

	public void HandlePointerUp()
	{
		if (!isDrawing)
		{
			return;
		}
	
		isDrawing = false;
		Shapes.Add(CurrentShape);
	}

	public void Clear()
	{
		Shapes.Clear();
		CurrentShape.Clear();

		isDrawing = false;
	}

	public DrawActionDTO GetDrawAction()
	{
		return new DrawActionDTO() 
		{
			Vertices = CurrentShape.Select(p => new Vertex { X = p.X, Y = p.Y }).ToArray(),
			Brush = ActiveBrush,
			BrushColour = new Colour { A = BrushColour.Alpha, R = BrushColour.Red, G = BrushColour.Green, B = BrushColour.Blue },
			BrushSize = BrushSize,
			ActionFrame = 0
		};
	}

	public void RecreateAnimation(IEnumerable<DrawActionDTO> actions)
	{
		foreach (var action in actions)
		{
			BrushColour = new SKColor((byte)action.BrushColour.R, (byte)action.BrushColour.G, (byte)action.BrushColour.B, (byte)action.BrushColour.A);
			BrushSize = action.BrushSize;
			ActiveBrush = action.Brush;

			HandlePointerDown(action.Vertices[0].X, action.Vertices[0].Y);
			foreach (var vertex in action.Vertices.Skip(1))
			{
				HandlePointerMove(vertex.X, vertex.Y);
			}
			HandlePointerUp();
		}
	}

	public void Draw(SKCanvas canvas)
	{
		canvas.Clear(SKColors.White);

		using var paint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeCap = SKStrokeCap.Round,

			Color = BrushColour,
			StrokeWidth = BrushSize,
			
			IsAntialias = true
		};

		foreach (var shape in Shapes)
		{
			for (var pointIndex = 1; pointIndex < shape.Count; pointIndex++)
			{
				var currentPoint = shape[pointIndex];
				var previousPoint = shape[pointIndex - 1];

				canvas.DrawLine(previousPoint, currentPoint, paint);
			}
		}

		for (var pointIndex = 1; pointIndex < CurrentShape.Count; pointIndex++)
		{
			var currentPoint = CurrentShape[pointIndex];
			var previousPoint = CurrentShape[pointIndex - 1];

			canvas.DrawLine(previousPoint, currentPoint, paint);
		}
	}
}

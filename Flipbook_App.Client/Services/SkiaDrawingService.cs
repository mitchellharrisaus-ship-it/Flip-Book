using SkiaSharp;

namespace Flipbook_App.Client.Services;

public class SkiaDrawingService
{
	List<List<SKPoint>> Shapes { get; } = [];
	List<SKPoint> CurrentShape { get; set; } = [];

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

	public void Draw(SKCanvas canvas)
	{
		canvas.Clear(SKColors.White);

		using var paint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Black,
			StrokeWidth = 3,
			StrokeCap = SKStrokeCap.Round,
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

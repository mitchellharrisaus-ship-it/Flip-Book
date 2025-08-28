using Flipbook_App.Client.Models;
using Flipbook_App.Client.Models.DTOs;
using SkiaSharp;

namespace Flipbook_App.Client.Services;

public class SkiaDrawingService : ISkiaDrawingService
{
	public List<Frame> Frames { get; } = [];

	public int currentCanvas;
	public DrawActionDTO? CurrentShape { get; set; }
	
	public BrushType ActiveBrush { get; set; } = BrushType.Pen;
	public SKColor BrushColour { get; set; } = SKColors.Black;
	public int BrushSize { get; set; } = 2;

	bool isDrawing;

	Stack<DrawActionDTO> undoneActions = [];

	public void HandlePointerDown(float x, float y)
	{
		isDrawing = true;

		CurrentShape = new()
		{
			Vertices = [new Vertex(x, y)],
			Brush = ActiveBrush,
			BrushColour = new Colour(BrushColour),
			BrushSize = BrushSize,
		};

		undoneActions.Clear();
	}

	public void HandlePointerMove(float x, float y)
	{
		if (!isDrawing)
		{
			return;
		}

		CurrentShape?.Vertices.Add(new Vertex(x, y));
	}

	public void HandlePointerUp()
	{
		if (!isDrawing)
		{
			return;
		}
	
		isDrawing = false;

		if (CurrentShape == null)
		{
			return;
		}

		Frames[currentCanvas].Actions.Push(CurrentShape);
		CurrentShape = null;
	}

	public void Clear()
	{
		Frames[currentCanvas].Actions = [];
		CurrentShape?.Vertices.Clear();

		isDrawing = false;
	}

	public void RecreateCurrentFrame()
	{
		foreach (var action in Frames[currentCanvas].Actions)
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

	public void Undo()
	{
		if (!Frames[currentCanvas].Actions.Any())
		{
			return;
		}

		var lastAction = Frames[currentCanvas].Actions.Pop();
		undoneActions.Push(lastAction);
	}

	public DrawActionDTO? Redo()
	{
		if (!undoneActions.Any())
		{
			return null;
		}

		var actionToRedo = undoneActions.Pop();
		Frames[currentCanvas].Actions.Push(actionToRedo);

		return actionToRedo;
	}

	public void Draw(SKCanvas canvas)
	{
		canvas.Clear(SKColors.White);

		foreach (var shape in Frames[currentCanvas].Actions)
		{
			DrawShape(canvas, shape);
		}

		if (CurrentShape == null)
		{
			return;
		}

		DrawShape(canvas, CurrentShape);
	}

	Guid animationID = Guid.Empty;
	Guid userID = Guid.Empty;
	string animationTitle = "MY FIRST ANIMATION";
	int frameIndex = 0;
	public Animation GetAnimation()
	{
		return new Animation
		{
			AnimationID = animationID,
			MetaData = new AnimationMetaData()
			{
				UserID = userID,
				CreatedAt = DateTime.UtcNow,
				Title = animationTitle
			},
			Frames = new List<Frame>
			{
				new Frame() { Actions = Frames[currentCanvas].Actions, FrameIndex = frameIndex }
			}
		};
	}

	static void DrawShape(SKCanvas canvas, DrawActionDTO shape)
	{
		using var paint = GetShapePaint(shape);

		for (var pointIndex = 1; pointIndex < shape.Vertices.Count; pointIndex++)
		{
			var currentPoint = shape.Vertices[pointIndex];
			var previousPoint = shape.Vertices[pointIndex - 1];

			canvas.DrawLine(new SKPoint(previousPoint.X, previousPoint.Y), new SKPoint(currentPoint.X, currentPoint.Y), paint);
		}
	}

	static SKPaint GetShapePaint(DrawActionDTO shape)
	{
		var color = new SKColor((byte)shape.BrushColour.R, (byte)shape.BrushColour.G, (byte)shape.BrushColour.B, (byte)shape.BrushColour.A);

		return new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeCap = SKStrokeCap.Round,

			Color = color,
			StrokeWidth = shape.BrushSize,

			IsAntialias = true
		};
	}
}

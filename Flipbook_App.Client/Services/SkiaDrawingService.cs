using FlipBook_Library.Core;
using FlipBook_Library.DTOs;
using SkiaSharp;

namespace Flipbook_App.Client.Services;

public class SkiaDrawingService : ISkiaDrawingService
{
	public List<Frame> Frames { get; } = [new Frame { FrameIndex = 0 }];
	
	public int CurrentFrameIndex { get; set; }

	public Frame CurrentFrame { get => Frames[CurrentFrameIndex]; }

	public DrawActionDTO? CurrentShape { get; set; }
	
	public BrushType ActiveBrush { get; set; } = BrushType.Pen;
	public SKColor BrushColour { get; set; } = SKColors.Black;
	public int BrushSize { get; set; } = 2;

	public bool IsPhysicsEnabled { get; set; } = false;
	public bool PhysicsAppliesOnShapes { get; set; } = false;
	public bool IsDrawingEnabled { get; set; } = true;
	public PhysicsSettings? CurrentPhysicsSettings { get; set; }

	bool isDrawing;

	Stack<DrawActionDTO> undoneActions = [];

	public void HandlePointerDown(float x, float y)
	{
		if (!IsDrawingEnabled) return;

		isDrawing = true;

		CurrentShape = new()
		{
			Vertices = [new Vertex(x, y)],
			Brush = ActiveBrush,
			BrushColour = new Colour(BrushColour),
			BrushSize = BrushSize,
			IsPhysicsObject = IsPhysicsEnabled && PhysicsAppliesOnShapes
		};

		undoneActions.Clear();
	}

	public void HandlePointerMove(float x, float y)
	{
		if (!IsDrawingEnabled || !isDrawing)
		{
			return;
		}

		if (ActiveBrush == BrushType.Circle)
		{
			// For circles, we only need start and current point to define the radius
			if (CurrentShape?.Vertices.Count == 1)
			{
				CurrentShape.Vertices.Add(new Vertex(x, y));
			}
			else if (CurrentShape?.Vertices.Count == 2)
			{
				// Update the second point (radius endpoint)
				CurrentShape.Vertices[1] = new Vertex(x, y);
			}
		}
		else
		{
			// Regular pen drawing
			CurrentShape?.Vertices.Add(new Vertex(x, y));
		}
	}

	public void HandlePointerUp()
	{
		if (!IsDrawingEnabled || !isDrawing)
		{
			return;
		}
	
		isDrawing = false;

		if (CurrentShape == null)
		{
			return;
		}

		CurrentFrame.Actions.Push(CurrentShape);
		CurrentShape = null;
	}

	public void Clear()
	{
		CurrentFrame.Actions = [];
		CurrentShape?.Vertices.Clear();

		isDrawing = false;
	}

	public void RecreateCurrentFrame()
	{
		foreach (var action in CurrentFrame.Actions)
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
		if (!CurrentFrame.Actions.Any())
		{
			return;
		}

		var lastAction = CurrentFrame.Actions.Pop();
		undoneActions.Push(lastAction);
	}

	public DrawActionDTO? Redo()
	{
		if (!undoneActions.Any())
		{
			return null;
		}

		var actionToRedo = undoneActions.Pop();
		CurrentFrame.Actions.Push(actionToRedo);

		return actionToRedo;
	}

	public void CreateFrame()
	{
		var newFrame = new Frame { FrameIndex = Frames.Count + 1 };
		Frames.Add(newFrame);
	}

	public void DeleteFrame(int frameIndex)
	{
		if (Frames.Count <= 1) return; // Don't delete the last canvas
		if (frameIndex < 0 || frameIndex >= Frames.Count) return; // safety guard

		Frames.RemoveAt(frameIndex);

		// Fix indices
		if (CurrentFrameIndex >= Frames.Count)
		{
			CurrentFrameIndex = Frames.Count - 1;
		}
		else if (CurrentFrameIndex > frameIndex)
		{
			CurrentFrameIndex--;
		}

		// Reassign FrameIndex numbers so they match their position
		for (var i = 0; i < Frames.Count; i++)
		{
			Frames[i].FrameIndex = i;
		}
	}

	public void Draw(SKCanvas canvas)
	{
		canvas.Clear(SKColors.White);

		foreach (var shape in CurrentFrame.Actions)
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
				new Frame() { Actions = CurrentFrame.Actions, FrameIndex = frameIndex }
			}
		};
	}

	static void DrawShape(SKCanvas canvas, DrawActionDTO shape)
	{
		using var paint = GetShapePaint(shape);

		if (shape.Brush == BrushType.Circle && shape.Vertices.Count >= 2)
		{
			// Draw circle
			var center = shape.Vertices[0];
			var radiusPoint = shape.Vertices[1];
			var radius = (float)Math.Sqrt(
				Math.Pow(radiusPoint.X - center.X, 2) + 
				Math.Pow(radiusPoint.Y - center.Y, 2)
			);

			canvas.DrawCircle(center.X, center.Y, radius, paint);

			// Only draw physics indicator if it's a physics object
			if (shape.IsPhysicsObject)
			{
				using var physicsPaint = new SKPaint
				{
					Style = SKPaintStyle.Stroke,
					Color = SKColors.Orange,
					StrokeWidth = 1,
					PathEffect = SKPathEffect.CreateDash([3, 3], 0)
				};
				canvas.DrawCircle(center.X, center.Y, radius + 2, physicsPaint);
			}
		}
		else
		{
			// Draw lines (pen)
			for (var pointIndex = 1; pointIndex < shape.Vertices.Count; pointIndex++)
			{
				var currentPoint = shape.Vertices[pointIndex];
				var previousPoint = shape.Vertices[pointIndex - 1];

				canvas.DrawLine(new SKPoint(previousPoint.X, previousPoint.Y), new SKPoint(currentPoint.X, currentPoint.Y), paint);
			}

			// Only draw physics indicator for pen strokes if it's a physics object
			if (shape.IsPhysicsObject && shape.Vertices.Count > 1)
			{
				using var physicsPaint = new SKPaint
				{
					Style = SKPaintStyle.Stroke,
					Color = SKColors.Orange,
					StrokeWidth = 1,
					PathEffect = SKPathEffect.CreateDash([2, 2], 0)
				};

				for (var pointIndex = 1; pointIndex < shape.Vertices.Count; pointIndex++)
				{
					var currentPoint = shape.Vertices[pointIndex];
					var previousPoint = shape.Vertices[pointIndex - 1];

					canvas.DrawLine(new SKPoint(previousPoint.X, previousPoint.Y), new SKPoint(currentPoint.X, currentPoint.Y), physicsPaint);
				}
			}
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

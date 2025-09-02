using FlipBook_Library.Core;
using FlipBook_Library.DTOs;
using SkiaSharp;
using FlipBook_Library.Services;
using FlipBook_Library.Enums;
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

	IList<Stack<DrawActionDTO>> undoneActions = [new()];

	readonly IDrawShapeService drawShapeService;

	public SkiaDrawingService(IDrawShapeService drawShapeService)
	{
		this.drawShapeService = drawShapeService ?? throw new ArgumentNullException(nameof(drawShapeService));
	}

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

		// Clear redo stack if we start a new action
		undoneActions[CurrentFrameIndex].Clear();
	}

	public void HandlePointerMove(float x, float y)
	{
		if (!IsDrawingEnabled || !isDrawing)
		{
			return;
		}

		if (ActiveBrush == BrushType.Circle || ActiveBrush == BrushType.Square)
		{
			// For shapes, we only need start and current point to define the size
			if (CurrentShape?.Vertices.Count == 1)
			{
				CurrentShape.Vertices.Add(new Vertex(x, y));
			}
			else if (CurrentShape?.Vertices.Count == 2)
			{
				// Update the second point (size endpoint)
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

		// If it's a physics object, initialize physics settings
		if (CurrentShape.IsPhysicsObject)
		{
			CurrentShape.PhysicsSettings = new PhysicsObjectSettings
			{
				Elasticity = 0.8f,
				Friction = 0.3f,
				Mass = 1.0f,
				Density = 1.0f,
				Shape = ActiveBrush == BrushType.Square ? PhysicsShape.Square : PhysicsShape.Circle
			};
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

	public void ClearAllFrames()
	{
		// Save the first frame but clear its actions
		var firstFrame = Frames.FirstOrDefault();
		
		// Clear everything
		Frames.Clear();
		undoneActions.Clear();
		
		// Re-add the first frame (empty)
		if (firstFrame != null)
		{
			firstFrame.Actions.Clear();
			firstFrame.FrameIndex = 0;
			Frames.Add(firstFrame);
		}
		else
		{
			// If somehow there was no first frame, create a new one
			Frames.Add(new Frame { FrameIndex = 0 });
		}
		
		// Add back a corresponding undone actions stack
		undoneActions.Add(new Stack<DrawActionDTO>());
		
		// Reset the current frame index
		CurrentFrameIndex = 0;
		
		// Ensure we're not in a drawing state
		isDrawing = false;
		CurrentShape = null;
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
		undoneActions[CurrentFrameIndex].Push(lastAction);
	}

	public DrawActionDTO? Redo()
	{
		if (undoneActions.ElementAtOrDefault(CurrentFrameIndex) == null || !undoneActions[CurrentFrameIndex].Any())
		{
			return null;
		}

		var actionToRedo = undoneActions[CurrentFrameIndex].Pop();
		CurrentFrame.Actions.Push(actionToRedo);

		return actionToRedo;
	}

	public void LoadAnimation(Animation? animation)
	{
		if (animation == null || animation.Frames == null || !animation.Frames.Any())
		{
			throw new ArgumentNullException(nameof(animation));
		}

		// reinitialise
		Frames.Clear();
		undoneActions.Clear();

		foreach (var frame in animation.Frames)
		{
			CreateFrame(frame);
		}
		
		if (CurrentFrameIndex >= animation.Frames.Count)
		{
			CurrentFrameIndex = animation.Frames.Count - 1;
		}
	}

	public void CreateFrame(Frame? givenFrame = null)
	{
		undoneActions.Add(new());

		if (givenFrame != null)
		{
			Frames.Add(givenFrame);
			return;
		}

		// Create a new blank frame
		var newFrame = new Frame { FrameIndex = Frames.Count };
		Frames.Add(newFrame);
	}

	public void DeleteFrame(int frameIndex)
	{
		if (Frames.Count <= 1) return; // Don't delete the last canvas
		if (frameIndex < 0 || frameIndex >= Frames.Count) return; // safety guard

		Frames.RemoveAt(frameIndex);

		// Sync undo / redo stacks
		if (undoneActions.ElementAtOrDefault(frameIndex) != null)
		{
			undoneActions.RemoveAt(frameIndex);
		}

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

		// Draw actions in the order they were added (oldest first, newest last)
		var actions = CurrentFrame.Actions.Reverse();
		foreach (var shape in actions)
		{
			DrawShape(canvas, shape);
		}

		if (CurrentShape == null)
		{
			return;
		}

		DrawShape(canvas, CurrentShape);
	}

	static void DrawShape(SKCanvas canvas, DrawActionDTO shape)
	{
		if (shape.Brush == BrushType.Lasso)
		{
			if (shape.Vertices.Count >= 3)
			{
				var path = new SKPath();
				path.MoveTo(shape.Vertices[0].X, shape.Vertices[0].Y);
				for (int i = 1; i < shape.Vertices.Count; i++)
				{
					path.LineTo(shape.Vertices[i].X, shape.Vertices[i].Y);
				}
				path.Close();
				var fillPaint = new SKPaint
				{
					Style = SKPaintStyle.Fill,
					Color = new SKColor((byte)shape.BrushColour.R, (byte)shape.BrushColour.G, (byte)shape.BrushColour.B, (byte)shape.BrushColour.A),
					IsAntialias = true
				};
				canvas.DrawPath(path, fillPaint);
			}
			return;
		}

		using var paint = GetShapePaint(shape);

		if (shape.Brush == BrushType.Circle)
		{
			// Circle: draw a proper circle using center and radius points
			if (shape.Vertices.Count >= 2)
			{
				var center = shape.Vertices[0];
				var radiusPoint = shape.Vertices[1];

				// Calculate radius using distance formula
				var radius = (float)Math.Sqrt(
					Math.Pow(radiusPoint.X - center.X, 2) +
					Math.Pow(radiusPoint.Y - center.Y, 2)
				);

				// Draw the main circle
				canvas.DrawCircle(center.X, center.Y, radius, paint);

				// Draw physics indicator if it's a physics object
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
		}
		else if (shape.Brush == BrushType.Square)
		{
			// Square: draw a square using center and corner points
			if (shape.Vertices.Count >= 2)
			{
				var center = shape.Vertices[0];
				var cornerPoint = shape.Vertices[1];

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
				if (shape.IsPhysicsObject)
				{
					using var physicsPaint = new SKPaint
					{
						Style = SKPaintStyle.Stroke,
						Color = SKColors.Orange,
						StrokeWidth = 1,
						PathEffect = SKPathEffect.CreateDash([3, 3], 0)
					};
					
					// Draw slightly larger rectangle for physics indicator
					var physicsRect = new SKRect(
						left - 2, 
						top - 2, 
						right + 2, 
						bottom + 2
					);
					canvas.DrawRect(physicsRect, physicsPaint);
				}
			}
		}
		else if (shape.Brush == BrushType.Eraser)
		{
			// Eraser: draw thick white lines to overwrite previous drawings
			paint.Color = SKColors.White;
			paint.StrokeWidth = shape.BrushSize * 6;
			paint.BlendMode = SKBlendMode.SrcOver;
			for (var pointIndex = 1; pointIndex < shape.Vertices.Count; pointIndex++)
			{
				var currentPoint = shape.Vertices[pointIndex];
				var previousPoint = shape.Vertices[pointIndex - 1];
				canvas.DrawLine(new SKPoint(previousPoint.X, previousPoint.Y), new SKPoint(currentPoint.X, currentPoint.Y), paint);
			}
		}
		else if (shape.Brush == BrushType.Chain)
		{
			// Chain: draw a solid block (rectangle) between each pair of points
			for (var pointIndex = 1; pointIndex < shape.Vertices.Count; pointIndex++)
			{
				var prev = shape.Vertices[pointIndex - 1];
				var curr = shape.Vertices[pointIndex];
				var halfWidth = shape.BrushSize;
				var top = Math.Min(prev.Y, curr.Y) - halfWidth;
				var bottom = Math.Max(prev.Y, curr.Y) + halfWidth;
				var left = Math.Min(prev.X, curr.X) - halfWidth;
				var right = Math.Max(prev.X, curr.X) + halfWidth;
				var rect = new SKRect(left, top, right, bottom);
				canvas.DrawRect(rect, paint);
			}
		}
		else if (shape.Brush == BrushType.Marker)
		{
			// Classic fountain pen: vertical lines at each vertex, connect endpoints, fill area, using brush width for thickness
			if (shape.Vertices.Count > 1)
			{
				var topPoints = new List<SKPoint>();
				var bottomPoints = new List<SKPoint>();
				int halfHeight = Math.Max(shape.BrushSize * 3, 2); // vertical thickness
				foreach (var v in shape.Vertices)
				{
					topPoints.Add(new SKPoint(v.X, v.Y - halfHeight));
					bottomPoints.Add(new SKPoint(v.X, v.Y + halfHeight));
				}
				var polygon = new List<SKPoint>();
				polygon.AddRange(topPoints);
				bottomPoints.Reverse();
				polygon.AddRange(bottomPoints);
				using var path = new SKPath();
				path.FillType = SKPathFillType.Winding;
				path.AddPoly(polygon.ToArray(), true);
				paint.Style = SKPaintStyle.Fill;
				paint.BlendMode = SKBlendMode.SrcOver;
				canvas.DrawPath(path, paint);
			}
		}
		else if (shape.Brush == BrushType.Kaleidoscope)
		{
			// Kaleidoscope: wide filled polygon effect (always visible, even for vertical strokes)
			if (shape.Vertices.Count > 1)
			{
				var topPoints = new List<SKPoint>();
				var bottomPoints = new List<SKPoint>();
				int halfHeight = Math.Max(shape.BrushSize * 2, 1);
				int halfWidth = Math.Max(shape.BrushSize * 3, 6);
				foreach (var v in shape.Vertices)
				{
					topPoints.Add(new SKPoint(v.X - halfWidth, v.Y - halfHeight));
					topPoints.Add(new SKPoint(v.X + halfWidth, v.Y - halfHeight));
					bottomPoints.Add(new SKPoint(v.X + halfWidth, v.Y + halfHeight));
					bottomPoints.Add(new SKPoint(v.X - halfWidth, v.Y + halfHeight));
				}
				var polygon = new List<SKPoint>();
				polygon.AddRange(topPoints);
				bottomPoints.Reverse();
				polygon.AddRange(bottomPoints);
				using var path = new SKPath();
				path.AddPoly(polygon.ToArray(), true);
				paint.Style = SKPaintStyle.Fill;
				paint.BlendMode = SKBlendMode.SrcOver;
				canvas.DrawPath(path, paint);
			}
		}
		else
		{
			// Pen: normal stroke
			for (var pointIndex = 1; pointIndex < shape.Vertices.Count; pointIndex++)
			{
				var currentPoint = shape.Vertices[pointIndex];
				var previousPoint = shape.Vertices[pointIndex - 1];
				canvas.DrawLine(new SKPoint(previousPoint.X, previousPoint.Y), new SKPoint(currentPoint.X, currentPoint.Y), paint);
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

	public void SetBrushColor(string color)
	{
		// Convert hex string to SKColor
		if (!string.IsNullOrWhiteSpace(color) && color.StartsWith("#") && color.Length == 7)
		{
			BrushColour = SKColor.Parse(color);
		}
	}

	public IList<byte[]> RenderThumbnails(int thumbWidth = 400, int thumbHeight = 400)
	{
		var thumbnails = new List<byte[]>();

		var sampleFrames = Frames.Count > 3
			? new() { Frames.First(), Frames.ElementAt(Frames.Count / 2), Frames.Last() }
			: Frames;

		foreach (var frame in sampleFrames)
		{
			// Create the thumbnail bitmap
			using var thumbBitmap = new SKBitmap(thumbWidth, thumbHeight);
			using (var thumbCanvas = new SKCanvas(thumbBitmap))
			{
				thumbCanvas.Clear(SKColors.White);

				float scaleX = thumbWidth / 700f;
				float scaleY = thumbHeight / 700f;

				// Draw each shape directly scaled
				foreach (var shape in frame.Actions)
				{
					DrawShapeScaled(thumbCanvas, shape, scaleX, scaleY);
				}
			}

			using var thumbImage = SKImage.FromBitmap(thumbBitmap);
			using var data = thumbImage.Encode(SKEncodedImageFormat.Webp, 80);
			thumbnails.Add(data.ToArray());
		}

		return thumbnails;
	}

	public IList<SKData> RenderAnimation(int renderQuality = 100, int width = 700, int height = 700)
	{
		var renderedFrames = new List<SKData>();

		foreach (var frame in Frames)
		{
			var frameBitmaps = new List<SKBitmap>();

			var bitmap = new SKBitmap(width, height);
			using var canvas = new SKCanvas(bitmap);
			canvas.Clear(SKColors.Transparent);

			foreach (var shape in frame.Actions)
			{
				DrawShape(canvas, shape);
			}

			var rendered = SKImage.FromBitmap(bitmap).Encode(SKEncodedImageFormat.Png, renderQuality);
			renderedFrames.Add(rendered);
		}

		return renderedFrames;
	}

	static void DrawShapeScaled(SKCanvas canvas, DrawActionDTO shape, float scaleX, float scaleY)
	{
		var newShape = new DrawActionDTO
		{
			Brush = shape.Brush,
			BrushColour = shape.BrushColour,
			BrushSize = Math.Max(1, (int)(shape.BrushSize * ((scaleX + scaleY) / 2))), // average scale for size
			IsPhysicsObject = shape.IsPhysicsObject,
			Vertices = shape.Vertices.Select(v => new Vertex(v.X * scaleX, v.Y * scaleY)).ToList()
		};

		DrawShape(canvas, newShape);
	}
}

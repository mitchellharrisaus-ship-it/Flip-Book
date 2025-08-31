using FlipBook_Library.Core;
using FlipBook_Library.DTOs;
using SkiaSharp;
using FlipBook_Library.Services;
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

	IList<Stack<DrawActionDTO>> undoneActions = [];

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

		if (undoneActions.ElementAtOrDefault(CurrentFrameIndex) == null)
		{
			undoneActions.Add(new Stack<DrawActionDTO>());
		}
		else
		{
			// Clear redo stack if we start a new action
			undoneActions[CurrentFrameIndex].Clear();
		}
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

	void DrawShape(SKCanvas canvas, DrawActionDTO shape)
	{
		drawShapeService.DrawShape(canvas, shape);
	}
}

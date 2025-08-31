using FlipBook_App.Shared.Core;
using FlipBook_Library.Core;
using FlipBook_Library.DTOs;
using FlipBook_Library.Services;
using PhysicsEngine.Core;

namespace Flipbook_App.Client.Services;

public class FrameGeneratorService
{
	IDrawShapeService drawShapeService;
	public FrameGeneratorService()
	{
		drawShapeService = new DrawShapeService();
	}
	//Calls some Controller in Flipbook_App.Server to generate frames with physics applied
	//Then applies these frames to the current animation in SkiaDrawingService
	Dictionary<int, DrawActionDTO> physicsDrawActions = new();
	public void GenerateFrames(List<Frame> frames, PhysicsSettings worldSettings)
	{
		physicsDrawActions = new();
		var i = 0;
		foreach (var drawAction in frames.First().Actions)
		{
			if (drawAction.IsPhysicsObject)
			{
				physicsDrawActions.Add(i, drawAction);
			}
			i++;
		}

		var physicsEngine = new PhysicsEngineCore(frames.First(), worldSettings);
		var generatedFrames = physicsEngine.GenerateCoordinatesFromPhysics();

		for (int count = i; i <= worldSettings.NumberOfFrames; count++)
		{
			Frame frame;

			if (count < frames.Count)
			{
				frame = frames[count];
			}
			else
			{
				frame = new Frame { FrameIndex = count };
				frames.Add(frame);
			}
			// Get all physics shape instances for this frame
			// Map them to DrawActionDTOs
			// Push the new DrawActionDTOs to the frame's Actions list
		}

	}

	public DrawActionDTO MapPhysiscsShapeInstanceToDrawAction(PhysicsShapeInstance shapeInstance)
	{
		var baseDrawAction = physicsDrawActions[shapeInstance.ObjectId];
		return new DrawActionDTO
		{
			Vertices = MapCentreToShape(shapeInstance),
			Brush = baseDrawAction.Brush,
			BrushColour = baseDrawAction.BrushColour,
			BrushSize = baseDrawAction.BrushSize,
			ActionFrame = baseDrawAction.ActionFrame,
			IsPhysicsObject = true,
			PhysicsSettings = baseDrawAction.PhysicsSettings
		};
	}

	public IList<Vertex> MapCentreToShape(PhysicsShapeInstance shapeInstance)
	{
		// Use the new GenerateCircleVertices method to convert center + radius to vertices
		return drawShapeService.GenerateCircleVertices(shapeInstance.CenterVertice, shapeInstance.Radius);
	}
}

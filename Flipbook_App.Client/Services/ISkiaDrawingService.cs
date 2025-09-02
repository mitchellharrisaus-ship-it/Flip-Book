using FlipBook_Library.Core;
using FlipBook_Library.DTOs;
using SkiaSharp;

namespace Flipbook_App.Client.Services;

public interface ISkiaDrawingService
{
	List<Frame> Frames { get; }
	int CurrentFrameIndex { get; set; }
	Frame CurrentFrame { get; }
	DrawActionDTO? CurrentShape { get; set; }
	BrushType ActiveBrush { get; set; }
	SKColor BrushColour { get; set; }
	int BrushSize { get; set; }
	bool IsPhysicsEnabled { get; set; }
	bool PhysicsAppliesOnShapes { get; set; }
	bool IsDrawingEnabled { get; set; }
	PhysicsSettings? CurrentPhysicsSettings { get; set; }

	void HandlePointerDown(float x, float y);
	void HandlePointerUp();
	void HandlePointerMove(float x, float y);
	void Clear();
	void RecreateCurrentFrame();

	void Undo();
	DrawActionDTO? Redo();

	void LoadAnimation(Animation? animation);

	void CreateFrame(Frame? givenFrame = null);
	void DeleteFrame(int frameIndex);

	void Draw(SKCanvas canvas);

	void ToggleOnionLayer();

	IList<byte[]> RenderThumbnails(int thumbWidth = 200, int thumbHeight = 150);

	IList<SKData> RenderAnimation(int renderQuality = 100, int width = 700, int height = 700);

	void ClearAllFrames();
}

using Flipbook_App.Client.Models.DTOs;
using SkiaSharp;

namespace Flipbook_App.Client.Services;

public interface ISkiaDrawingService
{
	void HandlePointerDown(float x, float y);

	void HandlePointerUp();

	void HandlePointerMove(float x, float y);

	void RecreateCurrentFrame();

	void Undo();

	DrawActionDTO? Redo();

	void LoadAnimation(Animation? animation);

	void CreateFrame(Frame? givenFrame = null);

	void DeleteFrame(int frameIndex);

	void Draw(SKCanvas canvas);

	IList<SKData> RenderAnimation(SKEncodedImageFormat format = SKEncodedImageFormat.Png, int renderQuality = 100, int width = 800, int height = 600);
}

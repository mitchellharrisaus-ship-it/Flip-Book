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

	void Draw(SKCanvas canvas);

	Animation GetAnimation();
}

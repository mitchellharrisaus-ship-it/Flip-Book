using Flipbook_App.Client.Models.DTOs;
using SkiaSharp;

namespace Flipbook_App.Client.Services;

public interface IDrawActionApiService
{
	Task<string> EnsureValidTitle(string currentTitle);

	Task Save(IEnumerable<Frame> animationFrames, string animationTitle);

	Task<Animation> Load(Guid animationID);

	Task Export(IList<SKData> renderedFrames, string animationTitle);
}

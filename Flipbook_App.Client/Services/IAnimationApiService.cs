using SkiaSharp;
using FlipBook_Library.DTOs;
using FlipBook_App.Shared.DTOs;

namespace Flipbook_App.Client.Services;

public interface IAnimationApiService
{
	Task<string> EnsureValidTitle(string currentTitle);

	Task Save(IEnumerable<Frame> animationFrames, string animationTitle);

	Task<Animation> Load(Guid animationID);

	Task Export(IList<SKData> renderedFrames, string animationTitle, ExportOptions exportOptions);
}

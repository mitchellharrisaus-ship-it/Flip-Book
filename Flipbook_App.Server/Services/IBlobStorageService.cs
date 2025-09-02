using FlipBook_Library.DTOs;

namespace Flipbook_App.Services;

public interface IBlobStorageService
{
	Task UploadAnimation(Animation animation);

	Task<Animation> DownloadAnimation(Guid animationID);

	Task DeleteAnimation(Guid animationID);

	Task UploadThumbnails(Guid animationID, List<byte[]> thumbnails);

	Task<List<Uri>> GetThumbnails(Guid animationID);
}

using Flipbook_App.Models.DTOs;

namespace Flipbook_App.Services;

public interface IBlobStorageService
{
	Task UploadAnimation(Animation animation);

	Task<Animation> DownloadAnimation(Guid animationID);
}

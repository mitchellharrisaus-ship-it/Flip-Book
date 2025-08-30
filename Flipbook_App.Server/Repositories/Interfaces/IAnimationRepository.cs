using Flipbook_App.Models;
using Flipbook_App.Models.DTOs;

namespace Flipbook_App.Repositories.Interfaces;

public interface IAnimationRepository : IRepository<AnimationReference>
{
	void CreateIfNotExists(Animation animation, string animationTitle);

	void RenameAnimation(Guid animationID, string newName);

	AnimationReference? GetByTitleAndUserID(string animationTitle, Guid userID);
}

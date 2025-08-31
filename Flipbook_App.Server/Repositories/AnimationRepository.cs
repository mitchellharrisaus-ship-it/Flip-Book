using Flipbook_App.Data;
using Flipbook_App.Repositories.Interfaces;
using FlipBook_App.Shared.Core;
using FlipBook_Library.DTOs;

namespace Flipbook_App.Repositories;

public class AnimationRepository : Repository<AnimationReference>, IAnimationRepository
{
	public AnimationRepository(FlipbookDBContext context) : base(context)
	{
	}

	public void CreateIfNotExists(Animation animation, string animationTitle)
	{
		var existingAnimation = context.Animations.Find(animation.AnimationID);

		if (existingAnimation != null)
		{
			return;
		}

		var animationReference = new AnimationReference
		{
			AnimationID = animation.AnimationID,
			UserID = animation.MetaData.UserID,
			Title = animationTitle,
			CreatedAt = DateTime.UtcNow
		};

		context.Animations.Add(animationReference);
	}

	public void RenameAnimation(Guid animationID, string newName)
	{
		throw new NotImplementedException();
	}

	public AnimationReference? GetByTitleAndUserID(string animationTitle, Guid userID)
	{
		var found = context.Animations.FirstOrDefault(a => a.Title == animationTitle && a.UserID == userID);
		if (found == null) return null;

		return new AnimationReference
		{
			AnimationID = found.AnimationID,
			Title = found.Title,
			UserID = found.UserID,
		};
	}
}

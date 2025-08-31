using Flipbook_App.Data;
using Flipbook_App.Repositories.Interfaces;

namespace Flipbook_App.Repositories;

public class RepositoryManager
{
	readonly FlipbookDBContext context;

	public IUserRepository Users { get; }

	public IAnimationRepository Animations { get; }

	public RepositoryManager(FlipbookDBContext context, IUserRepository users, IAnimationRepository animations)
	{
		this.context = context ?? throw new ArgumentNullException(nameof(context));

		Users = users ?? throw new ArgumentNullException(nameof(users));
		Animations = animations ?? throw new ArgumentNullException(nameof(animations));
	}

	public async Task SaveChangesAsync()
	{
		await context.SaveChangesAsync();
	}
}
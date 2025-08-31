using Flipbook_App.Data;
using Flipbook_App.Repositories.Interfaces;
using FlipBook_App.Shared.Core;
using FlipBook_Library.Core;
using System.Data.Entity;

namespace Flipbook_App.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
	public UserRepository(FlipbookDBContext context) : base(context)
	{
	}

	public IEnumerable<User> GetAllUsers()
	{
		return context.Users
			.Include(u => u.Animations)
			.ToList();
	}

	// the include animations does NOT WORK AND I CANT FIGURE OUT WHY
	// but this does, so use this
	public IList<AnimationReference> GetUserAnimations(string username)
	{
		return (from a in context.Animations
				join u in context.Users
				on a.UserID equals u.Id
				where u.Username == username
				orderby a.CreatedAt descending
				select a)
			   .ToList();
	}

	public User? GetByUsername(string username)
	{
		return context.Users
			.Include(u => u.Animations)
			.FirstOrDefault(u => u.Username == username);
	}
}

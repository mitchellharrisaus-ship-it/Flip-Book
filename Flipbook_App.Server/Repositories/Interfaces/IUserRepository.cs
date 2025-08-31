using FlipBook_App.Shared.Core;
using FlipBook_Library.Core;

namespace Flipbook_App.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
	IEnumerable<User> GetAllUsers();

	IList<AnimationReference> GetUserAnimations(string username);

	User? GetByUsername(string username);
}

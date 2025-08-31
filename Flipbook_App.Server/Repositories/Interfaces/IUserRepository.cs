using Flipbook_App.Models;

namespace Flipbook_App.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
	IEnumerable<User> GetAllUsers();

	IList<AnimationReference> GetUserAnimations(string username);

	User? GetByUsername(string username);
}

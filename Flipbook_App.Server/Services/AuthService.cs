using Flipbook_App.Repositories;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Flipbook_App.Services;

public class AuthService
{
	RepositoryManager repositoryManager;

	public AuthService(RepositoryManager repositoryManager)
	{
		this.repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
	}

	public bool IsUsernameTaken(string username)
	{
		return repositoryManager.Users.GetByUsername(username) != null;
	}

	public string GeneratePassword(string password)
	{
		var salt = RandomNumberGenerator.GetBytes(16);
		var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
			password: password,
			salt: salt,
			prf: KeyDerivationPrf.HMACSHA256,
			iterationCount: 100_000,
			numBytesRequested: 32));

		return $"{Convert.ToBase64String(salt)}:{hash}";
	}
}

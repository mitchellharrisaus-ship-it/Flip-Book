using FlipbookApp.Data;
using FlipbookApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using FlipBook_Library.Core;

namespace Flipbook_App.Pages;

public class RegisterModel : PageModel
{
	private readonly RepositoryManager repositoryManager;

	public RegisterModel(RepositoryManager repositoryManager)
	{
		this.repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
	}

	[BindProperty]
	public RegisterInput Input { get; set; }

	public class RegisterInput
	{
		[Required]
		public string Username { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}

	public void OnGet() { }

	public async Task<IActionResult> OnPostAsync()
	{
		if (!ModelState.IsValid)
			return Page();

		// Check for unique username
		if (repositoryManager.Users.GetByUsername(Input.Username) != null)
		{
			ModelState.AddModelError("Input.Username", "Username is already taken.");
			return Page();
		}

		// Hash password and save user
		var salt = RandomNumberGenerator.GetBytes(16);
		var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
			password: Input.Password,
			salt: salt,
			prf: KeyDerivationPrf.HMACSHA256,
			iterationCount: 100_000,
			numBytesRequested: 32));

		var user = new User
		{
			Username = Input.Username,
			PasswordHash = $"{Convert.ToBase64String(salt)}:{hash}"
		};

		repositoryManager.Users.Add(user);
		await repositoryManager.SaveChangesAsync();

		// Auto-login
		var claims = new List<Claim>
	{
		new Claim(ClaimTypes.Name, user.Username)
	};
		var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			new ClaimsPrincipal(claimsIdentity));

		return RedirectToPage("/Index");
	}

}

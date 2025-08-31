using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Flipbook_App.Repositories;

namespace Flipbook_App.Pages;

public class LoginModel : PageModel
{
	private readonly RepositoryManager repositoryManager;

	public LoginModel(RepositoryManager repositoryManager)
	{
		this.repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
	}

	[BindProperty]
	public LoginInput Input { get; set; }

	public class LoginInput
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

		// Find user in DB
		var user = repositoryManager.Users.GetByUsername(Input.Username);
		if (user == null)
		{
			ModelState.AddModelError("", "Invalid username or password.");
			return Page();
		}

		// Verify password hash
		var parts = user.PasswordHash.Split(':');
		if (parts.Length != 2)
		{
			ModelState.AddModelError("", "Invalid password data.");
			return Page();
		}

		var salt = Convert.FromBase64String(parts[0]);
		var hash = parts[1];

		var attemptedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
			password: Input.Password,
			salt: salt,
			prf: KeyDerivationPrf.HMACSHA256,
			iterationCount: 100_000,
			numBytesRequested: 32));

		if (attemptedHash != hash)
		{
			ModelState.AddModelError("", "Invalid username or password.");
			return Page();
		}

		// Login successful — create claims
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.Name, user.Username)
		};

		var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			new ClaimsPrincipal(claimsIdentity),
			new AuthenticationProperties { IsPersistent = true });

		return RedirectToPage("/Projects");
	}
}

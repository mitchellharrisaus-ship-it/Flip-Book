using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using FlipBook_Library.Core;
using Flipbook_App.Repositories;
using Flipbook_App.Services;

namespace Flipbook_App.Pages;

public class RegisterModel : PageModel
{
	private readonly RepositoryManager repositoryManager;
	private readonly AuthService authService;

	public RegisterModel(RepositoryManager repositoryManager, AuthService authService)
	{
		this.repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
		this.authService = authService ?? throw new ArgumentNullException(nameof(authService));
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
		if (authService.IsUsernameTaken(Input.Username))
		{
			ModelState.AddModelError("Input.Username", "Username is already taken.");
			return Page();
		}

		// Hash password and save user
		var hashedPassword = authService.GeneratePassword(Input.Password);
		var user = new User
		{
			Username = Input.Username,
			PasswordHash = hashedPassword
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

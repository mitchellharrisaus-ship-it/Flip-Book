using Flipbook_App.Repositories;
using FlipBook_Library.Core;
using Flipbook_App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace Flipbook_App.Pages;

public class SettingsModel : PageModel
{
	[BindProperty]
	public string Username { get; set; } = string.Empty;

	[BindProperty]
	public string Password { get; set; } = string.Empty;

	[BindProperty]
	public string ConfirmPassword { get; set; } = string.Empty;

	public string? ErrorMessage { get; set; }
	public string? SuccessMessage { get; set; }

	private readonly RepositoryManager repositoryManager;
	private readonly AuthService authService;

	public SettingsModel(RepositoryManager repositoryManager, AuthService authService)
	{
		this.repositoryManager = repositoryManager;
		this.authService = authService;
	}

	User? currentUser;

	public void OnGet()
	{
		if (User.Identity?.IsAuthenticated ?? false)
		{
			currentUser = repositoryManager.Users.GetByUsername(User.Identity.Name!);
			if (currentUser != null)
			{
				Username = currentUser.Username;
			}
		}
	}

	public async Task<IActionResult> OnPostAsync()
	{
		var usernameChanged = false;

		if (!(User.Identity?.IsAuthenticated ?? false))
		{
			ErrorMessage = "You must be logged in.";
			return Page();
		}

		currentUser = repositoryManager.Users.GetByUsername(User.Identity.Name!);
		if (currentUser == null)
		{
			ErrorMessage = "User not found.";
			return Page();
		}

		// Change Username
		if (!string.IsNullOrWhiteSpace(Username) && Username != currentUser.Username)
		{
			if (authService.IsUsernameTaken(Username))
			{
				ErrorMessage = "Username is already taken.";
				return Page();
			}
			currentUser.Username = Username;
			SuccessMessage = "Username name updated.";
			usernameChanged = true;
		}

		// Change Password
		if (!string.IsNullOrWhiteSpace(Password) || !string.IsNullOrWhiteSpace(ConfirmPassword))
		{
			if (Password != ConfirmPassword)
			{
				ErrorMessage = "Passwords do not match.";
				return Page();
			}
			if (string.IsNullOrWhiteSpace(Password))
			{
				ErrorMessage = "Password cannot be empty.";
				return Page();
			}
			currentUser.PasswordHash = authService.GeneratePassword(Password);
			SuccessMessage = "Password updated.";
		}

		repositoryManager.Users.UpdateUser(currentUser);
		await repositoryManager.SaveChangesAsync();

		// If the username was changed, log out and redirect to login
		if (!string.IsNullOrWhiteSpace(Username) && usernameChanged)
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToPage("/Login");
		}

		return Page();
	}
}

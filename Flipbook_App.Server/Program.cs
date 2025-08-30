using Flipbook_App.Controllers;
using Flipbook_App.Data;
using Flipbook_App.Repositories.Interfaces;
using Flipbook_App.Repositories;
using Flipbook_App.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Service Registration ---
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// --- Middleware Pipeline ---
ConfigureMiddleware(app);

app.Run();

// Service Registration
static void ConfigureServices(IServiceCollection services, IConfiguration config)
{
	// Razor Pages & Controllers
	services.AddRazorPages();
	services.AddControllers();

	// Authentication
	services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
		.AddCookie(options =>
		{
			options.LoginPath = "/Login";
			options.LogoutPath = "/Logout";
			options.ExpireTimeSpan = TimeSpan.FromHours(1);
		});

	// Database Context
	services.AddDbContext<FlipbookDBContext>(options =>
		options.UseSqlServer(config.GetConnectionString("Flipbook_DB")));

	// Repositories
	services.AddScoped<IUserRepository, UserRepository>();
	services.AddScoped<IAnimationRepository, AnimationRepository>();
	// Unit of work
	services.AddScoped<RepositoryManager>();

	services.AddSingleton<IBlobStorageService>(sp =>
	{
		var connectionString = config.GetValue<string>("AzureStorage:ConnectionString");
		var containerName = config.GetValue<string>("AzureStorage:ContainerName");
		return new BlobStorageService(connectionString, containerName);
	});

	services.AddScoped<CanvasController>();


}

// Middleware Pipeline
static void ConfigureMiddleware(WebApplication app)
{
	if (!app.Environment.IsDevelopment())
	{
		app.UseExceptionHandler("/Error");
		app.UseHsts();
	}

	app.UseHttpsRedirection();
	app.UseStaticFiles();
	app.UseBlazorFrameworkFiles("/canvas");

	app.UseRouting();

	app.UseAuthentication();
	app.UseAuthorization();

	app.MapControllers();
	app.MapRazorPages();
	app.MapFallbackToFile("/canvas/{*path}", "canvas/index.html");
}

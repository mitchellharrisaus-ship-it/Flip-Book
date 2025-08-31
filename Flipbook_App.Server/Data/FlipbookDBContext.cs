using FlipBook_App.Shared.Core;
using FlipBook_Library.Core;
using Microsoft.EntityFrameworkCore;

namespace Flipbook_App.Data;

public class FlipbookDBContext : DbContext
{
	public FlipbookDBContext(DbContextOptions<FlipbookDBContext> options) : base(options) { }

	public DbSet<User> Users { get; set; }

	public DbSet<AnimationReference> Animations { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<User>()
			.HasIndex(u => u.Username)
			.IsUnique();

		modelBuilder.Entity<AnimationReference>()
			.HasIndex(a => new { a.UserID, a.Title })
			.IsUnique();
	}
}
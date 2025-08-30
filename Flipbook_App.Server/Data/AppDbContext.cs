using FlipBook_Library.Core;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FlipbookApp.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	public DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<User>()
			.HasIndex(u => u.Username)
			.IsUnique();
	}
}
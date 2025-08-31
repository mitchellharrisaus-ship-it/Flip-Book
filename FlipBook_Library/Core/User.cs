using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlipBook_Library.Core;

public class User
{
	[Key]
	public Guid Id { get; set; }

	[Required]
	public string Username { get; set; } = string.Empty;

	[Required]
	public string PasswordHash { get; set; } = string.Empty;

	[InverseProperty("User")]
	public ICollection<AnimationReference> Animations { get; set; } = new List<AnimationReference>();
}
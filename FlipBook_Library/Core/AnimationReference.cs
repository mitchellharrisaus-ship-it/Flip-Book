using FlipBook_Library.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlipBook_App.Shared.Core;
public class AnimationReference
{
	[Key]
	public Guid AnimationID { get; set; }

	public string Title { get; set; } = string.Empty;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


	[ForeignKey(nameof(UserID))]
	[InverseProperty("Animations")]
	public User User { get; set; }

	public Guid UserID { get; set; }
}

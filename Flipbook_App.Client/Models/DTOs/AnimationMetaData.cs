namespace Flipbook_App.Client.Models.DTOs;

public class AnimationMetaData
{
	public Guid UserID { get; set; }

	public string Title { get; set; } = string.Empty;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

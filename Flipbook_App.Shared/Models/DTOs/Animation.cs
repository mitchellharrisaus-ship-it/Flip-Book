namespace Flipbook_App.Shared.Models.DTOs;

public class Animation
{
    public required Guid AnimationID { get; set; }
    public required AnimationMetaData MetaData { get; set; }
    public required IList<Frame> Frames { get; set; }
}
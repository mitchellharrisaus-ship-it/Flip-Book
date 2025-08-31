namespace Flipbook_App.Shared.Models.DTOs;

public class PhysicsSettings
{
    public float TimeToMap { get; set; }
    public int NumberOfFrames { get; set; }
    public bool HasBoarder { get; set; }
    public float Gravity { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}
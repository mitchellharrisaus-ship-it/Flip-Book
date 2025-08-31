using Flipbook_App.Shared.Models.Core;
using Flipbook_App.Shared.Models.Enums;

namespace Flipbook_App.Shared.Models.DTOs;

public class DrawActionDTO
{
    public IList<Vertex> Vertices { get; set; } = [];
    public BrushType Brush { get; set; }
    public required Colour BrushColour { get; set; }
    public int BrushSize { get; set; }
    public int ActionFrame { get; set; }
    public bool IsPhysicsObject { get; set; } = false;
    public PhysicsObjectSettings? PhysicsSettings { get; set; }
}
using Moq;
using Flipbook_App.Client.Services;
using FlipBook_Library.Core;
using FlipBook_Library.Enums;
using FlipBook_Library.Services;
using Microsoft.AspNetCore.Components;
using FlipBook_Library.DTOs;

namespace Flipbook_App.Tests;

public class DrawTests
{
    private SkiaDrawingService _drawingService;
    private Mock<IAnimationApiService> _apiServiceMock;
    private TestNavigationManager _navigation;
    private Mock<ToastService> _toastServiceMock;
    private DrawRazorTestHost _host;

    [SetUp]
    public void Setup()
    {
        _drawingService = new SkiaDrawingService(new DrawShapeService());
        _apiServiceMock = new Mock<IAnimationApiService>();
        _navigation = new TestNavigationManager();
        _toastServiceMock = new Mock<ToastService>();
        _host = new DrawRazorTestHost(
            _drawingService,
            _apiServiceMock.Object,
            _navigation,
            _toastServiceMock.Object
        );
    }

    [Test]
    public void InitializesWithDefaultValues()
    {
        Assert.That(_host.AnimationTitle, Is.EqualTo("My Animation"));
        Assert.That(_host.CurrentDrawingMode, Is.EqualTo(DrawingMode.Select));
        Assert.That(_host.ActiveFrameIndex, Is.EqualTo(0));
    }

    [Test]
    public void CreateNewFrame_AddsFrame()
    {
        int initialCount = _drawingService.Frames.Count;
        _host.CreateNewFrame();
        Assert.That(_drawingService.Frames.Count, Is.EqualTo(initialCount + 1));
    }

    [Test]
    public void SelectFrame_ValidIndex_UpdatesActiveFrame()
    {
        _host.CreateNewFrame(); // Ensure at least 2 frames
        _host.SelectFrame(1);
        Assert.That(_host.ActiveFrameIndex, Is.EqualTo(1));
        Assert.That(_drawingService.CurrentFrameIndex, Is.EqualTo(1));
    }

    [Test]
    public void DeleteFrame_UpdatesActiveFrameIndex()
    {
        _host.CreateNewFrame(); // Ensure at least 2 frames
        _host.SelectFrame(1);
        _host.DeleteFrame(1);
        Assert.That(_host.ActiveFrameIndex, Is.EqualTo(_drawingService.CurrentFrameIndex));
        Assert.That(_drawingService.Frames.Count, Is.EqualTo(1));
    }

    [Test]
    public void SetDrawingMode_UpdatesModeAndService()
    {
        _host.SetDrawingMode(DrawingMode.Pen);
        Assert.That(_host.CurrentDrawingMode, Is.EqualTo(DrawingMode.Pen));
        Assert.That(_drawingService.IsDrawingEnabled, Is.True);
    }

    [Test]
    public void SelectTool_SetsActiveBrush()
    {
        _host.SelectTool(BrushType.Marker);
        Assert.That(_drawingService.ActiveBrush, Is.EqualTo(BrushType.Marker));
    }

    [Test]
    public void ClearCanvas_ClearsDrawingService()
    {
        // Add a shape to the current frame
        _drawingService.CurrentFrame.Actions.Push(new DrawActionDTO { BrushColour = new Colour() });
        _host.ClearCanvas();
        Assert.That(_drawingService.CurrentFrame.Actions.Count, Is.EqualTo(0));
    }

    [Test]
    public void Undo_CallsDrawingServiceUndo()
    {
        // Simulate a drawing action to initialize undo stack
        _drawingService.HandlePointerDown(0, 0);
        _drawingService.HandlePointerMove(1, 1);
        _drawingService.HandlePointerUp();
        int before = _drawingService.CurrentFrame.Actions.Count;
        _host.Undo();
        Assert.That(_drawingService.CurrentFrame.Actions.Count, Is.EqualTo(before - 1));
    }

    [Test]
    public void Redo_CallsDrawingServiceRedo()
    {
        // Simulate a drawing action to initialize undo stack
        _drawingService.HandlePointerDown(0, 0);
        _drawingService.HandlePointerMove(1, 1);
        _drawingService.HandlePointerUp();
        _host.Undo();
        _host.Redo();
        Assert.That(_drawingService.CurrentFrame.Actions.Count, Is.EqualTo(1));
    }

    [Test]
    public void SetColor_UpdatesSelectedColorAndService()
    {
        _host.SetColor("#FF0000");
        Assert.That(_host.SelectedColor, Is.EqualTo("#FF0000"));
        Assert.That(_drawingService.BrushColour.Red, Is.EqualTo(255));
        Assert.That(_drawingService.BrushColour.Green, Is.EqualTo(0));
        Assert.That(_drawingService.BrushColour.Blue, Is.EqualTo(0));
    }

    [Test]
    public void OnShapeSelectionChanged_UpdatesSelectedShape()
    {
        var e = new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = BrushType.Circle.ToString() };
        _host.CurrentDrawingMode = DrawingMode.Shape;
        _host.OnShapeSelectionChanged(e);
        Assert.That(_host.SelectedShape, Is.EqualTo(BrushType.Circle));
        Assert.That(_drawingService.ActiveBrush, Is.EqualTo(BrushType.Circle));
    }

    [Test]
    public void OnPhysicsEnabledChanged_EnablesPhysics()
    {
        var e = new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = true };
        _host.OnPhysicsEnabledChanged(e);
        Assert.That(_host.IsPhysicsEnabled, Is.True);
        Assert.That(_drawingService.IsPhysicsEnabled, Is.True);
    }

    [Test]
    public void OnPhysicsAppliesOnShapesChanged_UpdatesService()
    {
        var e = new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = true };
        _host.OnPhysicsAppliesOnShapesChanged(e);
        Assert.That(_host.PhysicsAppliesOnShapes, Is.True);
        Assert.That(_drawingService.PhysicsAppliesOnShapes, Is.True);
    }

    [Test]
    public void OpenExportModal_SetsModalState()
    {
        _host.OpenExportModal();
        Assert.That(_host.IsExportModalOpen, Is.True);
    }

    [Test]
    public void GoToProjects_NavigatesToProjects()
    {
        _host.GoToProjects();
        Assert.That(_navigation.LastUri, Is.EqualTo("/projects"));
    }
}

public class DrawRazorTestHost
{
    public string AnimationTitle { get; set; } = "My Animation";
    public DrawingMode CurrentDrawingMode { get; set; } = DrawingMode.Select;
    public int ActiveFrameIndex { get; set; } = 0;
    public BrushType SelectedShape { get; set; } = BrushType.Pen;
    public bool IsPhysicsEnabled { get; set; } = false;
    public bool PhysicsAppliesOnShapes { get; set; } = false;
    public bool IsExportModalOpen { get; set; } = false;
    public SkiaDrawingService DrawingService { get; }
    public IAnimationApiService ApiService { get; }
    public NavigationManager Navigation { get; }
    public ToastService ToastService { get; }

    public DrawRazorTestHost(SkiaDrawingService drawingService, IAnimationApiService apiService, NavigationManager navigation, ToastService toastService)
    {
        DrawingService = drawingService;
        ApiService = apiService;
        Navigation = navigation;
        ToastService = toastService;
    }

    public void CreateNewFrame() => DrawingService.CreateFrame();
    public void SelectFrame(int index)
    {
        if (index >= 0 && index < DrawingService.Frames.Count)
        {
            ActiveFrameIndex = index;
            DrawingService.CurrentFrameIndex = index;
        }
    }
    public void DeleteFrame(int index)
    {
        DrawingService.DeleteFrame(index);
        ActiveFrameIndex = DrawingService.CurrentFrameIndex;
    }
    public void SetDrawingMode(DrawingMode mode)
    {
        CurrentDrawingMode = mode;
        switch (mode)
        {
            case DrawingMode.Select:
                DrawingService.IsDrawingEnabled = false;
                break;
            case DrawingMode.Pen:
                DrawingService.IsDrawingEnabled = true;
                break;
            case DrawingMode.Shape:
                DrawingService.IsDrawingEnabled = true;
                DrawingService.ActiveBrush = SelectedShape;
                break;
        }
    }
    public void SelectTool(BrushType tool)
    {
        DrawingService.ActiveBrush = tool;
    }
    public void ClearCanvas() => DrawingService.Clear();
    public void Undo() => DrawingService.Undo();
    public void Redo() => DrawingService.Redo();
    public string SelectedColor { get; set; } = "#000000";
    public void SetColor(string color)
    {
        SelectedColor = color;
        DrawingService.SetBrushColor(color);
    }
    public void OnShapeSelectionChanged(ChangeEventArgs e)
    {
        if (Enum.TryParse<BrushType>(e.Value?.ToString(), out var brushType))
        {
            SelectedShape = brushType;
            if (CurrentDrawingMode == DrawingMode.Shape)
            {
                DrawingService.ActiveBrush = SelectedShape;
            }
        }
    }
    public void OnPhysicsEnabledChanged(ChangeEventArgs e)
    {
        IsPhysicsEnabled = (bool)(e.Value ?? false);
        DrawingService.IsPhysicsEnabled = IsPhysicsEnabled;
    }
    public void OnPhysicsAppliesOnShapesChanged(ChangeEventArgs e)
    {
        PhysicsAppliesOnShapes = (bool)(e.Value ?? false);
        DrawingService.PhysicsAppliesOnShapes = PhysicsAppliesOnShapes;
    }
    public void OpenExportModal() => IsExportModalOpen = true;
    public void GoToProjects() => Navigation.NavigateTo("/projects");
}

using SkiaSharp;

namespace Flipbook_App.Models.Drawing;

public class SkiaDrawingService
{
    private readonly SKBitmap _bitmap;
    private readonly SKCanvas _canvas;
    private bool _isDrawing;
    private SKPoint? _previousPoint;
    private readonly List<List<SKPoint>> _shapes;
    private List<SKPoint> _currentShape;

    public SkiaDrawingService(int width = 800, int height = 600)
    {
        _bitmap = new SKBitmap(width, height);
        _canvas = new SKCanvas(_bitmap);
        
        // Initialize with white background
        _canvas.Clear(SKColors.White);
        
        _shapes = new List<List<SKPoint>>();
        _currentShape = new List<SKPoint>();
        _isDrawing = false;
    }

    public void HandlePointerDown(float x, float y)
    {
        _isDrawing = true;
        _currentShape = new List<SKPoint>();
        _previousPoint = new SKPoint(x, y);
    }

    public void HandlePointerMove(float x, float y)
    {
        if (!_isDrawing || !_previousPoint.HasValue) return;

        var currentPoint = new SKPoint(x, y);
        
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 3,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true
        };

        // Draw line from previous to current point
        _canvas.DrawLine(_previousPoint.Value, currentPoint, paint);
        
        // Add point to current shape
        _currentShape.Add(currentPoint);
        
        _previousPoint = currentPoint;
    }

    public void HandlePointerUp()
    {
        if (!_isDrawing) return;
        
        _isDrawing = false;
        if (_currentShape.Count > 0)
        {
            _shapes.Add(_currentShape);
        }
        _previousPoint = null;
    }

    public void Clear()
    {
        _canvas.Clear(SKColors.White);
        _shapes.Clear();
        _currentShape.Clear();
        _isDrawing = false;
        _previousPoint = null;
    }

    public byte[] GetImageBytes()
    {
        using var image = SKImage.FromBitmap(_bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public void Dispose()
    {
        _canvas.Dispose();
        _bitmap.Dispose();
    }
}
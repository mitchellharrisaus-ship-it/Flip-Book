using FlipBook_Library.Core;
using FlipBook_Library.DTOs;
using FlipBook_Library.Enums;
using SkiaSharp;

namespace FlipBook_Library.Services; 

public interface IDrawShapeService
{
	/// <summary>
	/// Draws a shape on the canvas based on the DrawActionDTO
	/// </summary>
	/// <param name="canvas">The SkiaSharp canvas to draw on</param>
	/// <param name="shape">The shape data containing vertices, brush type, color, etc.</param>
	void DrawShape(SKCanvas canvas, DrawActionDTO shape);

	/// <summary>
	/// Draws a specific shape type with given parameters
	/// </summary>
	/// <param name="canvas">The SkiaSharp canvas to draw on</param>
	/// <param name="shapeType">The type of shape to draw</param>
	/// <param name="vertices">The vertices defining the shape</param>
	/// <param name="radius">The radius for circular shapes</param>
	/// <param name="color">The color to draw with</param>
	/// <param name="strokeWidth">The stroke width</param>
	/// <param name="isPhysicsObject">Whether this shape represents a physics object</param>
	void DrawShape(SKCanvas canvas, PhysicsShape shapeType, IList<Vertex> vertices, int radius, Colour color, int strokeWidth, bool isPhysicsObject = false);

	/// <summary>
	/// Creates the appropriate paint object for a shape
	/// </summary>
	/// <param name="color">The color for the paint</param>
	/// <param name="strokeWidth">The stroke width</param>
	/// <returns>A configured SKPaint object</returns>
	SKPaint CreateShapePaint(Colour color, int strokeWidth);
}
using FlipBook_Library.Core;
using FlipBook_Library.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlipBook_App.Shared.Core;
public class PhysicsShapeInstance
{
	public int ObjectId { get; set; }
	public PhysicsShape Shape { get; set; }
	public float Radius { get; set; }
	public Vertex CenterVertice { get; set; }

	public PhysicsShapeInstance(int objectId, PhysicsShape shape, float radius, Vertex centerVertice)
	{
		ObjectId = objectId;
		Shape = shape;
		Radius = radius;
		CenterVertice = centerVertice;
	}
}

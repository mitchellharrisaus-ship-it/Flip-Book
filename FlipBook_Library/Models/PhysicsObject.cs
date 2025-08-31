using FlipBook_Library.Core;
using FlipBook_Library.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FlipBook_Library.Models;
public class PhysicsObject
{
	public PhysicsObject(int id, PhysicsObjectSettings settings, Vertex initialCentreOfObject, float radius)
	{
		Id = id;
		Settings = settings;
		InitialCentreOfObject = initialCentreOfObject;
		Radius = radius;
	}
	public int Id { get; set; } // Equal to Index of DrawAction in Frame
	public PhysicsObjectSettings Settings { get; set; }
	
	public Vertex InitialCentreOfObject { get; set; }
	public float Radius { get; set; }
}

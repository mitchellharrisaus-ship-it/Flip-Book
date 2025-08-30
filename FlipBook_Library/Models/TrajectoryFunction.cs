using FlipBook_Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlipBook_Library.Models;

    public class TrajectoryFunction
    {
		public int ObjectId { get; set; }
		public PhysicsObject OriginalAction { get; set; }
		public Func<float, Vertex> PositionFunction { get; set; }
		//public Func<float, Vertex> VelocityFunction { get; set; }
		public float StartTime { get; set; }
		public float EndTime { get; set; }

		public Vertex GetPositionAtTime(float time)
		{
			if (time < StartTime || time > EndTime)
				return PositionFunction(EndTime);

			return PositionFunction(time);
		}
}
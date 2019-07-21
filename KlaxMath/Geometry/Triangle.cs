using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KlaxMath.Geometry
{
	public struct STriangle
	{
		public STriangle(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			Point1 = v1;
			Point2 = v2;
			Point3 = v3;
		}

		public Vector3 Normal
		{
			get
			{
				Vector3 a = Point2 - Point1;
				Vector3 b = Point3 - Point2;

				return Vector3.Cross(a, b);
			}
		}

		public Vector3 WeightedCenter
		{
			get { return (Point1 + Point2 + Point3) / 3; }
		}

		public Vector3 Point1 { get; set; }
		public Vector3 Point2 { get; set; }
		public Vector3 Point3 { get; set; }
	}
}

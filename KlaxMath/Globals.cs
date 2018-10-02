using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxMath
{
    // Coordinate System Definition
    // Default Coordinate System is LH with Z Forward, Y Up and X Right
    public static class Axis
    {
        public static readonly SharpDX.Vector3 Forward = new SharpDX.Vector3(0, 0, 1);
        public static readonly SharpDX.Vector3 Up = new SharpDX.Vector3(0, 1, 0);
        public static readonly SharpDX.Vector3 Right = new SharpDX.Vector3(1, 0, 0);
    }
}

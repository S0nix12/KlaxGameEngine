using KlaxMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxRenderer.Camera
{
    public abstract class ICamera
    {
        public abstract void UpdateViewParams();


        /****************** Properties *******************/
        public Transform Transform
        { get; protected set; } = new Transform();

        public SharpDX.Matrix ProjectionMatrix
        { get; protected set; }

        public SharpDX.Matrix ViewMatrix
        { get; protected set; }
    }
}

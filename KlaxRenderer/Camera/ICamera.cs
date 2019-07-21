using KlaxMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KlaxRenderer.Camera
{
    public abstract class ICamera
    {
        public abstract void UpdateViewParams();
	    public abstract Vector3 WorldToScreenPoint(Vector3 worldPosition);
	    public abstract Vector3 ScreenToWorldPointAbs(Vector3 screenPosition);
	    public abstract Vector3 ScreenToWorldPointRel(Vector3 screenPosition);

	    public abstract float GetScreenWidth();
	    public abstract float GetScreenHeight();


        /****************** Properties *******************/
        public Transform Transform
        { get; protected set; } = new Transform();

        public SharpDX.Matrix ProjectionMatrix
        { get; protected set; }

        public SharpDX.Matrix ViewMatrix
        { get; protected set; }
    }
}

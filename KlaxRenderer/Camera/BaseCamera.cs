using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using KlaxMath;

namespace KlaxRenderer.Camera
{
    public class CBaseCamera : ICamera
    {
        public override void UpdateViewParams()
        {
            // Set Projection and ViewMatrix here

            // Create our ViewMatrix from our Transform
            // TODO think about a method to use dirty flags so we only recreate this when the transform changes
            //ViewMatrix = Matrix.LookAtLH(Transform.Position, Transform.Position + Transform.Forward, Transform.Up);
	        ViewMatrix = Matrix.Invert(Transform.WorldMatrix);

			// Set Projection Matrix
			if (IsPerspective)
            {
                ProjectionMatrix = Matrix.PerspectiveFovLH(FieldOfView, AspectRatio, ScreenNear, ScreenFar);
			}
            else
            {
                ProjectionMatrix = Matrix.OrthoLH(ScreenWidth, ScreenHeight, ScreenNear, ScreenFar);
            }
        }

	    public override Vector3 WorldToScreenPoint(Vector3 worldPosition)
	    {
		    return Vector3.Project(worldPosition, 0.0f, 0.0f, ScreenWidth, ScreenHeight, ScreenNear, ScreenFar, ViewMatrix * ProjectionMatrix);			
	    }

	    public override Vector3 ScreenToWorldPointAbs(Vector3 screenPosition)
	    {
		    return Vector3.Unproject(screenPosition, 0.0f, 0.0f, ScreenWidth, ScreenHeight, ScreenNear, ScreenFar, ViewMatrix * ProjectionMatrix);
	    }

	    public override Vector3 ScreenToWorldPointRel(Vector3 screenPosition)
	    {
		    screenPosition.X *= ScreenWidth;
		    screenPosition.Y *= ScreenHeight;
		    return Vector3.Unproject(screenPosition, 0.0f, 0.0f, ScreenWidth, ScreenHeight, ScreenNear, ScreenFar, ViewMatrix * ProjectionMatrix);
	    }

		public override float GetScreenWidth()
		{
			return ScreenWidth;
		}

		public override float GetScreenHeight()
		{
			return ScreenHeight;
		}

		/****************** Properties *******************/
		public float FieldOfView
        { get; set; } = MathUtil.Pi / 4.0f;

        public float AspectRatio
        {
	        get { return ScreenWidth / ScreenHeight; }
        }

        public float ScreenNear
        { get; set; } = 0.2f;

        public float ScreenFar
        { get; set; } = 10000.0f;

        public float ScreenWidth
        { get; set; } = 1280.0f;

        public float ScreenHeight
        { get; set; } = 720.0f;

        public bool IsPerspective
        { get; set; } = true;        
    }
}

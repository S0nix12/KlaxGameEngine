using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using KlaxMath;

namespace KlaxRenderer.Camera
{
    class CBaseCamera : ICamera
    {
        public override void UpdateViewParams()
        {
            // Set Projection and ViewMatrix here

            // Create our ViewMatrix from our Transform
            // TODO think about a method to use dirty flags so we only recreate this when the transform changes
            ViewMatrix = Matrix.LookAtLH(Transform.Position, Transform.Forward, Transform.Up);

            // Set Projection Matrix
            if(IsPerspective)
            {
                ProjectionMatrix = Matrix.PerspectiveFovLH(FieldOfView, AspectRatio, ScreenNear, ScreenFar);
            }
            else
            {
                ProjectionMatrix = Matrix.OrthoLH(ScreenWidth, ScreenHeight, ScreenNear, ScreenFar);
            }
        }

        /****************** Properties *******************/
        public float FieldOfView
        { get; set; } = SharpDX.MathUtil.Pi / 4.0f;

        public float AspectRatio
        { get; set; } = 16.0f / 9.0f;

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

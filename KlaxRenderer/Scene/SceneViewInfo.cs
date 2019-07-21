using System;
using KlaxMath;
using SharpDX;

namespace KlaxRenderer.Scene
{
	public struct SSceneViewInfo
	{
		public Matrix ViewMatrix;
		public Matrix ProjectionMatrix;
		public Vector3 ViewLocation;

		public float ScreenTop;
		public float ScreenLeft;
		public float ScreenWidth;
		public float ScreenHeight;

		public float ScreenNear;
		public float ScreenFar;

		public float Fov;
		public bool FitProjectionToScene;

		public BoundingFrustum CameraFrustum { get; private set; }

		public void CreateBoundingFrustum()
		{
			Matrix invView = Matrix.Invert(ViewMatrix);
			CameraFrustum = BoundingFrustum.FromCamera(ViewLocation, -invView.Forward, invView.Up, Fov, ScreenNear, ScreenFar, ScreenWidth / ScreenHeight);
		}

		public Vector3 WorldToScreenPoint(in Vector3 worldPosition)
		{
			return Vector3.Project(worldPosition, 0.0f, 0.0f, ScreenWidth, ScreenHeight, ScreenNear, ScreenFar, ViewMatrix * ProjectionMatrix);
		}

		public System.Numerics.Vector2 WorldToScreenNumeric(in Vector3 worldPosition)
		{
			Vector3 screenPoint = WorldToScreenPoint(in worldPosition);
			return new System.Numerics.Vector2(screenPoint.X, screenPoint.Y);
		}

		public Vector3 ScreenToWorldPoint(in Vector3 screenPosition)
		{
			return Vector3.Unproject(screenPosition, 0.0f, 0.0f, ScreenWidth, ScreenHeight, ScreenNear, ScreenFar, ViewMatrix * ProjectionMatrix);
		}

		public float GetScreenScaleFactor(in Vector3 worldPosition)
		{
			Matrix invView = Matrix.Invert(ViewMatrix);
			Vector3 toWorld = worldPosition - ViewLocation;

			// Get the distance of the point from the camera along its forward axis
			float screenFactor = Vector3.Dot(toWorld, -invView.Forward);
			screenFactor = Math.Max(screenFactor, ScreenNear);

			// Compensate the fov factor, note this break with an extreme fov
			return screenFactor * MathUtilities.Tanf(Fov / 2.0f);
		}
	}
}
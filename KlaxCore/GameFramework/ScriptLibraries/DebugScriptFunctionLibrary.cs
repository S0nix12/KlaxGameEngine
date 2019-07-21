using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxRenderer;
using KlaxRenderer.Debug;
using KlaxShared.Attributes;
using SharpDX;

namespace KlaxCore.GameFramework.ScriptLibraries
{
	[KlaxLibrary]
	public static class CDebugScriptFunctionLibrary
	{
		[KlaxFunction(Category = "Debug")]
		public static void DebugDrawBox(Vector3 position, Quaternion rotation, Vector3 extent, Color4 color, float displayTime, bool useWireframe, bool disableDepthTest)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawBox(in position, in rotation, in extent, in color, displayTime, GetDebugDrawFlags(useWireframe, disableDepthTest));
		}

		[KlaxFunction(Category = "Debug")]
		public static void DebugDrawSphere(Vector3 position, float radius, Color4 color, float displayTime, bool useWireframe, bool disableDepthTest)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawSphere(in position, radius, in color, displayTime, GetDebugDrawFlags(useWireframe, disableDepthTest));
		}

		[KlaxFunction(Category = "Debug")]
		public static void DebugDrawHemisphere(Vector3 baseCenter, Vector3 up, float radius, Color4 color, float displayTime, bool useWireframe, bool disableDepthTest)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawHemisphere(in baseCenter, in up, radius, in color, displayTime, GetDebugDrawFlags(useWireframe, disableDepthTest));
		}

		[KlaxFunction(Category = "Debug")]
		public static void DrawCylinder(Vector3 center, float height, float radius, Quaternion rotation, Color4 color, float displayTime, bool useWireframe, bool disableDepthTest)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawCylinder(in center, height, radius, in rotation, in color, displayTime, GetDebugDrawFlags(useWireframe, disableDepthTest));
		}

		[KlaxFunction(Category = "Debug")]
		public static void DrawCone(Vector3 baseCenter, float height, float radius, Quaternion rotation, Color4 color, float displayTime, bool useWireframe, bool disableDepthTest)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawCone(in baseCenter, height, radius, in rotation, in color, displayTime, GetDebugDrawFlags(useWireframe, disableDepthTest));
		}

		[KlaxFunction(Category = "Debug")]
		public static void DrawPyramid(Vector3 baseCenter, float baseLength, float baseWidth, float height, Quaternion rotation, Color4 color, float displayTime, bool useWireframe, bool disableDepthTest)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawPyramid(in baseCenter, baseLength, baseWidth, height, in rotation, in color, displayTime, GetDebugDrawFlags(useWireframe, disableDepthTest));
		}

		[KlaxFunction(Category = "Debug")]
		public static void DrawLine(Vector3 startPos, Color4 startColor, Vector3 endPos,  Color4 endColor, float displayTime, bool useWireframe, bool disableDepthTest)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawLine(in startPos, in startColor, in endPos, in endColor, displayTime, GetDebugDrawFlags(useWireframe, disableDepthTest));
		}

		[KlaxFunction(Category = "Debug")]
		public static void DrawPoint(Vector3 location, float size, Color4 color, float displayTime, bool useWireframe, bool disableDepthTest)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawPoint(in location, size, in color, displayTime, GetDebugDrawFlags(useWireframe, disableDepthTest));
		}

		[KlaxFunction(Category = "Debug")]
		public static void DrawArrow(Vector3 startPos, Vector3 direction, float length, Color4 color, float displayTime, bool useWireframe, bool disableDepthTest)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawArrow(in startPos, in direction, length, in color, displayTime, GetDebugDrawFlags(useWireframe, disableDepthTest));
		}

		[KlaxFunction(Category = "Debug")]
		public static void DrawCircle(Vector3 center, Vector3 axis, float radius, Color4 color, float displayTime, bool useWireframe, bool disableDepthTest)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawCircle(in center, in axis, radius, in color, displayTime, 32, GetDebugDrawFlags(useWireframe, disableDepthTest));
		}

		[KlaxFunction(Category = "Debug")]
		public static void DrawCircleSegment(Vector3 center, Vector3 axis, Vector3 up, float radius, float fraction, Color4 color, float displayTime, bool useWireframe, bool disableDepthTest)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawCircleSegment(in center, in axis, in up, radius, fraction, in color, displayTime, 16, GetDebugDrawFlags(useWireframe, disableDepthTest));
		}

		[KlaxFunction(Category = "Debug")]
		public static void DrawTextScreenRel(Vector2 screenPos, string text, float size, Color4 color, float displayTime)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawTextScreenRel(in screenPos, text, size, in color, displayTime);
		}

		[KlaxFunction(Category = "Debug")]
		public static void DrawTextScreenAbs(Vector2 screenPos, string text, float size, Color4 color, float displayTime)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawTextScreenAbs(in screenPos, text, size, in color, displayTime);
		}

		[KlaxFunction(Category = "Debug")]
		public static void DrawTextWorld(Vector3 worldPos, string text, float size, Color4 color, float displayTime)
		{
			CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
			debugRenderer.DrawTextWorld(in worldPos, text, size, in color, displayTime);
		}

		[KlaxFunction(Category = "Debug")]
		public static void Log(string text)
		{
			LogUtility.Log(text);
		}

		private static EDebugDrawCommandFlags GetDebugDrawFlags(bool useWireframe, bool disableDepthTest)
		{
			EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None;
			if (useWireframe) flags |= EDebugDrawCommandFlags.Wireframe;
			if (disableDepthTest) flags |= EDebugDrawCommandFlags.NoDepthTest;
			return flags;
		}
	}
}

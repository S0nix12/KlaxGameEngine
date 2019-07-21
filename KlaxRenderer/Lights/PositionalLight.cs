using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KlaxMath;
using SharpDX;

namespace KlaxRenderer.Lights
{
	public enum ELightType
	{
		Directional = 0,
		Point       = 1,
		Spot        = 2,
		Ambient     = 3
	}

	/// <summary>
	/// A positional light has a defined position in the world
	/// Unlike non positional lights (Directional, Ambient) it does not always affect the whole scene
	/// All positional lights are saved in the same cbuffer in the shader of the object they affect, only a maximum of 6 lights can affect a single mesh
	/// </summary>
	public abstract class CPositionalLight : ILight
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct SPositionalLightShaderData
		{
			public Vector4 Position; // offset 0
			public Vector4 Direction; // offset 16
			public Vector4 Color; // offset 32
			public float Range; // offset 48
			public float SpotAngle; // offset 52
			public float ConstantAttenuation; // offset 56
			public float LinearAttenuation; // offset 60
			public float QuadraticAttenuation; // offset 64
			public int LightType; // offset 68
			public int bEnabled; // offset 72
			private float _padding; //offset 76 // size 80
		}

		public const int MaxNumPositionalLights = 6;

		public abstract void FillShaderData(ref SPositionalLightShaderData data);
		public abstract ELightType GetLightType();
		public Transform Transform { get; protected set; } = new Transform();
	}
}

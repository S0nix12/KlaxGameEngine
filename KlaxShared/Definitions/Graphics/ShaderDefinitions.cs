using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxShared.Serialization.Converters;
using Newtonsoft.Json;
using SharpDX;

namespace KlaxShared.Definitions.Graphics
{
	public enum EShaderParameterType
	{
		Scalar,
		Vector,
		Color,
		Matrix,
		Texture
	}

	public enum EShaderTargetStage
	{
		Vertex,
		Pixel,
		Geometry,
		Compute,
		Domain,
		Hull
	}

	[JsonConverter(typeof(CShaderParameterConverter))]
	public struct SShaderParameter
	{
		public SShaderParameter(EShaderParameterType type, object data)
		{
			parameterType = type;
			parameterData = data;
		}

		public EShaderParameterType parameterType;
		public object parameterData;
	}

	public static class ShaderHelpers
	{
		public static int GetSizeFromParameterType(EShaderParameterType type)
		{
			switch (type)
			{
				case EShaderParameterType.Scalar:
					return 4;
				case EShaderParameterType.Vector:
					return Vector3.SizeInBytes;
				case EShaderParameterType.Color:
					return SharpDX.Utilities.SizeOf<Color4>();
				case EShaderParameterType.Matrix:
					return Matrix.SizeInBytes;
				case EShaderParameterType.Texture:
					return -1;
			}

			return -1;
		}

		public static Type GetTypeFromParameterType(EShaderParameterType type)
		{
			switch (type)
			{
				case EShaderParameterType.Scalar:
					return typeof(float);
				case EShaderParameterType.Vector:
					return typeof(Vector3);
				case EShaderParameterType.Color:
					return typeof(Color4);
				case EShaderParameterType.Matrix:
					return typeof(Matrix);
			}

			return null;
		}
	}
}

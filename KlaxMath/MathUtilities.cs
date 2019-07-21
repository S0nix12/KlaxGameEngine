using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KlaxMath
{
	public static class MathUtilities
	{
		/// <summary>
		/// Creates a rotation matrix that tranforms the world Forward Axis to the given Forward Vector
		/// Up must NOT be almost equal to forward
		/// </summary>
		/// <param name="forward"></param>
		/// <param name="up"></param>
		/// <param name="result"></param>
		public static Matrix3x3 CreateLookAtRotationMatrix(ref Vector3 forward, ref Vector3 up)
		{
			Matrix3x3 result = Matrix3x3.Identity;
			CreateLookAtRotation(ref forward, ref up, ref result);
			return result;
		}

		/// <summary>
		/// Creates a rotation matrix that tranforms the world Forward Axis to the given Forward Vector
		/// Up must NOT be almost equal to forward
		/// </summary>
		/// <param name="forward"></param>
		/// <param name="up"></param>
		/// <param name="result"></param>
		public static void CreateLookAtRotation(ref Vector3 forward, ref Vector3 up, ref Matrix3x3 result)
		{
			Vector3 xaxis, yaxis;			
			Vector3.Cross(ref up, ref forward, out xaxis); xaxis.Normalize();
			Vector3.Cross(ref forward, ref xaxis, out yaxis);

			result = Matrix3x3.Identity;
			result.M11 = xaxis.X; result.M21 = yaxis.X; result.M31 = forward.X;
			result.M12 = xaxis.Y; result.M22 = yaxis.Y; result.M32 = forward.Y;
			result.M13 = xaxis.Z; result.M23 = yaxis.Z; result.M33 = forward.Z;
		}

		public static Quaternion CreateLookAtQuaternion(Vector3 forward, Vector3 up)
		{
			Quaternion result = Quaternion.Identity;
			CreateLookAtRotation(ref forward, ref up, ref result);
			return result;
		}

		public static void CreateLookAtRotation(ref Vector3 forward, ref Vector3 up, ref Quaternion result)
		{
			Matrix3x3 rotationMatrix = Matrix3x3.Identity;
			CreateLookAtRotation(ref forward, ref up, ref rotationMatrix);
			Quaternion.RotationMatrix(ref rotationMatrix, out result);
		}

		public static Matrix CreateLocalTransformationMatrix(Vector3 scaling, Quaternion rotation, Vector3 translation)
		{
			Matrix result = new Matrix();
			CreateLocalTransformationMatrix(ref scaling, ref rotation, ref translation, ref result);
			return result;
		}

		public static void CreateLocalTransformationMatrix(ref Vector3 scaling, ref Quaternion rotation, ref Vector3 translation, ref Matrix result)
		{
			result = Matrix.Scaling(scaling) * Matrix.RotationQuaternion(rotation) * Matrix.Translation(translation);
		}

		public static int Clamp(int value, int min, int max)
		{
			return value < min ? min : value > max ? max : value;
		}
		public static float Clamp(float value, float min, float max)
		{
			return value < min ? min : value > max ? max : value;
		}
		public static double Clamp(double value, double min, double max)
		{
			return value < min ? min : value > max ? max : value;
		}

		public static float Cosf(float x)
		{
			return (float) Math.Cos(x);
		}
		public static float Acosf(float x)
		{
			return (float)Math.Acos(x);
		}

		public static float Sinf(float x)
		{
			return (float) Math.Sin(x);
		}
		public static float Asinf(float x)
		{
			return (float)Math.Asin(x);
		}

		public static float Tanf(float x)
		{
			return (float) Math.Tan(x);
		}

		public static float Atanf(float x)
		{
			return (float) Math.Atan(x);
		}
	}
}

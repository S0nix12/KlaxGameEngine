using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KlaxMath
{
	public static class MathExtensions
	{
		public static BoundingBox TransformBoundingBox(this BoundingBox boundingBox, Matrix transformationMatrix)
		{
			Vector3 xa = transformationMatrix.Right * boundingBox.Minimum.X;
			Vector3 xb = transformationMatrix.Right * boundingBox.Maximum.X;

			Vector3 ya = transformationMatrix.Up * boundingBox.Minimum.Y;
			Vector3 yb = transformationMatrix.Up * boundingBox.Maximum.Y;

			Vector3 za = transformationMatrix.Backward * boundingBox.Minimum.Z;
			Vector3 zb = transformationMatrix.Backward * boundingBox.Maximum.Z;

			return new BoundingBox(
				Vector3.Min(xa, xb) + Vector3.Min(ya, yb) + Vector3.Min(za, zb) + transformationMatrix.TranslationVector,
				Vector3.Max(xa, xb) + Vector3.Max(ya, yb) + Vector3.Max(za, zb) + transformationMatrix.TranslationVector);
		}

		public static Vector3 ProjectOn(this Vector3 a, Vector3 b)
		{
			Vector3 unitB = Vector3.Normalize(b);
			return Vector3.Dot(a, unitB) * b;
		}
		public static Vector2 ProjectOn(this Vector2 a, Vector2 b)
		{			
			Vector2 unitB = Vector2.Normalize(b);
			return Vector2.Dot(a, unitB) * b;
		}

		public static Vector3 ToEuler(this Quaternion q)
		{
			float pitch;
			float yaw;
			float roll;

			double edgeCase = q.X * q.Y + q.Z * q.W;
			if (edgeCase > 0.49999995)
			{
				yaw = (float)(2 * Math.Atan2(q.X, q.W));
				pitch = MathUtil.PiOverTwo;
				roll = 0;
			}
			else if (edgeCase < -0.49999995)
			{
				yaw = (float)(-2 * Math.Atan2(q.X, q.W));
				pitch = -MathUtil.PiOverTwo;
				roll = 0;
			}
			else
			{
				yaw = (float)Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, 1 - 2 * q.Y * q.Y - 2 * q.Z * q.Z);
				roll = (float)Math.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, 1 - 2 * q.X * q.X - 2 * q.Z * q.Z);
				pitch = (float)Math.Asin(2 * q.X * q.Y + 2 * q.Z * q.W);
			}

			return new Vector3(roll, yaw, pitch);
		}
		public static Quaternion EulerToQuaternion(this Vector3 euler)
		{
			Quaternion outQuat;
			// Assuming the angles are in radians.
			double c1 = Math.Cos(euler.Y / 2);
			double s1 = Math.Sin(euler.Y / 2);
			double c2 = Math.Cos(euler.Z / 2);
			double s2 = Math.Sin(euler.Z / 2);
			double c3 = Math.Cos(euler.X / 2);
			double s3 = Math.Sin(euler.X / 2);
			double c1c2 = c1 * c2;
			double s1s2 = s1 * s2;
			outQuat.W = (float) (c1c2 * c3 - s1s2 * s3);
			outQuat.X = (float) (c1c2 * s3 + s1s2 * c3);
			outQuat.Y = (float)(s1 * c2 * c3 + c1 * s2 * s3);
			outQuat.Z = (float) (c1 * s2 * c3 - s1 * c2 * s3);
			
			return outQuat;
		}

		/// <summary>
		/// Returns the biggest component of this vector
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static float MaxComponent(this Vector3 v)
		{
			return Math.Max(v.X, Math.Max(v.Y, v.Z));
		}

		/// <summary>
		/// Returns the smallest component of this vector
		/// </summary>
		/// <param name="mat"></param>
		/// <returns></returns>
		public static float MinComponent(this Vector3 v)
		{
			return Math.Min(v.X, Math.Min(v.Y, v.Z));
		}

		public static Vector3 GetEulerRotation(this Matrix mat)
		{
			Vector3 outEuler;
			outEuler.X = (float) Math.Atan2(mat.M23, mat.M33);
			outEuler.Y = (float) Math.Atan2(-mat.M13, Math.Sqrt(mat.M23 * mat.M23 + mat.M33 * mat.M33));
			outEuler.Z = (float) Math.Atan2(mat.M12, mat.M11);
			return outEuler;
		}

		#region Conversions
		// System Numerics -> SharpDx
		public static Vector2 ToSharpVector(this System.Numerics.Vector2 a)
		{
			return new Vector2(a.X, a.Y);
		}

		public static Vector3 ToSharpVector(this System.Numerics.Vector3 a)
		{
			return new Vector3(a.X, a.Y, a.Z);
		}

		public static Vector4 ToSharpVector(this System.Numerics.Vector4 a)
		{
			return new Vector4(a.X, a.Y, a.Z, a.W);
		}

		public static Vector3 ToVector3(this Vector4 a)
		{
			return new Vector3(a.X, a.Y, a.Z);
		}

		public static Quaternion ToSharpQuat(this System.Numerics.Quaternion q)
		{
			return new Quaternion(q.X, q.Y, q.Z, q.W);
		}

		public static Matrix ToSharpMat(this System.Numerics.Matrix4x4 m)
		{
			return new Matrix(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
		}

		// SharpDx -> System Numerics
		public static System.Numerics.Vector2 ToNumericVector(this Vector2 a)
		{
			return new System.Numerics.Vector2(a.X, a.Y);
		}

		public static System.Numerics.Vector3 ToNumericVector(this Vector3 a)
		{
			return new System.Numerics.Vector3(a.X, a.Y, a.Z);
		}

		public static System.Numerics.Vector4 ToNumericVector(this Vector4 a)
		{
			return new System.Numerics.Vector4(a.X, a.Y, a.Z, a.W);
		}

		public static System.Numerics.Quaternion ToNumericQuat(this Quaternion q)
		{
			return new System.Numerics.Quaternion(q.X, q.Y, q.Z, q.W);
		}

		public static System.Numerics.Matrix4x4 ToNumericMat(this Matrix m)
		{
			return new System.Numerics.Matrix4x4(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
		}

		// BepuPhysics -> SharpDX
		public static BEPUutilities.Vector2 ToBepu(this Vector2 a)
		{
			return new BEPUutilities.Vector2(a.X, a.Y);
		}

		public static BEPUutilities.Vector3 ToBepu(this Vector3 a)
		{
			return new BEPUutilities.Vector3(a.X, a.Y, a.Z);
		}

		public static BEPUutilities.Vector4 ToBepu(this Vector4 a)
		{
			return new BEPUutilities.Vector4(a.X, a.Y, a.Z, a.W);
		}

		public static BEPUutilities.Quaternion ToBepu(this Quaternion q)
		{
			return new BEPUutilities.Quaternion(q.X, q.Y, q.Z, q.W);
		}

		public static BEPUutilities.Matrix ToBepu(this Matrix m)
		{
			return new BEPUutilities.Matrix(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
		}

		public static BEPUutilities.BoundingBox ToBepu(this BoundingBox bb)
		{
			return new BEPUutilities.BoundingBox(bb.Minimum.ToBepu(), bb.Maximum.ToBepu());
		}

		public static BEPUutilities.BoundingSphere ToBepu(this BoundingSphere bs)
		{
			return new BEPUutilities.BoundingSphere(bs.Center.ToBepu(), bs.Radius);
		}

		public static BEPUutilities.Ray ToBepu(this Ray r)
		{
			return new BEPUutilities.Ray(r.Position.ToBepu(), r.Direction.ToBepu());
		}

		public static BEPUutilities.Plane ToBepu(this Plane p)
		{
			return new BEPUutilities.Plane(p.Normal.ToBepu(), -p.D);
		}

		// SharpDX -> BepuPhysics
		public static Vector2 ToSharp(this BEPUutilities.Vector2 a)
		{
			return new Vector2(a.X, a.Y);
		}

		public static Vector3 ToSharp(this BEPUutilities.Vector3 a)
		{
			return new Vector3(a.X, a.Y, a.Z);
		}

		public static Vector4 ToSharp(this BEPUutilities.Vector4 a)
		{
			return new Vector4(a.X, a.Y, a.Z, a.W);
		}

		public static Quaternion ToSharp(this BEPUutilities.Quaternion q)
		{
			return new Quaternion(q.X, q.Y, q.Z, q.W);
		}

		public static Matrix ToSharp(this BEPUutilities.Matrix m)
		{
			return new Matrix(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
		}

		public static BoundingBox ToSharp(this BEPUutilities.BoundingBox bb)
		{			
			return new BoundingBox(bb.Min.ToSharp(), bb.Max.ToSharp());
		}

		public static BoundingSphere ToSharp(this BEPUutilities.BoundingSphere bs)
		{
			return new BoundingSphere(bs.Center.ToSharp(), bs.Radius);
		}

		public static Ray ToSharp(this BEPUutilities.Ray r)
		{
			return new Ray(r.Position.ToSharp(), r.Direction.ToSharp());
		}

		public static Plane ToSharp(this BEPUutilities.Plane p)
		{
			return new Plane(p.Normal.ToSharp(), -p.D);
		}
		#endregion
	}
}

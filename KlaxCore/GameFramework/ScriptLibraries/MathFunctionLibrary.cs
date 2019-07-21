using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxMath;
using KlaxShared.Attributes;
using SharpDX;

namespace KlaxCore.GameFramework.ScriptLibraries
{
	[KlaxLibrary]
	public static class MathFunctionLibrary
	{
		[KlaxFunction(Category = "Bool", DisplayName = "And", IsImplicit = true)]
		public static bool AndBool(bool a, bool b) { return a && b; }

		[KlaxFunction(Category = "Bool", DisplayName = "Or", IsImplicit = true)]
		public static bool OrBool(bool a, bool b) { return a || b; }


		////////////////////////////////////////////////////////////////////////////////
		[KlaxFunction(Category = "Float", DisplayName = "Add", IsImplicit = true)]
		public static float AddFloat(float a, float b) { return a + b; }

		[KlaxFunction(Category = "Float", DisplayName = "Subtract", IsImplicit = true)]
		public static float SubtractFloat(float a, float b) { return a - b; }

		[KlaxFunction(Category = "Float", DisplayName = "Multiply", IsImplicit = true)]
		public static float MultiplyFloat(float a, float b) { return a * b; }

		[KlaxFunction(Category = "Float", DisplayName = "Divide", IsImplicit = true)]
		public static float DivideFloat(float a, float b) { return a / b; }

		[KlaxFunction(Category = "Float", DisplayName = "Modulus", IsImplicit = true)]
		public static float ModulusFloat(float a, float b) { return a % b; }

		[KlaxFunction(Category = "Float", DisplayName = "Abs", IsImplicit = true)]
		public static float AbsFloat(float a) { return Math.Abs(a); }

		[KlaxFunction(Category = "Float", DisplayName = "Floor (Int)", IsImplicit = true)]
		public static int FloorToInt(float a) { return (int)Math.Floor(a); }

		[KlaxFunction(Category = "Float", DisplayName = "Floor (Float)", IsImplicit = true)]
		public static float FloorToFloat(float a) { return (float)Math.Floor(a); }

		[KlaxFunction(Category = "Float", DisplayName = "Ceil (Int)", IsImplicit = true)]
		public static int CeilToInt(float a) { return (int)Math.Ceiling(a); }

		[KlaxFunction(Category = "Float", DisplayName = "Ceil (Float)", IsImplicit = true)]
		public static float CeilToFloat(float a) { return (float)Math.Ceiling(a); }

		[KlaxFunction(Category = "Float", DisplayName = "Round to Int", IsImplicit = true)]
		public static int RoundToInt(float a) { return (int)Math.Round(a); }

		[KlaxFunction(Category = "Float", DisplayName = "Round to Float", IsImplicit = true)]
		public static float RoundToFloat(float a) { return (float)Math.Round(a); }

		[KlaxFunction(Category = "Float", DisplayName = "Greater (>)", IsImplicit = true)]
		public static bool GreaterThanFloat(float a, float b) { return a > b; }

		[KlaxFunction(Category = "Float", DisplayName = "Greater/Equal (>=)", IsImplicit = true)]
		public static bool GreaterThanOrEqualFloat(float a, float b) { return a >= b; }

		[KlaxFunction(Category = "Float", DisplayName = "Equal (==)", IsImplicit = true)]
		public static bool EqualFloat(float a, float b) { return a == b; }

		[KlaxFunction(Category = "Float", DisplayName = "Roughly Equal (==)", IsImplicit = true)]
		public static bool RoughlyEqualFloat(float a, float b, float tolerance = 0.001f) { return Math.Abs(Math.Abs(a) - Math.Abs(b)) < tolerance; }

		[KlaxFunction(Category = "Float", DisplayName = "Smaller (<)", IsImplicit = true)]
		public static bool SmallerThanFloat(float a, float b) { return a < b; }

		[KlaxFunction(Category = "Float", DisplayName = "Smaller/Equal (<=)", IsImplicit = true)]
		public static bool SmallerThanOrEqualFloat(float a, float b) { return a <= b; }

		[KlaxFunction(Category = "Float", IsImplicit = true)]
		public static float Sin(float x) { return (float) Math.Sin(x); }

		[KlaxFunction(Category = "Float", IsImplicit = true)]
		public static float Cos(float x) { return (float) Math.Cos(x); }

		[KlaxFunction(Category = "Float", IsImplicit = true)]
		public static float Tan(float x) { return (float) Math.Tan(x); }


		////////////////////////////////////////////////////////////////////////////////
		[KlaxFunction(Category = "Int", DisplayName = "Add", IsImplicit = true)]
		public static int AddInt(int a, int b) { return a + b; }

		[KlaxFunction(Category = "Int", DisplayName = "Subtract", IsImplicit = true)]
		public static int SubtractInt(int a, int b) { return a - b; }

		[KlaxFunction(Category = "Int", DisplayName = "Multiply", IsImplicit = true)]
		public static int MultiplyInt(int a, int b) { return a * b; }

		[KlaxFunction(Category = "Int", DisplayName = "Divide", IsImplicit = true)]
		public static int DivideInt(int a, int b) { return a / b; }

		[KlaxFunction(Category = "Int", DisplayName = "Modulus", IsImplicit = true)]
		public static int ModulusInt(int a, int b) { return a % b; }

		[KlaxFunction(Category = "Int", DisplayName = "Abs", IsImplicit = true)]
		public static float AbsInt(int a) { return Math.Abs(a); }

		[KlaxFunction(Category = "Int", DisplayName = "Greater (>)", IsImplicit = true)]
		public static bool GreaterThanInt(int a, int b) { return a > b; }

		[KlaxFunction(Category = "Int", DisplayName = "Greater/Equal (>=)", IsImplicit = true)]
		public static bool GreaterThanOrEqualInt(int a, int b) { return a >= b; }

		[KlaxFunction(Category = "Int", DisplayName = "Equal (==)", IsImplicit = true)]
		public static bool EqualInt(int a, int b) { return a == b; }

		[KlaxFunction(Category = "Int", DisplayName = "Smaller (<)", IsImplicit = true)]
		public static bool SmallerThanInt(int a, int b) { return a < b; }

		[KlaxFunction(Category = "Int", DisplayName = "Smaller/Equal (<=)", IsImplicit = true)]
		public static bool SmallerThanOrEqualInt(int a, int b) { return a <= b; }


		////////////////////////////////////////////////////////////////////////////////
		[KlaxFunction(Category = "Vector2", DisplayName = "Add", IsImplicit = true)]
		public static Vector2 AddVector2(Vector2 a, Vector2 b) { return a + b; }

		[KlaxFunction(Category = "Vector2", DisplayName = "Subtract", IsImplicit = true)]
		public static Vector2 SubtractVector2(Vector2 a, Vector2 b) { return a - b; }

		[KlaxFunction(Category = "Vector2", DisplayName = "Dot", IsImplicit = true)]
		public static float DotVector2(Vector2 a, Vector2 b) { Vector2.Dot(ref a, ref b, out float result); return result; }

		[KlaxFunction(Category = "Vector2", DisplayName = "Length", IsImplicit = true)]
		public static float LengthVector2(Vector2 a) { return a.Length(); }

		[KlaxFunction(Category = "Vector2", DisplayName = "Length Squared", IsImplicit = true)]
		public static float LengthSqVector2(Vector2 a) { return a.LengthSquared(); }

		[KlaxFunction(Category = "Vector2", DisplayName = "Scale", IsImplicit = true)]
		public static Vector2 ScaleVector2(Vector2 a, float b) { return a * b; }

		[KlaxFunction(Category = "Vector2", DisplayName = "Get X", IsImplicit = true)]
		public static float GetXVector2(Vector2 a) { return a.X; }

		[KlaxFunction(Category = "Vector2", DisplayName = "Get Y", IsImplicit = true)]
		public static float GetYVector2(Vector2 a) { return a.Y; }

		[KlaxFunction(Category = "Vector2", DisplayName = "Lerp", IsImplicit = true)]
		public static Vector2 LerpVector2(Vector2 a, Vector2 b, float alpha) { return Vector2.Lerp(a, b, alpha); }

		[KlaxFunction(Category = "Vector2", DisplayName = "MakeVector", IsImplicit = true)]
		public static Vector2 MakeVector2(float x, float y) { return new Vector2(x, y); }

		[KlaxFunction(Category = "Vector2", DisplayName = "Normalize", IsImplicit = true)]
		public static Vector2 NormalizeVector2(Vector2 a) { return Vector2.Normalize(a); }


		////////////////////////////////////////////////////////////////////////////////
		[KlaxFunction(Category = "Vector3", DisplayName = "Add", IsImplicit = true)]
		public static Vector3 AddVector3(Vector3 a, Vector3 b) { return a + b; }

		[KlaxFunction(Category = "Vector3", DisplayName = "Subtract", IsImplicit = true)]
		public static Vector3 SubtractVector3(Vector3 a, Vector3 b) { return a - b; }

		[KlaxFunction(Category = "Vector3", DisplayName = "Dot", IsImplicit = true)]
		public static float DotVector3(Vector3 a, Vector3 b) { return Vector3.Dot(a, b); }

		[KlaxFunction(Category = "Vector3", DisplayName = "Cross", IsImplicit = true)]
		public static Vector3 CrossVector3(Vector3 a, Vector3 b) { return Vector3.Cross(a, b); }

		[KlaxFunction(Category = "Vector3", DisplayName = "Length", IsImplicit = true)]
		public static float LengthVector3(Vector3 a) { return a.Length(); }

		[KlaxFunction(Category = "Vector3", DisplayName = "Length Squared", IsImplicit = true)]
		public static float LengthSqVector3(Vector3 a) { return a.LengthSquared(); }

		[KlaxFunction(Category = "Vector3", DisplayName = "Scale", IsImplicit = true)]
		public static Vector3 ScaleVector3(Vector3 a, float b) { return a * b; }

		[KlaxFunction(Category = "Vector3", DisplayName = "Get X", IsImplicit = true)]
		public static float GetXVector3(Vector3 a) { return a.X; }

		[KlaxFunction(Category = "Vector3", DisplayName = "Get Y", IsImplicit = true)]
		public static float GetYVector3(Vector3 a) { return a.Y; }

		[KlaxFunction(Category = "Vector3", DisplayName = "Get Z", IsImplicit = true)]
		public static float GetZVector3(Vector3 a) { return a.Z; }

		[KlaxFunction(Category = "Vector3", DisplayName = "Lerp", IsImplicit = true)]
		public static Vector3 LerpVector3(Vector3 a, Vector3 b, float alpha) { return Vector3.Lerp(a, b, alpha); }

		[KlaxFunction(Category = "Vector3", DisplayName = "Rotate", IsImplicit = true)]
		public static Vector3 RotateVector3(Vector3 a, Quaternion rotation) { return Vector3.Transform(a, rotation); }

		[KlaxFunction(Category = "Vector3", DisplayName = "MakeVector", IsImplicit = true)]
		public static Vector3 MakeVector3(float x, float y, float z) {  return new Vector3(x,y,z); }

		[KlaxFunction(Category = "Vector3", DisplayName = "Normalize", IsImplicit = true)]
		public static Vector3 NormalizeVector3(Vector3 a) { return Vector3.Normalize(a); }


		////////////////////////////////////////////////////////////////////////////////
		[KlaxFunction(Category = "Quaternion", DisplayName = "Slerp", IsImplicit = true)]
		public static Quaternion SlerpQuaternion(Quaternion a, Quaternion b, float alpha) { return Quaternion.Slerp(a, b, alpha); }

		[KlaxFunction(Category = "Quaternion", DisplayName = "Lerp", IsImplicit = true)]
		public static Quaternion LerpQuaternion(Quaternion a, Quaternion b, float alpha) { return Quaternion.Lerp(a, b, alpha); }

		[KlaxFunction(Category = "Quaternion", DisplayName = "ToEuler", IsImplicit = true)]
		public static Vector3 ToEuler(Quaternion a) { return a.ToEuler(); }

		[KlaxFunction(Category = "Quaternion", DisplayName = "FromEuler", IsImplicit = true)]
		public static Quaternion FromEuler(Vector3 a) { return a.EulerToQuaternion(); }

		[KlaxFunction(Category = "Quaternion", DisplayName = "FromDirection", IsImplicit = true)]
		public static Quaternion FromDirection(Vector3 forward, Vector3 up) { return MathUtilities.CreateLookAtQuaternion(forward, up); }
		
		[KlaxFunction(Category = "Quaternion", IsImplicit = true, Tooltip = "Combines the 2 given quaternions (b * a)")]
		public static Quaternion Combine(Quaternion a, Quaternion b) { return b * a; }

		[KlaxFunction(Category = "Quaternion", IsImplicit = true, Tooltip = "Creates a rotation around the given axis by the given angle in degree")]
		public static Quaternion FromAxisAngle(Vector3 axis, float angleDegree) { return Quaternion.RotationAxis(axis, MathUtil.DegreesToRadians(angleDegree)); }
	}
}

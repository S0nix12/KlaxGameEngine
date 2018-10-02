using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KlaxMath
{
    /// <summary>
    /// A Transform defines Position, Rotation and Scale in 3d Space
    /// Each object that can exist in the world should have a transform
    /// Position uses a Vector3
    /// Rotation uses a Quaternion
    /// Scale uses a Vector3
    /// </summary>
    public class Transform
    {
        public Transform()
        {}

        public Transform(Vector3 position)
        {
            Position = position;
        }

        public Transform(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public Matrix GetTRSMatrix()
        {
            return Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Scale, Vector3.Zero, Rotation, Position);
        }

        public Vector3 Forward
        { get { return Vector3.Transform(Axis.Forward, Rotation); } }

        public Vector3 Right
        { get { return Vector3.Transform(Axis.Right, Rotation); } }

        public Vector3 Up
        { get { return Vector3.Transform(Axis.Up, Rotation); } }

        public Vector3 Position
        { get; set; } = Vector3.Zero;

        public Vector3 Scale
        { get; set; } = Vector3.One;

        public Quaternion Rotation
        { get; set; } = Quaternion.Identity;
    }
}

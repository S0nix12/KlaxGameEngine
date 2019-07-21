using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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
		public event Action<Vector3> OnPositionChanged;
		public event Action<Quaternion> OnRotationChanged;
		public event Action<Vector3> OnScaleChanged;

		public Transform()
		{ }

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

		/// <summary>
		/// Rotates the transform with the given quaternion in it's local space
		/// </summary>
		/// <param name="deltaRotation"></param>
		public void RotateLocal(in Quaternion deltaRotation)
		{
			Rotation = deltaRotation * Rotation;
		}

		public void SetWorldPosition(in Vector3 worldPosition)
		{
			if (m_parent == null)
			{
				Position = worldPosition;
			}
			else
			{
				Matrix parentWorldMatrix = m_parent.WorldMatrix;
				parentWorldMatrix.Invert();
				Position = Vector3.TransformCoordinate(worldPosition, parentWorldMatrix);
			}
		}

		public void SetWorldRotation(in Quaternion worldRotation)
		{
			if (m_parent == null)
			{
				Rotation = worldRotation;
			}
			else
			{
				Quaternion parentWorldRotation = m_parent.WorldRotation;
				parentWorldRotation.Invert();
				Rotation = parentWorldRotation * worldRotation;
			}
		}

		public void SetWorldScale(in Vector3 worldScale)
		{
			if (m_parent == null)
			{
				Scale = worldScale;
			}
			else
			{
				Vector3 parentWorldScaleInverse = 1 / m_parent.WorldScale;
				Scale = parentWorldScaleInverse * worldScale;
			}
		}

		public void SetFromMatrix(in Matrix matrix)
		{
			matrix.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 position);

			Scale = scale;
			Position = position;
			Rotation = rotation;

			MarkWorldMatrixDirty();
		}

		private void MarkWorldMatrixDirty()
		{
			m_bIsWorldMatrixDirty = true;
			for (int i = 0; i < m_children.Count; i++)
			{
				m_children[i].MarkWorldMatrixDirty();
			}
		}

		private void SendPositionChanged()
		{
			OnPositionChanged?.Invoke(WorldPosition);
			foreach (var child in m_children)
			{
				child.SendPositionChanged();
			}
		}

		private void SendRotationChanged()
		{
			OnRotationChanged?.Invoke(WorldRotation);
			foreach (var child in m_children)
			{
				child.SendRotationChanged();
			}
		}

		private void SendScaleChanged()
		{
			OnScaleChanged?.Invoke(WorldScale);
			foreach (var child in m_children)
			{
				child.SendScaleChanged();
			}
		}

		[JsonIgnore]
		public Vector3 Forward { get { return Vector3.Normalize(Vector3.TransformNormal(Axis.Forward, WorldMatrix)); } }

		[JsonIgnore]
		public Vector3 Right { get { return Vector3.Normalize(Vector3.TransformNormal(Axis.Right, WorldMatrix)); } }

		[JsonIgnore]
		public Vector3 Up { get { return Vector3.Normalize(Vector3.TransformNormal(Axis.Up, WorldMatrix)); } }

		private Vector3 m_position = Vector3.Zero;
		/// <summary>
		/// Position local to the transforms parent
		/// </summary>
		public Vector3 Position
		{
			get { return m_position; }
			set
			{
				m_position = value;
				MarkWorldMatrixDirty();
				SendPositionChanged();
			}
		}

		private Vector3 m_scale = Vector3.One;
		/// <summary>
		/// Scale local to the transforms parent
		/// </summary>
		public Vector3 Scale
		{
			get { return m_scale; }
			set
			{
				m_scale = value;
				MarkWorldMatrixDirty();
				SendScaleChanged();
			}
		}

		private Quaternion m_rotation = Quaternion.Identity;
		/// <summary>
		/// Rotation local to the transforms parent
		/// </summary>
		public Quaternion Rotation
		{
			get { return m_rotation; }
			set
			{
				m_rotation = value;
				MarkWorldMatrixDirty();
				SendRotationChanged();
			}
		}

		[JsonIgnore]
		public Vector3 WorldScale
		{
			get
			{
				if (m_parent == null)
				{
					return m_scale;
				}
				//WorldMatrix.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 position);
				return m_parent.WorldScale * m_scale;
			}
		}

		/// <summary>
		/// Rotation in world space
		/// </summary>
		[JsonIgnore]
		public Quaternion WorldRotation
		{
			get
			{
				if (m_parent == null)
				{
					return m_rotation;
				}
				//WorldMatrix.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 position);
				//return rotation;
				return m_parent.WorldRotation * m_rotation;
			}
		}

		[JsonIgnore]
		public Vector3 WorldPosition
		{
			get
			{
				if (m_parent == null)
				{
					return m_position;
				}
				//WorldMatrix.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 position);
				//return position;
				return Vector3.Transform(m_parent.WorldScale * m_position, m_parent.WorldRotation) + m_parent.WorldPosition;
			}
		}

		private readonly List<Transform> m_children = new List<Transform>();
		private Transform m_parent = null;
		public Transform Parent
		{
			get { return m_parent; }
			set
			{
				if (value != m_parent)
				{
					m_parent?.m_children.Remove(this);
					MarkWorldMatrixDirty();
					m_parent = value;
					m_parent?.m_children.Add(this);
					OnPositionChanged?.Invoke(WorldPosition);
					OnRotationChanged?.Invoke(WorldRotation);
					OnScaleChanged?.Invoke(WorldScale);
				}
			}
		}

		[JsonIgnore]
		public Matrix LocalMatrix
		{
			get { return MathUtilities.CreateLocalTransformationMatrix(m_scale, m_rotation, m_position); }
		}

		private bool m_bIsWorldMatrixDirty;
		private Matrix m_cachedWorldMatrix = Matrix.Identity;
		[JsonIgnore]
		public Matrix WorldMatrix
		{
			get
			{
				if (m_bIsWorldMatrixDirty)
				{
					m_cachedWorldMatrix = LocalMatrix;
					if (m_parent != null)
					{
						m_cachedWorldMatrix = MathUtilities.CreateLocalTransformationMatrix(WorldScale, WorldRotation, WorldPosition);
					}
					m_bIsWorldMatrixDirty = false;
				}

				return m_cachedWorldMatrix;
			}
		}
	}
}

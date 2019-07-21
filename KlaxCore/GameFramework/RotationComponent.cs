using KlaxRenderer;
using KlaxRenderer.Debug;
using KlaxShared.Attributes;
using SharpDX;

namespace KlaxCore.GameFramework
{
	[KlaxComponent(Category = "Physics")]
	class CRotationComponent : CEntityComponent
	{
		public override void Init()
		{
			base.Init();
			m_updateScope = World.UpdateScheduler.Connect(Update, EUpdatePriority.Default);
		}

		public override void Shutdown()
		{
			base.Shutdown();
			m_updateScope?.Disconnect();
		}

		private void Update(float deltaTime)
		{
			if (RotationAxis.IsZero)
			{
				return;
			}

			Quaternion deltaRotation = Quaternion.RotationAxis(RotationAxis, RotationSpeed * deltaTime);
			Quaternion entityLocalRot = Owner.GetLocalRotation();

			Quaternion entityRotation = UseWorldAxis ? deltaRotation * entityLocalRot : entityLocalRot * deltaRotation;
			Owner.SetLocalRotation(in entityRotation);

			if (DrawRotationAxis)
			{
				Vector3 drawAxis = UseWorldAxis ? RotationAxis : Vector3.Transform(RotationAxis, entityRotation);
				CRenderer.Instance.ActiveScene.DebugRenderer.DrawArrow(Owner.GetWorldPosition(), drawAxis, 1.5f, Color.Yellow.ToColor4(), 0.0f, EDebugDrawCommandFlags.None);
			}
		}

		/// <summary>
		/// The axis about which to rotate the entity
		/// </summary>
		[KlaxProperty]
		public Vector3 RotationAxis { get; set; }

		/// <summary>
		/// The speed in radian angles per second with which to rotate the entity
		/// </summary>
		[KlaxProperty]
		public float RotationSpeed { get; set; }

		/// <summary>
		/// If true the Rotation Axis will be treated in world space otherwise we rotate in local space
		/// </summary>
		[KlaxProperty]
		public bool UseWorldAxis { get; set; }
		[KlaxProperty]
		public bool DrawRotationAxis { get; set; }

		private CUpdateScope m_updateScope;
	}
}

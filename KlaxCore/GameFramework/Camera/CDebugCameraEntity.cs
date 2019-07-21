using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using KlaxCore.Core;
using KlaxIO.Input;
using KlaxMath;
using KlaxShared.Attributes;
using SharpDX;

namespace KlaxCore.GameFramework.Camera
{
	[KlaxScriptType(Name = "DebugCameraEntity")]
	class CDebugCameraEntity : CEntity
	{
		public CDebugCameraEntity()
		{
			ShowInOutliner = false;
		}

		public override void Init(CWorld world, object userData)
		{
			base.Init(world, userData);
			m_cameraComponent = AddComponent<CCameraComponent>(true, false);
			m_cameraComponent.RegisterDuringInit = true;
			m_cameraComponent.Init();

			m_updateScope = World.UpdateScheduler.Connect(Update, EUpdatePriority.Editor);
		}

		public override void Shutdown()
		{
			base.Shutdown();
			m_updateScope?.Disconnect();
		}

		public new void Update(float deltaTime)
		{
			if (!Input.IsInputClassActive(InputClass))
			{
				return;
			}

			// Read Mouse and Update Rotation
			bool bRightMouseDown = Input.IsButtonPressed(EInputButton.MouseRightButton);

			float upAxisRotationAngle = 0.0f;
			float rightAxisRotationAngle = 0.0f;

			if (bRightMouseDown)
			{
				upAxisRotationAngle = Input.GetNativeAxisValue(EInputAxis.MouseX) * 0.005f;
				rightAxisRotationAngle = Input.GetNativeAxisValue(EInputAxis.MouseY) * 0.005f;
			}
			else
			{
				float rightStickX = Input.GetNativeAxisValue(EInputAxis.ControllerRightStickX);
				float rightStickY = Input.GetNativeAxisValue(EInputAxis.ControllerRightStickY);

				if (Math.Abs(rightStickX) > 0.15f || Math.Abs(rightStickY) > 0.15f)
				{
					upAxisRotationAngle = rightStickX * deltaTime * 3;
					rightAxisRotationAngle = rightStickY * deltaTime * 3;
				}
			}

			if (Math.Abs(upAxisRotationAngle) > 0.0f || Math.Abs(rightAxisRotationAngle) > 0.0f)
			{
				Quaternion xRotation = Quaternion.RotationAxis(KlaxMath.Axis.Up, upAxisRotationAngle);
				SetWorldRotation(xRotation * GetWorldRotation());
				Quaternion yRotation = Quaternion.RotationAxis(GetRight(), rightAxisRotationAngle);
				SetWorldRotation(yRotation * GetWorldRotation());
			}

			// Read keyboard state and update movement

			Vector3 deltaMove = Vector3.Zero;
			if (bRightMouseDown)
			{

				// Keyboard Input
				bool bWDown = Input.IsButtonPressed(EInputButton.W);
				bool bSDown = Input.IsButtonPressed(EInputButton.S);
				bool bDDown = Input.IsButtonPressed(EInputButton.D);
				bool bADown = Input.IsButtonPressed(EInputButton.A);
				bool bEDown = Input.IsButtonPressed(EInputButton.E);
				bool bQDown = Input.IsButtonPressed(EInputButton.Q);

				// Build movement vector
				if (bWDown)
				{
					deltaMove += GetForward();
				}

				if (bDDown)
				{
					deltaMove += GetRight();
				}

				if (bSDown)
				{
					deltaMove -= GetForward();
				}

				if (bADown)
				{
					deltaMove -= GetRight();
				}

				if (bEDown)
				{
					deltaMove += KlaxMath.Axis.Up;
				}

				if (bQDown)
				{
					deltaMove -= KlaxMath.Axis.Up;
				}
				deltaMove.Normalize();
			}
			else
			{
				// Controller Input
				float leftStickX = Input.GetNativeAxisValue(EInputAxis.ControllerLeftStickX);
				if (Math.Abs(leftStickX) > 0.15f)
				{
					deltaMove += GetRight() * leftStickX;
				}

				float leftStickY = Input.GetNativeAxisValue(EInputAxis.ControllerLeftStickY);
				if (Math.Abs(leftStickY) > 0.15f)
				{
					deltaMove += GetForward() * leftStickY;
				}

				float rightTrigger = Input.GetNativeAxisValue(EInputAxis.ControllerRightTrigger);
				if (rightTrigger > 0.01f)
				{
					deltaMove -= KlaxMath.Axis.Up * rightTrigger;
				}

				float leftTrigger = Input.GetNativeAxisValue(EInputAxis.ControllerLeftTrigger);
				if (leftTrigger > 0.01f)
				{
					deltaMove += KlaxMath.Axis.Up * leftTrigger;
				}
			}

			if (!deltaMove.IsZero)
			{
				bool bShiftDown = Input.IsButtonPressed(EInputButton.LeftShift) || Input.IsButtonPressed(EInputButton.RightShift);
				deltaMove = bShiftDown ? deltaMove * MoveSpeedFast : deltaMove * MoveSpeedSlow;
				deltaMove *= deltaTime;
				SetWorldPosition(GetWorldPosition() + deltaMove);
			}
		}

		public float MoveSpeedSlow { get; set; } = 15f;
		public float MoveSpeedFast { get; set; } = 30f;
		public EInputClass InputClass { get; set; } = EInputClass.Default;

		private CCameraComponent m_cameraComponent;
		private CUpdateScope m_updateScope;
	}
}

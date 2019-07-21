using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using KlaxIO.Input;
using SharpDX;
using SharpDX.DirectInput;

namespace KlaxRenderer.Camera
{
	public class CDebugCamera : CBaseCamera
	{
		public void Update(float deltaTime)
		{
			// Read Mouse and Update Rotation
			m_bRightMouseDown = Input.IsButtonPressed(EInputButton.MouseRightButton);

			float upAxisRotationAngle = 0.0f;
			float rightAxisRotationAngle = 0.0f;

			if (m_bRightMouseDown)
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
				Transform.RotateLocal(xRotation);
				Quaternion yRotation = Quaternion.RotationAxis(Transform.Right, rightAxisRotationAngle);
				Transform.RotateLocal(yRotation);
			}

			// Read keyboard state and update movement

			Vector3 deltaMove = Vector3.Zero;
			if (m_bRightMouseDown)
			{
				// Keyboard Input
				m_bShiftDown = Input.IsButtonPressed(EInputButton.LeftShift) || Input.IsButtonPressed(EInputButton.RightShift);

				bool bWDown = Input.IsButtonPressed(EInputButton.W);
				bool bSDown = Input.IsButtonPressed(EInputButton.S);
				bool bDDown = Input.IsButtonPressed(EInputButton.D);
				bool bADown = Input.IsButtonPressed(EInputButton.A);
				bool bEDown = Input.IsButtonPressed(EInputButton.E);
				bool bQDown = Input.IsButtonPressed(EInputButton.Q);

				// Build movement vector
				if (bWDown)
				{
					deltaMove += Transform.Forward;
				}

				if (bDDown)
				{
					deltaMove += Transform.Right;
				}

				if (bSDown)
				{
					deltaMove -= Transform.Forward;
				}

				if (bADown)
				{
					deltaMove -= Transform.Right;
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
					deltaMove += Transform.Right * leftStickX;
				}

				float leftStickY = Input.GetNativeAxisValue(EInputAxis.ControllerLeftStickY);
				if (Math.Abs(leftStickY) > 0.15f)
				{
					deltaMove += Transform.Forward * leftStickY;
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
				deltaMove = m_bShiftDown ? deltaMove * MoveSpeedFast : deltaMove * MoveSpeedSlow;
				deltaMove *= deltaTime;
				Transform.Position += deltaMove;
			}
		}

		public float MoveSpeedSlow { get; set; } = 15f;
		public float MoveSpeedFast { get; set; } = 30f;
		private bool m_bShiftDown;		
		private bool m_bRightMouseDown;
	}
}

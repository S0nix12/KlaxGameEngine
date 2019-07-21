using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxMath;
using KlaxShared;

namespace KlaxIO.Input
{
	public enum EInputAxis
	{
		MouseX,
		MouseY,
		MouseWheel,
		ControllerLeftStickX,
		ControllerLeftStickY,
		ControllerRightStickX,
		ControllerRightStickY,
		ControllerRightTrigger,
		ControllerLeftTrigger,
		Count
	}

	struct SInputAxisButtonPoint
	{
		public EInputButton button;
		public float scale;
	}

	// Prototype code for custom input axis that can be defined by users
	/*
	public class CInputAxisTarget
	{
		internal void UpdateAxis()
		{
			// Read Axis
			for (int i = 0; i < m_nativeAxises.Count; i++)
			{
				// Special handling for mouse axis as it can be higher than 1.0f
				EInputAxis nativeAxis = m_nativeAxises[i];
				float axisValue = Input.GetNativeAxisValue(nativeAxis);
				if ((nativeAxis == EInputAxis.MouseX || nativeAxis == EInputAxis.MouseY || nativeAxis == EInputAxis.MouseWheel) && Math.Abs(axisValue) > 1.0f)
				{
					m_currentValue = axisValue;
					return; // Early out here as we don't want to clamp mouse input					
				}
				m_currentValue += axisValue;
			}

			// Read Buttons
			m_currentValue = 0.0f;
			for (int i = 0; i < m_customPoints.Count; ++i)
			{
				SInputAxisButtonPoint point = m_customPoints[i];
				if (Input.IsButtonPressed(point.button))
				{
					m_currentValue += point.scale;
				}
			}

			m_currentValue = MathUtilities.Clamp(m_currentValue, -1.0f, 1.0f);
		}

		public float GetCurrentValue()
		{
			return m_currentValue;
		}

		private SHashedName m_axisName;
		private float m_currentValue;
		private readonly List<EInputAxis> m_nativeAxises = new List<EInputAxis>();
		private readonly List<SInputAxisButtonPoint> m_customPoints = new List<SInputAxisButtonPoint>();
	}
	*/
}

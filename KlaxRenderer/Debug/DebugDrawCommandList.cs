using System.Collections.Generic;
using System.Reflection;
using KlaxRenderer.Camera;
using KlaxRenderer.Debug.Primitives;
using KlaxShared.Utilities;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Debug
{
	enum EDebugPrimitiveType
	{
		Cube,
		Sphere,
		Cylinder,
		Cone,
		Pyramid,
		Hemisphere,
		Count
	}

	class CDebugDrawCommandList
	{
		public CDebugDrawCommandList()
		{
			for (int i = 0; i < (int)EDebugPrimitiveType.Count; i++)
			{
				m_persistentDrawDataPerPrimitive[i] = new List<PrimitivePerInstanceData>();
				m_frameDrawDataPerPrimitive[i] = new List<PrimitivePerInstanceData>();
				m_remainingTimesPerPrimitive[i] = new List<float>();
			}

			m_listItemsFieldInfo = typeof(List<PrimitivePerInstanceData>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public void Update(float deltaTime)
		{
			for (int i = 0; i < (int)EDebugPrimitiveType.Count; i++)
			{
				List<float> remainingTimes = m_remainingTimesPerPrimitive[i];
				for (int j = remainingTimes.Count - 1; j >= 0; --j)
				{
					remainingTimes[j] -= deltaTime;
					if (remainingTimes[j] <= 0.0f)
					{
						ContainerUtilities.RemoveSwapAt(remainingTimes, j);
						ContainerUtilities.RemoveSwapAt(m_persistentDrawDataPerPrimitive[i], j);
					}
				}
			}
		}

		public void Draw(DeviceContext deviceContext, CDebugPrimitiveMap primitiveMap)
		{
			for (int i = 0; i < (int)EDebugPrimitiveType.Count; i++)
			{
				List<PrimitivePerInstanceData> persistentDrawData = m_persistentDrawDataPerPrimitive[i];
				if (persistentDrawData.Count > 0)
				{
					CDebugDrawPrimitive drawPrimitive = primitiveMap.GetPrimitiveInstance(i);
					var value = (PrimitivePerInstanceData[])m_listItemsFieldInfo.GetValue(persistentDrawData);

					drawPrimitive.Draw(deviceContext, value, persistentDrawData.Count);
				}

				List<PrimitivePerInstanceData> frameDrawData = m_frameDrawDataPerPrimitive[i];
				if (frameDrawData.Count > 0)
				{
					CDebugDrawPrimitive drawPrimitive = primitiveMap.GetPrimitiveInstance(i);
					var value = (PrimitivePerInstanceData[])m_listItemsFieldInfo.GetValue(frameDrawData);

					drawPrimitive.Draw(deviceContext, value, frameDrawData.Count);
				}

				frameDrawData.Clear();
			}
		}
		
		public bool HasAnyCommands()
		{
			for (int i = 0; i < (int)EDebugPrimitiveType.Count; i++)
			{
				if (m_persistentDrawDataPerPrimitive[i].Count > 0)
				{
					return true;
				}

				if (m_frameDrawDataPerPrimitive[i].Count > 0)
				{
					return true;
				}
			}
			return false;
		}

		public void AddInstance(EDebugPrimitiveType primitiveType, float displayTime, in PrimitivePerInstanceData instanceData)
		{
			int primitiveIndex = (int) primitiveType;
			if (displayTime > 0.0f)
			{
				m_persistentDrawDataPerPrimitive[primitiveIndex].Add(instanceData);
				m_remainingTimesPerPrimitive[primitiveIndex].Add(displayTime);
			}
			else
			{
				m_frameDrawDataPerPrimitive[primitiveIndex].Add(instanceData);
			}
		}

		public void ClearCommands()
		{
			for (int i = 0; i < (int)EDebugPrimitiveType.Count; i++)
			{
				m_persistentDrawDataPerPrimitive[i].Clear();
				m_remainingTimesPerPrimitive[i].Clear();
				m_frameDrawDataPerPrimitive[i].Clear();
			}
		}
		
		private readonly List<PrimitivePerInstanceData>[] m_persistentDrawDataPerPrimitive  = new List<PrimitivePerInstanceData>[(int)EDebugPrimitiveType.Count];
		private readonly List<float>[] m_remainingTimesPerPrimitive = new List<float>[(int)EDebugPrimitiveType.Count];
		private readonly List<PrimitivePerInstanceData>[] m_frameDrawDataPerPrimitive = new List<PrimitivePerInstanceData>[(int)EDebugPrimitiveType.Count];
		private readonly FieldInfo m_listItemsFieldInfo;
	}
}

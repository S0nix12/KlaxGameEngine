using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KlaxShared.Utilities;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace KlaxRenderer.Debug
{
	[StructLayout(LayoutKind.Sequential)]
	struct LineData
	{
		public Vector3 pos1;
		public Vector4 col1;
		public Vector3 pos2;
		public Vector4 col2;

		public static LineData CreateLine(Vector3 pos1, Vector3 pos2, Color4 col)
		{
			LineData data = new LineData()
			{
				pos1 = pos1,
				pos2 = pos2,
				col1 = col,
				col2 = col
			};
			return data;
		}
		public static LineData CreateLine(Vector3 pos1, Color4 col1, Vector3 pos2, Color4 col2)
		{
			LineData data = new LineData()
			{
				pos1 = pos1,
				pos2 = pos2,
				col1 = col1,
				col2 = col2
			};
			return data;
		}
	}

	class CDebugLineCommandList : IDisposable
	{
		public const int MAX_NUM_LINES = 1000000;

		public void Init(Device device)
		{
			m_vertexBuffer = new Buffer(device, MAX_NUM_LINES * Utilities.SizeOf<LineData>(), ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
		}

		public void Update(float deltaTime)
		{
			for (int i = m_lineRemainingTime.Count - 1; i >= 0; --i)
			{
				float timeRemaining = m_lineRemainingTime[i] - deltaTime;
				m_lineRemainingTime[i] = timeRemaining;
				if (timeRemaining <= 0.0f)
				{
					ContainerUtilities.RemoveSwapAt(m_lineRemainingTime, i);
					ContainerUtilities.RemoveSwapAt(m_persistentLineData, i);
				}
			}
		}

		public void Draw(DeviceContext deviceContext)
		{
			// Update VertexBuffer with line data
			int persistentLineCount = m_persistentLineData.Count;
			int frameLineCount = m_frameLineData.Count;
			int totalLineCount = frameLineCount + persistentLineCount;
			if (totalLineCount > 0)
			{
				deviceContext.MapSubresource(m_vertexBuffer, MapMode.WriteDiscard, MapFlags.None, out DataStream dataStream);

				if (persistentLineCount > 0)
				{
					LineData[] lineData = (LineData[])m_listDataFieldInfo.GetValue(m_persistentLineData);
					dataStream.WriteRange(lineData, 0, persistentLineCount);
				}

				if (frameLineCount > 0)
				{
					LineData[] lineData = (LineData[])m_listDataFieldInfo.GetValue(m_frameLineData);
					dataStream.WriteRange(lineData, 0, frameLineCount);
				}

				deviceContext.UnmapSubresource(m_vertexBuffer, 0);

				deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertexBuffer, Utilities.SizeOf<LineData>() / 2, 0));
				deviceContext.Draw(totalLineCount * 2, 0);
			}

			m_frameLineData.Clear();
		}

		public void Dispose()
		{
			m_vertexBuffer.Dispose();
		}

		public void AddLine(float timeToDraw, in LineData lineData)
		{			
			System.Diagnostics.Debug.Assert(m_frameLineData.Count + m_persistentLineData.Count < MAX_NUM_LINES, "Tried to add more than " + MAX_NUM_LINES + " to the debug drawing");

			if (timeToDraw > 0.0f)
			{
				m_lineRemainingTime.Add(timeToDraw);
				m_persistentLineData.Add(lineData);
			}
			else
			{
				m_frameLineData.Add(lineData);
			}
		}

		public bool HasAnyCommands()
		{
			return m_persistentLineData.Count + m_frameLineData.Count > 0;
		}

		private Buffer m_vertexBuffer;

		private readonly List<float> m_lineRemainingTime = new List<float>();
		private readonly List<LineData> m_persistentLineData = new List<LineData>();
		private readonly List<LineData> m_frameLineData = new List<LineData>();
		private readonly FieldInfo m_listDataFieldInfo = typeof(List<LineData>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
	}
}

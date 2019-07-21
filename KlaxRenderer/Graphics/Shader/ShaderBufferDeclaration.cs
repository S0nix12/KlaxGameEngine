using System.Collections.Generic;
using Buffer = SharpDX.Direct3D11.Buffer;
using KlaxShared;
using KlaxShared.Definitions.Graphics;

namespace KlaxRenderer.Graphics
{
	struct SShaderParameterTarget
	{
		public SHashedName parameterName;
		public EShaderParameterType parameterType;
		public int offset;
	}

	struct SShaderTextureTarget
	{
		public SHashedName parameterName;
		public int targetSlot;
		public EShaderTargetStage targetStage;
	}

	class CShaderBufferDeclaration
	{
		public void BeginArray()
		{
			m_currentOffset += m_currentRegisterSizeLeft;
			m_currentRegisterSizeLeft = 0;
			m_bInArray = true;
		}

		public void ArrayParameterEnd()
		{
			m_currentOffset += m_currentRegisterSizeLeft;
			m_currentRegisterSizeLeft = 0;
		}

		public void EndArray()
		{
			m_bInArray = false;
		}

		public void AddParameterTarget(SHashedName name, EShaderParameterType type)
		{
			SShaderParameterTarget newParameterTarget;
			newParameterTarget.parameterName = name;
			newParameterTarget.parameterType = type;

			int sizeInBytes = ShaderHelpers.GetSizeFromParameterType(type);
			if (m_currentRegisterSizeLeft >= sizeInBytes && !m_bInArray)
			{
				m_currentRegisterSizeLeft -= sizeInBytes;
				newParameterTarget.offset = m_currentOffset;
			}
			else
			{
				m_currentOffset += m_currentRegisterSizeLeft;
				newParameterTarget.offset = m_currentOffset;
				m_currentRegisterSizeLeft = (16 - sizeInBytes % 16) % 16;
			}
			m_currentOffset += sizeInBytes;

			parameterTargets.Add(newParameterTarget);
		}

		public List<SShaderParameterTarget> parameterTargets = new List<SShaderParameterTarget>();
		public Buffer targetBuffer = null;
		public int targetSlot = 0;
		public EShaderTargetStage targetStage = EShaderTargetStage.Vertex;
		private int m_currentOffset = 0;
		private int m_currentRegisterSizeLeft = 0;
		private bool m_bInArray = false;
	}
}

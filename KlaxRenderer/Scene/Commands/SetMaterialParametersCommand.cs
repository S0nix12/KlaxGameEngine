using System.Collections.Generic;
using KlaxIO.AssetManager.Assets;
using KlaxRenderer.RenderNodes;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Scene.Commands
{
    public class CSetMaterialParametersCommand : IRenderSceneCommand
    {
        public CSetMaterialParametersCommand(CMeshRenderNode targetNode, List<SMaterialParameterEntry> parameters)
        {
            m_targetNode = targetNode;
            m_parameters = parameters.ToArray();
        }

        public CSetMaterialParametersCommand(CMeshRenderNode targetNode, SMaterialParameterEntry[] parameters)
        {
            m_targetNode = targetNode;
            m_parameters = parameters;
        }

        public bool TryExecute(Device device, DeviceContext deviceContext, CRenderScene scene)
        {
	        return true;
        }

        private readonly CMeshRenderNode m_targetNode;
        private readonly SMaterialParameterEntry[] m_parameters;
    }
}

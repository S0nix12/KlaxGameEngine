using KlaxConfig;
using KlaxShared.Attributes;
using System;
using System.Reflection;

namespace KlaxRenderer
{
    public class CRendererCVars : IDisposable
    {
        public CRendererCVars(CRenderer renderer)
        {
            m_renderer = renderer;
        }

        public void InitConsole()
        {
            CConfigManager manager = CConfigManager.Instance;
            if (manager != null)
            {
                manager.RegisterInstance(this);
            }
        }

        public void Dispose()
        {
            CConfigManager manager = CConfigManager.Instance;

            if (manager != null)
            {
                manager.UnregisterInstance(this);
            }
        }

        public void OnWireframeChanged(int oldValue, int newValue)
        {
            m_renderer.Dispatch(ERendererDispatcherPriority.BeginFrame, () => m_renderer.SetWireframeEnabled(newValue > 0));
        }

		public void OnMSAASampleCountChanged(int oldValue, int newValue)
		{
			if (newValue != oldValue)
			{
				m_renderer.Dispatch(ERendererDispatcherPriority.BeginFrame, () => m_renderer.SetMSAASampleCount(newValue));
			}
		}

        [CVar(Callback = "OnWireframeChanged")]
        public int Wireframe { get; private set; }

		[CVar(Callback = "OnMSAASampleCountChanged")]
		public int MSAASampleCount { get; private set; } = 4;

        private readonly CRenderer m_renderer;
    }
}

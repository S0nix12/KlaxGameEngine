using System;
using System.Threading;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Assets;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Graphics.ResourceManagement
{
	public abstract class CResource : IDisposable
	{
		internal abstract void InitFromAsset(Device device, CAsset asset);
		internal abstract void InitWithContext(Device device, DeviceContext deviceContext);

		internal abstract bool IsAssetCorrectType(CAsset asset);

		public abstract void Dispose();

		internal virtual bool NeedsContext()
		{
			return false;
		}

		public void WaitUntilLoaded()
		{
			while (!IsLoaded)
			{
				Thread.Sleep(1);
			}
		}

		internal void FinishLoading()
		{
			IsLoaded = true;
		}

		public bool IsLoaded { get; private set; }
		public Guid Guid { get; internal set; }
	}
}

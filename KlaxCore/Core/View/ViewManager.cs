using System.Collections.Generic;
using KlaxRenderer.Scene;
using SharpDX;

namespace KlaxCore.Core.View
{
	public interface IViewProvider
	{
		Matrix GetViewMatrix();
		Matrix GetProjectionMatrix();
		Vector3 GetViewLocation();
		float GetScreenNear();
		float GetScreenFar();
		float GetFov();
		int GetPriority();
	}

	public class CViewManager
	{
		public void ResizeView(float newWidth, float newHeight, float newLeft, float newTop)
		{
			ScreenTop = newTop;
			ScreenLeft = newLeft;
			ScreenWidth = newWidth;
			ScreenHeight = newHeight;
		}

		public void RegisterViewProvider(IViewProvider viewProvider)
		{
			m_viewProviders.Add(viewProvider);
		}

		public void UnregisterViewProvider(IViewProvider viewProvider)
		{
			m_viewProviders.Remove(viewProvider);
		}

		public void GetViewInfo(out SSceneViewInfo viewInfo)
		{
			viewInfo = new SSceneViewInfo {ScreenTop = ScreenTop, ScreenLeft = ScreenLeft, ScreenWidth = ScreenWidth, ScreenHeight = ScreenHeight};

			if (m_viewProviders.Count > 0)
			{
				IViewProvider activeViewProvider = m_viewProviders[0];
				for (int i = 1; i < m_viewProviders.Count; i++)
				{
					if (activeViewProvider.GetPriority() <= 0)
					{
						break;
					}

					if (m_viewProviders[i].GetPriority() < activeViewProvider.GetPriority())
					{
						activeViewProvider = m_viewProviders[i];
					}
				}

				viewInfo.ViewLocation = activeViewProvider.GetViewLocation();
				viewInfo.ViewMatrix = activeViewProvider.GetViewMatrix();
				viewInfo.ProjectionMatrix = activeViewProvider.GetProjectionMatrix();
				viewInfo.ScreenNear = activeViewProvider.GetScreenNear();
				viewInfo.ScreenFar = activeViewProvider.GetScreenFar();
				viewInfo.Fov = activeViewProvider.GetFov();
				viewInfo.FitProjectionToScene = false;
			}
			else
			{
				viewInfo.ViewLocation = Vector3.Zero;
				viewInfo.ViewMatrix = Matrix.Identity;
				viewInfo.ProjectionMatrix = Matrix.Identity;
				viewInfo.ScreenNear = 0.2f;
				viewInfo.ScreenFar = 100000.0f;
				viewInfo.Fov = MathUtil.PiOverFour;
				viewInfo.FitProjectionToScene = false;
			}

			viewInfo.CreateBoundingFrustum();
		}

		public float ScreenTop { get; protected set; }
		public float ScreenLeft { get; protected set; }
		public float ScreenWidth { get; protected set; }
		public float ScreenHeight { get; protected set; }

		private readonly List<IViewProvider> m_viewProviders = new List<IViewProvider>();
	}
}

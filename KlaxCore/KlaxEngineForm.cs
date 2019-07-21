using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KlaxCore.Core;
using KlaxCore.KlaxScript;
using KlaxIO.Input;
using KlaxIO.Log;
using KlaxRenderer;
using KlaxRenderer.Graphics;
using KlaxShared;
using KlaxShared.Attributes;
using SharpDX.Windows;

namespace KlaxCore
{
	[System.ComponentModel.DesignerCategory("")]
	class KlaxEngineForm : RenderForm
	{
		[CVar]
		public static int WindowWidth { get; set; }
		[CVar]
		public static int WindowHeight { get; set; }

		private const int WM_CHAR = 0x0102;

		protected override void OnClosed(EventArgs e)
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () => CEngine.Instance.Shutdown());
			base.OnClosed(e);
		}

		protected override void OnResizeEnd(EventArgs e)
		{
			if (engineWorld != null)
			{
				System.Drawing.Point topLeft = PointToScreen(new System.Drawing.Point(0, 0));
				CRenderer renderer = CRenderer.Instance;
				IntPtr handlePtr = Handle;
				renderer.Dispatch(ERendererDispatcherPriority.BeginFrame, () => renderer.Resize(ClientSize.Width, ClientSize.Height, topLeft.X, topLeft.Y, handlePtr));
				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () => { engineWorld.ViewManager.ResizeView(ClientSize.Width, ClientSize.Height, topLeft.X, topLeft.Y);});
			}
			base.OnResizeEnd(e);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_CHAR)
			{
				Input.ProcessCharacterInput((char) m.WParam);
			}
			else
			{
				base.WndProc(ref m);
			}
		}

		private void ShowWorld(CWorld world)
		{
			engineWorld = world;

			System.Drawing.Point topLeft = PointToScreen(new System.Drawing.Point(0, 0));
			CRenderer renderer = CRenderer.Instance;
			IntPtr handlePtr = Handle;
			renderer.Dispatch(ERendererDispatcherPriority.BeginFrame, () => renderer.Resize(ClientSize.Width, ClientSize.Height, topLeft.X, topLeft.Y, handlePtr));
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () => { engineWorld.ViewManager.ResizeView(ClientSize.Width, ClientSize.Height, topLeft.X, topLeft.Y); });

			Show();
		}

		internal void CreateEngineAndShow()
        {
            RenderFormSurface surface = new RenderFormSurface(this);
			CLogger logger = new CLogger("game.log", true, true, true);

			CInitializer initializer = new CInitializer();
            initializer.Add("Viewport");
            initializer.Add<IRenderSurface>(surface);
			initializer.Add(logger);
			CEngine.Create(initializer, true);

			CKlaxScriptRegistry dummy = CKlaxScriptRegistry.Instance;

			Input.SetReferenceHWND(Handle);
			Input.CursorVisibilitySetter = (arg) =>
			{
				if (arg)
					Cursor.Show();
				else
					Cursor.Hide();
			};

            Width = WindowWidth;
            Height = WindowHeight;

            CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () => { CEngine.Instance.LoadWorld(null, WorldLoadedCallback);});

			Point size = new Point(ClientSize.Width, ClientSize.Height);
            System.Drawing.Point topLeft = PointToScreen(new System.Drawing.Point(0, 0));
			IntPtr handlePtr = Handle;
            CRenderer renderer = CRenderer.Instance;
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				renderer.Resize(size.X, size.Y, topLeft.X, topLeft.Y, handlePtr);
				CEngine.Instance.CurrentWorld.StartPlayMode();
			});
        }

		private void WorldLoadedCallback(CWorld world)
		{
			if (InvokeRequired)
			{			
				Invoke((Action) delegate { this.ShowWorld(world); });
			}
			else
			{
				ShowWorld(world);
			}
		}

		private CWorld engineWorld;
	}
}

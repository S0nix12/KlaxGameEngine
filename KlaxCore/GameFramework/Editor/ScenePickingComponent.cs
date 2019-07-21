using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using KlaxCore.Core.View;
using KlaxIO.Input;
using KlaxMath.Geometry;
using KlaxRenderer;
using KlaxRenderer.RenderNodes;
using KlaxRenderer.Scene;
using KlaxShared.Attributes;
using SharpDX;

namespace KlaxCore.GameFramework.Editor
{
	[KlaxComponent(HideInEditor = true, Category = "Editor")]
	public class CScenePickingComponent : CEntityComponent
	{
		[CVar]
		private static int MarkPickedObjects { get; set; } = 0;

		public CScenePickingComponent()
		{
			ShowInInspector = false;
		}

		public override void Init()
		{
			base.Init();
			Input.RegisterListener(OnInputEvent);
			m_gizmo = World.CreateObject<CTransformGizmo>(null);
		}

		public override void Shutdown()
		{
			base.Shutdown();
			Input.UnregisterListener(OnInputEvent);
			m_gizmo.Destroy();
			m_gizmo = null;
		}

		private void OnInputEvent(ReadOnlyCollection<SInputButtonEvent> buttonevents, string textinput)
		{
			if (World.IsPlaying)
				return;

			foreach (var buttonEvent in buttonevents)
			{
				if (buttonEvent.button == EInputButton.MouseLeftButton && buttonEvent.buttonEvent == EButtonEvent.Pressed)
				{
					if (ImGui.GetIO().WantCaptureMouse)
					{
						return;
					}
					CViewManager viewManager = World.ViewManager;
					int mouseAbsX = System.Windows.Forms.Cursor.Position.X - (int)viewManager.ScreenLeft;
					int mouseAbsY = System.Windows.Forms.Cursor.Position.Y - (int)viewManager.ScreenTop;

					if (mouseAbsX < 0 || mouseAbsY < 0 || mouseAbsX > viewManager.ScreenWidth || mouseAbsY > viewManager.ScreenHeight)
					{
						return;
					}

					viewManager.GetViewInfo(out SSceneViewInfo viewInfo);

					if (m_gizmo.IsHovered)
					{
						return;
					}

					Ray pickRay = Ray.GetPickRay(mouseAbsX, mouseAbsY, new ViewportF(0, 0, viewManager.ScreenWidth, viewManager.ScreenHeight), viewInfo.ViewMatrix * viewInfo.ProjectionMatrix);
					if (CRenderer.Instance.ActiveScene.RayIntersection(pickRay, out CRenderNode node, out STriangle tri, out float dist, MarkPickedObjects > 0))
					{
						if (node.Outer is CSceneComponent sceneComponent)
						{
							CSceneComponent rootComponent = sceneComponent.Owner.RootComponent;

							m_gizmo.IsActive = true;
							m_gizmo.SetControlledTransform(rootComponent.Transform);

                            OnComponentPicked?.Invoke(rootComponent);
                        }
					}
					else
					{
						m_gizmo.IsActive = false;
						OnComponentPicked?.Invoke(null);
					}
				}
				else if(buttonEvent.button == EInputButton.MouseLeftButton)
				{
					m_gizmo.Deselect();
				}
			}
		}

        public void Pick(CSceneComponent component)
        {
			if (!Owner.IsAlive)
			{
				return;
			}

            if (component != null)
            {
                m_gizmo.IsActive = true;
                m_gizmo.SetControlledTransform(component.Transform);
            }
            else
            {
                m_gizmo.IsActive = false;
                m_gizmo.SetControlledTransform(null);
            }
        }

		private CTransformGizmo m_gizmo;

        public static Action<CSceneComponent> OnComponentPicked;
	}
}

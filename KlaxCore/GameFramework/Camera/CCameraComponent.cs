using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.Core.View;
using KlaxShared.Attributes;
using SharpDX;

namespace KlaxCore.GameFramework.Camera
{
	[KlaxComponent(Category = "Common")]
	class CCameraComponent : CSceneComponent, IViewProvider
	{
		public override void Init()
		{
			base.Init();

			if (RegisterDuringInit)
			{
				World.ViewManager.RegisterViewProvider(this);
			}
		}

		public override void Start()
		{
			base.Start();

			if (!RegisterDuringInit)
			{
				World.ViewManager.RegisterViewProvider(this);
			}
		}

		public override void Shutdown()
		{
			base.Shutdown();
			World?.ViewManager.UnregisterViewProvider(this);
		}

		public Matrix GetViewMatrix()
		{
			return Matrix.Invert(m_transform.WorldMatrix);
		}

		public Matrix GetProjectionMatrix()
		{
			float screenAspect = World.ViewManager.ScreenWidth / World.ViewManager.ScreenHeight;
			if (IsPerspective)
			{
				return Matrix.PerspectiveFovLH(FieldOfView, screenAspect, ScreenNear, ScreenFar);
			}
			else
			{
				return Matrix.OrthoLH(OrthoSize * screenAspect, OrthoSize, ScreenNear, ScreenFar);
			}
		}

		public Vector3 GetViewLocation()
		{
			return m_transform.WorldPosition;
		}

		public float GetScreenNear()
		{
			return ScreenNear;
		}

		public float GetScreenFar()
		{
			return ScreenFar;
		}

		public float GetFov()
		{
			return FieldOfView;
		}

		public int GetPriority()
		{
			return ViewPriority;
		}

		[KlaxProperty(Category = "Camera")]
		public float FieldOfView { get; set; } = MathUtil.PiOverFour;
		[KlaxProperty(Category = "Camera")]
		public float ScreenNear { get; set; } = 0.2f;
		[KlaxProperty(Category = "Camera")]
		public float ScreenFar { get; set; } = 10000.0f;
		[KlaxProperty(Category = "Camera")]
		public float OrthoSize { get; set; } = 20.0f;
		[KlaxProperty(Category = "Camera")]
		public bool IsPerspective { get; set; } = true;
		[KlaxProperty(Category = "Camera")]
		public int ViewPriority { get; set; } = 10;
		[KlaxProperty(Category = "Camera")]
		public bool RegisterDuringInit = false;
	}
}

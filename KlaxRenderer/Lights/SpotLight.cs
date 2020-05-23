using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxRenderer.Graphics.Texture;
using KlaxRenderer.Scene;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace KlaxRenderer.Lights
{
	public class CSpotLight : CPositionalLight
	{
		public override void FillShaderData(ref SPositionalLightShaderData data)
		{
			data.Position = new Vector4(Transform.WorldPosition, 1.0f);
			data.Direction = new Vector4(Transform.Forward, 1.0f);
			data.Color = LightColor;
			data.Range = Range;
			data.SpotAngle = SpotAngle;
			data.ConstantAttenuation = ConstantAttenuation;
			data.LinearAttenuation = LinearAttenuation;
			data.QuadraticAttenuation = QuadraticAttenuation;
			data.LightType = LightType;
			data.bEnabled = Enabled ? 1 : 0;
			data.bCastShadows = IsCastingShadows ? 1 : 0;
			data.shadowMapRegister = ShadowMapRegister;
			data.viewProjection = Matrix.Transpose(Matrix.Multiply(Matrix.Invert(Transform.WorldMatrix), m_projectionMatrix));
		}

		public override ELightType GetLightType()
		{
			return ELightType.Spot;
		}

		public override void InitializeShadowMaps(Device device)
		{
			ShadowMapTexture = new CDepthTexture();

			Texture2DDescription textureDesc = new Texture2DDescription()
			{
				ArraySize = 1,
				BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				Width = CStaticRendererCvars.ShadowMapSize,
				Height = CStaticRendererCvars.ShadowMapSize,
				Format = Format.R32_Typeless,
				MipLevels = 1,
				OptionFlags = ResourceOptionFlags.None,
				Usage = ResourceUsage.Default
			};
			textureDesc.SampleDescription.Count = 1;

			ShadowMapTexture.InitEmpty(device, in textureDesc);
			m_isShadowMapIntialized = true;
		}

		public override void GenerateShadowMaps(Device device, DeviceContext deviceContext, CRenderScene renderScene)
		{
			UserDefinedAnnotation annotation = deviceContext.QueryInterface<UserDefinedAnnotation>();
			annotation.BeginEvent("SpotLightShadowMap");

			deviceContext.Rasterizer.SetViewport(0.0f, 0.0f, CStaticRendererCvars.ShadowMapSize, CStaticRendererCvars.ShadowMapSize);

			deviceContext.ClearDepthStencilView(ShadowMapTexture.GetRenderTarget(), DepthStencilClearFlags.Depth, 1.0f, 0);
			deviceContext.OutputMerger.SetRenderTargets(ShadowMapTexture.GetRenderTarget());

			DepthStencilStateDescription depthStateDesc = new DepthStencilStateDescription()
			{
				IsDepthEnabled = true,
				DepthWriteMask = DepthWriteMask.All,
				DepthComparison = Comparison.Less,

				IsStencilEnabled = false,
				StencilReadMask = 0xFF,
				StencilWriteMask = 0xFF,
			};

			DepthStencilState depthState = new DepthStencilState(device, depthStateDesc);
			deviceContext.OutputMerger.SetDepthStencilState(depthState);

			SSceneViewInfo viewInfo = new SSceneViewInfo()
			{
				FitProjectionToScene = false,
				Fov = MathUtil.Pi / 2.0f,
				ScreenFar = Range,
				ScreenNear = 0.1f,
				ScreenHeight = CStaticRendererCvars.ShadowMapSize,
				ScreenWidth = CStaticRendererCvars.ShadowMapSize,
				ScreenLeft = 0.0f,
				ScreenTop = 0.0f,
				ViewLocation = Transform.WorldPosition,
				ViewMatrix = Matrix.Invert(Transform.WorldMatrix)
			};

			m_projectionMatrix = Matrix.PerspectiveFovLH(viewInfo.Fov, 1.0f, viewInfo.ScreenNear, viewInfo.ScreenFar);
			viewInfo.ProjectionMatrix = m_projectionMatrix;
			viewInfo.CreateBoundingFrustum();

			depthState.Dispose();
			renderScene.RenderSceneDepth(device, deviceContext, in viewInfo);

			DepthStencilView nullDepthView = null;
			deviceContext.OutputMerger.SetRenderTargets(nullDepthView);

			annotation.EndEvent();
			annotation.Dispose();
		}

		public override void Dispose()
		{
			ShadowMapTexture.Dispose();
			m_isShadowMapIntialized = false;
		}

		public override ShaderResourceView GetShadowMapView()
		{
			return ShadowMapTexture.GetTexture();
		}

		public override bool IsShadowMapCube()
		{
			return false;
		}

		public const int LightType = 2;

		public Vector4 LightColor { get; set; }
		public float Range { get; set; }
		public float SpotAngle { get; set; }
		public float ConstantAttenuation { get; set; }
		public float LinearAttenuation { get; set; }
		public float QuadraticAttenuation { get; set; }
		public bool Enabled { get; set; } = true;

		internal CDepthTexture ShadowMapTexture { get; private set; }

		private Matrix m_projectionMatrix;
	}
}

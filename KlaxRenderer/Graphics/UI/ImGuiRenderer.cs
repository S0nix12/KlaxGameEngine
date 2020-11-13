using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ImGuiNET;
using KlaxIO.Input;
using KlaxShared;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using IntPtr = System.IntPtr;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using KlaxShared.Definitions.Graphics;

namespace KlaxRenderer.Graphics.UI
{
	class ImGuiRenderer : IDisposable
	{
		public void Init(Device device, DeviceContext deviceContext, int screenWidth, int screenHeight, int screenLeft, int screenTop)
		{
			m_contextPtr = ImGui.CreateContext();
			ImGui.SetCurrentContext(m_contextPtr);
			m_d3Device = device;
			m_d3DeviceContext = deviceContext;

			m_imguiIO = ImGui.GetIO();
			m_screenWidth = screenWidth;
			m_screenHeight = screenHeight;
			m_screenLeft = screenLeft;
			m_screenTop = screenTop;
			m_imguiIO.DisplaySize = new System.Numerics.Vector2(m_screenWidth, m_screenHeight);
			
			FontProvider = new CFontProvider(m_imguiIO.Fonts, CreateFontsTexture);

			CreateFontsTexture();
			SetupInput();

			m_mouseAbsX = System.Windows.Forms.Cursor.Position.X;
			m_mouseAbsY = System.Windows.Forms.Cursor.Position.Y;

			Input.RegisterListener(ProcessInputEvent);
			CreateDeviceResources();
		}

		public IntPtr CreateAdditionalContext()
		{
			IntPtr newContext = ImGui.CreateContext(m_imguiIO.Fonts);
			SetContext(newContext);
			SetupInput();
			return newContext;
		}

		public void SetContext(IntPtr contextPtr)
		{
			m_contextPtr = contextPtr;
			ImGui.SetCurrentContext(m_contextPtr);
			m_imguiIO = ImGui.GetIO();
		}

		public void Resize(int screenWidth, int screenHeight, int screenLeft, int screenTop)
		{
			m_screenWidth = screenWidth;
			m_screenHeight = screenHeight;
			m_screenLeft = screenLeft;
			m_screenTop = screenTop;
			m_imguiIO.DisplaySize = new System.Numerics.Vector2(m_screenWidth, m_screenHeight);
		}

		public void BeginRender(float elapsedTime)
		{
			m_imguiIO.DeltaTime = elapsedTime;


			UpdateInput();
			ImGui.NewFrame();
		}

		public void EndRender()
		{
			ImGui.Render();

			unsafe
			{
				ImDrawDataPtr drawData = ImGui.GetDrawData();
				if (drawData.CmdListsCount <= 0)
				{
					return;
				}

				UserDefinedAnnotation annotation = m_d3DeviceContext.QueryInterface<UserDefinedAnnotation>();
				annotation.BeginEvent("ImguiPass");

				// Grow buffers in case they are to small
				if (m_vertexBufferSize < drawData.TotalVtxCount || m_indexBufferSize < drawData.TotalIdxCount)
				{
					m_indexBufferSize = (int) (drawData.TotalIdxCount * 1.5f);
					m_vertexBufferSize = (int) (drawData.TotalVtxCount * 1.5f);

					CreateVertexIndexBuffer();
				}

				m_d3DeviceContext.MapSubresource(m_vertexBuffer, MapMode.WriteDiscard, MapFlags.None, out DataStream vertexStream);
				m_d3DeviceContext.MapSubresource(m_indexBuffer, MapMode.WriteDiscard, MapFlags.None, out DataStream indexStream);
				for (int i = 0; i < drawData.CmdListsCount; i++)
				{
					ImDrawListPtr drawList = drawData.CmdListsRange[i];
					IntPtr vertexPtr = drawList.VtxBuffer.Data;
					vertexStream.Write(vertexPtr, 0, drawList.VtxBuffer.Size * Utilities.SizeOf<ImDrawVert>());

					IntPtr indexPtr = drawList.IdxBuffer.Data;
					indexStream.Write(indexPtr, 0,  drawList.IdxBuffer.Size * Utilities.SizeOf<ushort>());
				}
				m_d3DeviceContext.UnmapSubresource(m_vertexBuffer, 0);
				m_d3DeviceContext.UnmapSubresource(m_indexBuffer, 0);

				float offset = 0.0f;
				
				Matrix mvpMatrix = Matrix.OrthoOffCenterLH(offset, m_imguiIO.DisplaySize.X + offset, m_imguiIO.DisplaySize.Y + offset, offset, -1.0f, 1.0f);
				mvpMatrix.Transpose();

				SShaderParameter parameter;
				parameter.parameterType = EShaderParameterType.Matrix;
				parameter.parameterData = mvpMatrix;
				m_shaderParameters[new SHashedName("mvpMatrix")] = parameter;

				SShaderParameter fontTextureParams;
				fontTextureParams.parameterType = EShaderParameterType.Texture;
				fontTextureParams.parameterData = m_boundTextures[m_fontAtlasId];
				m_shaderParameters[new SHashedName("texture")] = fontTextureParams;

				m_shader.SetShaderParameters(m_d3DeviceContext, m_shaderParameters);				
				m_shader.SetActive(m_d3DeviceContext);
				
				m_d3DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertexBuffer, Utilities.SizeOf<ImDrawVert>(), 0));
				m_d3DeviceContext.InputAssembler.SetIndexBuffer(m_indexBuffer, Format.R16_UInt, 0);
				m_d3DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

				RawViewportF[] prevViewports = m_d3DeviceContext.Rasterizer.GetViewports<RawViewportF>();
				RasterizerState prevRasterizerState = m_d3DeviceContext.Rasterizer.State;
				DepthStencilState prevDepthState = m_d3DeviceContext.OutputMerger.DepthStencilState;
				BlendState prevBlendState = m_d3DeviceContext.OutputMerger.BlendState;
				Rectangle[] prevScissorRects = new Rectangle[8];
				m_d3DeviceContext.Rasterizer.GetScissorRectangles(prevScissorRects);


				Viewport viewport = new Viewport(0, 0, m_screenWidth, m_screenHeight, 0.0f, 1.0f);
				m_d3DeviceContext.Rasterizer.SetViewport(viewport);
				m_d3DeviceContext.OutputMerger.DepthStencilState = m_depthStencilState;
				m_d3DeviceContext.OutputMerger.BlendState = m_blendState;
				m_d3DeviceContext.Rasterizer.State = m_rasterizerState;

				int vertexOffset = 0;
				int indexOffset = 0;
				
				for (int i = 0; i < drawData.CmdListsCount; i++)
				{					
					ImDrawListPtr drawList = drawData.CmdListsRange[i];
					for (int n = 0; n < drawList.CmdBuffer.Size; n++)
					{
						ImDrawCmdPtr command = drawList.CmdBuffer[n];
						if (!m_boundTextures.ContainsKey(command.TextureId))
						{
							throw new InvalidOperationException("Requested texture was not bound to imgui, please check your binding");
						}
						
						CTextureSampler textureSampler = m_boundTextures[command.TextureId];					

						m_d3DeviceContext.Rasterizer.SetScissorRectangle((int)command.ClipRect.X, (int)command.ClipRect.Y, (int)command.ClipRect.Z, (int)command.ClipRect.W);
						m_d3DeviceContext.PixelShader.SetSampler(0, textureSampler.SamplerState);
						m_d3DeviceContext.PixelShader.SetShaderResource(0, textureSampler.Texture.GetTexture());

						m_d3DeviceContext.DrawIndexed((int)command.ElemCount, indexOffset, vertexOffset);
						indexOffset += (int)command.ElemCount;
					}

					vertexOffset += drawList.VtxBuffer.Size;
				}
				
				m_d3DeviceContext.Rasterizer.SetScissorRectangles(prevScissorRects);
				m_d3DeviceContext.OutputMerger.BlendState = prevBlendState;
				m_d3DeviceContext.OutputMerger.DepthStencilState = prevDepthState;
				m_d3DeviceContext.Rasterizer.State = prevRasterizerState;
				m_d3DeviceContext.Rasterizer.SetViewports(prevViewports);

				prevRasterizerState?.Dispose();
				prevBlendState?.Dispose();
				prevDepthState?.Dispose();

				annotation.EndEvent();
				annotation.Dispose();				
			}
		}

		public void EndFrame()
		{
			m_textInputBuffer = "";
		}
		public void Dispose()
		{
			Input.UnregisterListener(ProcessInputEvent);
			Utilities.Dispose(ref m_d3Device);
			Utilities.Dispose(ref m_d3DeviceContext);
			Utilities.Dispose(ref m_vertexBuffer);
			Utilities.Dispose(ref m_indexBuffer);
			Utilities.Dispose(ref m_rasterizerState);
			Utilities.Dispose(ref m_blendState);
			Utilities.Dispose(ref m_depthStencilState);
			m_shader.Dispose();

			foreach (var textureSampler in m_boundTextures)
			{
				textureSampler.Value.Dispose();
			}

			ImGui.DestroyContext(m_contextPtr);
		}

		private void SetupInput()
		{
			m_imguiIO.KeyMap[(int)ImGuiKey.Tab] = (int) EInputButton.Tab;
			m_registredButtons.Add(EInputButton.Tab);
			m_imguiIO.KeyMap[(int)ImGuiKey.LeftArrow] = (int) EInputButton.Left;
			m_registredButtons.Add(EInputButton.Left);
			m_imguiIO.KeyMap[(int)ImGuiKey.RightArrow] = (int) EInputButton.Right;
			m_registredButtons.Add(EInputButton.Right);
			m_imguiIO.KeyMap[(int)ImGuiKey.UpArrow] = (int) EInputButton.Up;
			m_registredButtons.Add(EInputButton.Up);
			m_imguiIO.KeyMap[(int)ImGuiKey.DownArrow] = (int) EInputButton.Down;
			m_registredButtons.Add(EInputButton.Down);
			m_imguiIO.KeyMap[(int)ImGuiKey.PageUp] = (int) EInputButton.PageUp;
			m_registredButtons.Add(EInputButton.PageUp);
			m_imguiIO.KeyMap[(int)ImGuiKey.PageDown] = (int) EInputButton.PageDown;
			m_registredButtons.Add(EInputButton.PageDown);
			m_imguiIO.KeyMap[(int)ImGuiKey.Home] = (int)EInputButton.Home;
			m_registredButtons.Add(EInputButton.Home);
			m_imguiIO.KeyMap[(int)ImGuiKey.End] = (int)EInputButton.End;
			m_registredButtons.Add(EInputButton.End);
			m_imguiIO.KeyMap[(int)ImGuiKey.Delete] = (int)EInputButton.Delete;
			m_registredButtons.Add(EInputButton.Delete);
			m_imguiIO.KeyMap[(int)ImGuiKey.Backspace] = (int)EInputButton.Back;
			m_registredButtons.Add(EInputButton.Back);
			m_imguiIO.KeyMap[(int)ImGuiKey.Enter] = (int)EInputButton.Return;
			m_registredButtons.Add(EInputButton.Return);
			m_imguiIO.KeyMap[(int)ImGuiKey.Escape] = (int)EInputButton.Escape;
			m_registredButtons.Add(EInputButton.Escape);
			m_imguiIO.KeyMap[(int)ImGuiKey.A] = (int)EInputButton.A;
			m_registredButtons.Add(EInputButton.A);
			m_imguiIO.KeyMap[(int)ImGuiKey.C] = (int)EInputButton.C;
			m_registredButtons.Add(EInputButton.C);
			m_imguiIO.KeyMap[(int)ImGuiKey.V] = (int)EInputButton.V;
			m_registredButtons.Add(EInputButton.V);
			m_imguiIO.KeyMap[(int)ImGuiKey.X] = (int)EInputButton.X;
			m_registredButtons.Add(EInputButton.X);
			m_imguiIO.KeyMap[(int)ImGuiKey.Y] = (int)EInputButton.Y;
			m_registredButtons.Add(EInputButton.Y);
			m_imguiIO.KeyMap[(int)ImGuiKey.Z] = (int)EInputButton.Z;
			m_registredButtons.Add(EInputButton.Z);
		}

		private void UpdateInput()
		{
			string inputText = "";
			lock (m_textInputMutex)
			{
				inputText = string.Copy(m_textInputBuffer);
			}

			m_imguiIO.AddInputCharactersUTF8(inputText);
			for (int i = 0; i < m_registredButtons.Count; ++i)
			{
				EInputButton button = m_registredButtons[i];
				m_imguiIO.KeysDown[(int) button] = Input.IsButtonPressed(button);
			}

			m_imguiIO.KeyShift = Input.IsButtonPressed(EInputButton.LeftShift) || Input.IsButtonPressed(EInputButton.RightShift);
			m_imguiIO.KeyCtrl = Input.IsButtonPressed(EInputButton.LeftControl) || Input.IsButtonPressed(EInputButton.RightControl);
			m_imguiIO.KeyAlt = Input.IsButtonPressed(EInputButton.LeftAlt) || Input.IsButtonPressed(EInputButton.RightAlt);
			m_imguiIO.KeySuper = Input.IsButtonPressed(EInputButton.LeftWindowsKey) || Input.IsButtonPressed(EInputButton.RightWindowsKey);
			m_mouseAbsX = System.Windows.Forms.Cursor.Position.X - m_screenLeft;
			m_mouseAbsY = System.Windows.Forms.Cursor.Position.Y - m_screenTop;

			m_imguiIO.MousePos = new System.Numerics.Vector2(m_mouseAbsX, m_mouseAbsY);

			m_imguiIO.MouseWheel = Input.GetNativeAxisValue(EInputAxis.MouseWheel) / 120.0f;
			m_imguiIO.MouseDown[0] = Input.IsButtonPressed(EInputButton.MouseLeftButton);
			m_imguiIO.MouseDown[1] = Input.IsButtonPressed(EInputButton.MouseRightButton);
			m_imguiIO.MouseDown[2] = Input.IsButtonPressed(EInputButton.MouseMiddleButton);
		}

		private unsafe void CreateFontsTexture()
		{
			byte* pixels;
			int width, height, bytesPerPixel;
			m_imguiIO.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height, out bytesPerPixel);
			
			Texture2DDescription texDescription = new Texture2DDescription()
			{
				Width = width,
				Height = height,
				MipLevels = 1,
				ArraySize = 1,
				BindFlags = BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				Format = Format.R8G8B8A8_UNorm,
				Usage = ResourceUsage.Default,
				SampleDescription = new SampleDescription(1, 0),
				OptionFlags = ResourceOptionFlags.None
			};

			IntPtr pixelsPointer = new IntPtr(pixels);
			CTexture texture = new CTexture();
			texture.InitFromData(m_d3Device, m_d3DeviceContext, pixelsPointer, width * bytesPerPixel, in texDescription, false);

			SamplerStateDescription samplerDesc = new SamplerStateDescription()
			{
				Filter = Filter.MinMagMipLinear,
				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
				MipLodBias = 0.0f,
				ComparisonFunction = Comparison.Always,
				MinimumLod = 0,
				MaximumLod = 0,
			};

			CTextureSampler textureSampler = new CTextureSampler(m_d3Device, m_d3DeviceContext, texture, in samplerDesc);

			m_fontAtlasId = BindTexture(textureSampler);
			m_imguiIO.Fonts.SetTexID(m_fontAtlasId);
			m_imguiIO.Fonts.ClearTexData();
		}

		private void CreateDeviceResources()
		{
			m_shader = new CUIShader();
			m_shader.Init(m_d3Device);

			CreateVertexIndexBuffer();
			CreateBlendState();
			CreateRasterizerState();
			CreateDepthStencilState();
		}

		private void CreateDepthStencilState()
		{
			DepthStencilStateDescription depthDescription = new DepthStencilStateDescription()
			{
				IsDepthEnabled = false,
				DepthWriteMask = DepthWriteMask.All,
				DepthComparison = Comparison.Always,
				IsStencilEnabled = false
			};
			depthDescription.FrontFace.FailOperation = StencilOperation.Keep;
			depthDescription.FrontFace.PassOperation = StencilOperation.Keep;
			depthDescription.FrontFace.DepthFailOperation = StencilOperation.Keep;
			depthDescription.BackFace = depthDescription.FrontFace;

			m_depthStencilState = new DepthStencilState(m_d3Device, depthDescription);
		}

		private void CreateRasterizerState()
		{
			RasterizerStateDescription rasterizerDescription = new RasterizerStateDescription()
			{
				FillMode = FillMode.Solid,
				CullMode = CullMode.None,
				IsScissorEnabled = true,
				IsDepthClipEnabled = true
			};

			m_rasterizerState = new RasterizerState(m_d3Device, rasterizerDescription);
		}

		private void CreateBlendState()
		{
			BlendStateDescription blendDescription = new BlendStateDescription();
			blendDescription.AlphaToCoverageEnable = false;
			blendDescription.RenderTarget[0].IsBlendEnabled = true;
			blendDescription.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
			blendDescription.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
			blendDescription.RenderTarget[0].BlendOperation = BlendOperation.Add;
			blendDescription.RenderTarget[0].SourceAlphaBlend = BlendOption.InverseSourceAlpha;
			blendDescription.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
			blendDescription.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
			blendDescription.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

			m_blendState = new BlendState(m_d3Device, blendDescription);
		}

		private void CreateVertexIndexBuffer()
		{
			m_vertexBuffer?.Dispose();
			m_indexBuffer?.Dispose();

			BufferDescription vertexBufferDescription = new BufferDescription()
			{
				Usage = ResourceUsage.Dynamic,
				SizeInBytes = Utilities.SizeOf<ImDrawVert>() * m_vertexBufferSize,
				BindFlags = BindFlags.VertexBuffer,
				CpuAccessFlags = CpuAccessFlags.Write
			};

			m_vertexBuffer = new Buffer(m_d3Device, vertexBufferDescription);

			BufferDescription indexBufferDescription = new BufferDescription(Utilities.SizeOf<ushort>() * m_indexBufferSize, ResourceUsage.Dynamic, BindFlags.IndexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
			m_indexBuffer = new Buffer(m_d3Device, indexBufferDescription);
		}

		private void ProcessInputEvent(ReadOnlyCollection<SInputButtonEvent> buttonEvents, string textInput)
		{
			if (!string.IsNullOrEmpty(textInput))
			{
				lock (m_textInputMutex)
				{
					m_textInputBuffer += textInput;
				}
			}
		}

		/// <summary>
		/// Bound a texture to be used with imgui, the id will be used by imgui to notify us which texture to draw, bound textures will be disposed with the ui renderer
		/// </summary>
		/// <param name="texture"></param>
		/// <returns></returns>
		public IntPtr BindTexture(CTextureSampler texture)
		{
			IntPtr id = new IntPtr(m_textureId++);
			m_boundTextures.Add(id, texture);
			return id;
		}

		public CFontProvider FontProvider { get; private set; }

		private IntPtr m_contextPtr;
		private ImGuiIOPtr m_imguiIO;
		private Device m_d3Device;
		private DeviceContext m_d3DeviceContext;
		
		private Buffer m_vertexBuffer;
		private Buffer m_indexBuffer;
		private RasterizerState m_rasterizerState;
		private BlendState m_blendState;
		private DepthStencilState m_depthStencilState;

		private Dictionary<IntPtr, CTextureSampler> m_boundTextures = new Dictionary<IntPtr, CTextureSampler>();
		private IntPtr m_fontAtlasId;

		private Dictionary<SHashedName, SShaderParameter> m_shaderParameters = new Dictionary<SHashedName, SShaderParameter>();

		private CUIShader m_shader;

		private int m_vertexBufferSize = 5000;
		private int m_indexBufferSize = 10000;

		private int m_textureId = 0;

		private int m_screenWidth;
		private int m_screenHeight;
		private int m_screenLeft;
		private int m_screenTop;

		private readonly List<EInputButton> m_registredButtons = new List<EInputButton>(15);

		private int m_mouseAbsX;
		private int m_mouseAbsY;

		private string m_textInputBuffer = "";
		private readonly object m_textInputMutex = new object();
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using KlaxMath;
using KlaxRenderer.Debug.Primitives;
using KlaxRenderer.Graphics.UI;
using KlaxRenderer.Scene;
using KlaxShared;
using KlaxShared.Definitions.Graphics;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;

namespace KlaxRenderer.Debug
{
	[StructLayout(LayoutKind.Sequential)]
	struct DebugVertexType
	{
		public Vector3 position;
		private float _padding;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct PrimitivePerInstanceData
	{
		public Vector4 color;
		public Matrix worldMatrix;
	}

	[Flags]
	public enum EDebugDrawCommandFlags
	{
		None = 0,
		Wireframe = 1,
		NoDepthTest = 2,
		Transparent = 4,
		All = 8
	}

	public class CDebugRenderer : IDisposable
	{
		public CDebugRenderer()
		{
			for (int i = 0; i < m_primitiveCommandLists.Length; i++)
			{
				m_primitiveCommandLists[i] = new CDebugDrawCommandList();
			}
		}

		internal void Init(Device device, CFontProvider fontProvider)
		{
			m_textRenderer.Init(fontProvider);
			m_primitiveShader.Init(device);
			m_primitiveMap.Init(device);
			m_lineShader.Init(device);

			for (int i = 0; i < m_lineCommandLists.Length; i++)
			{
				CDebugLineCommandList lineCommandList = new CDebugLineCommandList();
				lineCommandList.Init(device);
				m_lineCommandLists[i] = lineCommandList;
			}

			RasterizerStateDescription wireframeDescription = new RasterizerStateDescription()
			{
				CullMode = CullMode.Back,
				DepthBias = 0,
				FillMode = FillMode.Wireframe,
				IsScissorEnabled = false
			};

			m_wireframeState = new RasterizerState(device, wireframeDescription);

			RasterizerStateDescription solidDescription = new RasterizerStateDescription()
			{
				CullMode = CullMode.Back,
				DepthBias = 0,
				FillMode = FillMode.Solid,
				IsScissorEnabled = false
			};

			m_solidState = new RasterizerState(device, solidDescription);

			DepthStencilStateDescription depthEnabledDescription;
			depthEnabledDescription.IsDepthEnabled = true;
			depthEnabledDescription.DepthWriteMask = DepthWriteMask.All;
			depthEnabledDescription.DepthComparison = Comparison.Less;
			
			depthEnabledDescription.IsStencilEnabled = true;
			depthEnabledDescription.StencilReadMask = 0xFF;
			depthEnabledDescription.StencilWriteMask = 0xFF;
			
			depthEnabledDescription.FrontFace.FailOperation = StencilOperation.Keep;
			depthEnabledDescription.FrontFace.DepthFailOperation = StencilOperation.Keep;
			depthEnabledDescription.FrontFace.PassOperation = StencilOperation.Keep;
			depthEnabledDescription.FrontFace.Comparison = Comparison.Always;
			
			depthEnabledDescription.BackFace.FailOperation = StencilOperation.Keep;
			depthEnabledDescription.BackFace.DepthFailOperation = StencilOperation.Decrement;
			depthEnabledDescription.BackFace.PassOperation = StencilOperation.Keep;
			depthEnabledDescription.BackFace.Comparison = Comparison.Always;

			m_depthEnabledState = new DepthStencilState(device, depthEnabledDescription);

			DepthStencilStateDescription depthDisabledDescription = new DepthStencilStateDescription();
			depthDisabledDescription.IsDepthEnabled = false;
			depthDisabledDescription.IsStencilEnabled = false;

			m_depthDisabledState = new DepthStencilState(device, depthDisabledDescription);
		}

		public void Dispose()
		{
			m_primitiveShader.Dispose();
			m_wireframeState.Dispose();
			m_solidState.Dispose();
			m_depthEnabledState.Dispose();
			m_depthDisabledState.Dispose();
			m_primitiveMap.Dispose();
			m_lineShader.Dispose();

			foreach (CDebugLineCommandList commandList in m_lineCommandLists)
			{
				commandList.Dispose();
			}
		}

		internal void Update(float deltaTime)
		{
			foreach (CDebugLineCommandList lineCommandList in m_lineCommandLists)
			{
				lineCommandList.Update(deltaTime);
			}

			for (int i = 0; i < (int)EDebugDrawCommandFlags.All; i++)
			{
				m_lineCommandLists[i].Update(deltaTime);
				m_primitiveCommandLists[i].Update(deltaTime);
			}
			
			m_textRenderer.Update(deltaTime);
		}

		internal void Draw(DeviceContext deviceContext, in SSceneViewInfo viewInfo)
		{
			m_bWireframe = false;
			m_bDepthEnabled = true;

			RasterizerState prevRasterizerState = deviceContext.Rasterizer.State;
			DepthStencilState prevDepthStencilState = deviceContext.OutputMerger.DepthStencilState;

			deviceContext.Rasterizer.State = m_solidState;
			deviceContext.OutputMerger.DepthStencilState = m_depthEnabledState;

			Matrix viewProjection = Matrix.Multiply(viewInfo.ViewMatrix, viewInfo.ProjectionMatrix);
			SShaderParameter shaderParam = new SShaderParameter
			{
				parameterType = EShaderParameterType.Matrix,
				parameterData = viewProjection
			};

			// Draw primitives
			Dictionary<SHashedName, SShaderParameter> parameters = new Dictionary<SHashedName, SShaderParameter>();
			parameters.Add(new SHashedName("viewProjectionMatrix"), shaderParam);
			m_primitiveShader.SetShaderParameters(deviceContext, parameters);
			m_primitiveShader.SetActive(deviceContext);
			deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

			for (int i = 0; i < (int)EDebugDrawCommandFlags.All; i++)
			{
				CDebugDrawCommandList primitiveCommandList = m_primitiveCommandLists[i];
				if (primitiveCommandList.HasAnyCommands())
				{
					EDebugDrawCommandFlags currentFlags = (EDebugDrawCommandFlags) i;
					bool bWireframeSet = currentFlags.HasFlag(EDebugDrawCommandFlags.Wireframe);
					if (bWireframeSet != m_bWireframe)
					{
						m_bWireframe = bWireframeSet;
						deviceContext.Rasterizer.State = m_bWireframe ? m_wireframeState : m_solidState;
					}

					bool bDepthEnabled = !currentFlags.HasFlag(EDebugDrawCommandFlags.NoDepthTest);
					if (bDepthEnabled != m_bDepthEnabled)
					{
						m_bDepthEnabled = bDepthEnabled;
						deviceContext.OutputMerger.DepthStencilState = m_bDepthEnabled ? m_depthEnabledState : m_depthDisabledState;
					}

					primitiveCommandList.Draw(deviceContext, m_primitiveMap);
				}
			}

			// Draw lines
			m_lineShader.SetShaderParameters(deviceContext, parameters);
			m_lineShader.SetActive(deviceContext);
			deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;

			for (int i = 0; i < (int)EDebugDrawCommandFlags.All; i++)
			{
				CDebugLineCommandList lineCommandList = m_lineCommandLists[i];
				if (lineCommandList.HasAnyCommands())
				{
					EDebugDrawCommandFlags currentFlags = (EDebugDrawCommandFlags)i;
					bool bWireframeSet = currentFlags.HasFlag(EDebugDrawCommandFlags.Wireframe);
					if (bWireframeSet != m_bWireframe)
					{
						m_bWireframe = bWireframeSet;
						deviceContext.Rasterizer.State = m_bWireframe ? m_wireframeState : m_solidState;
					}

					bool bDepthEnabled = !currentFlags.HasFlag(EDebugDrawCommandFlags.NoDepthTest);
					if (bDepthEnabled != m_bDepthEnabled)
					{
						m_bDepthEnabled = bDepthEnabled;						
						deviceContext.OutputMerger.DepthStencilState = m_bDepthEnabled ? m_depthEnabledState : m_depthDisabledState;
					}

					lineCommandList.Draw(deviceContext);
				}
			}

			// Draw text
			m_textRenderer.Draw(deviceContext, in viewInfo);

			// Reset device state
			deviceContext.Rasterizer.State = prevRasterizerState;
			deviceContext.OutputMerger.DepthStencilState = prevDepthStencilState;
			prevRasterizerState.Dispose();
			prevDepthStencilState.Dispose();
		}

		public void DrawBox(in Vector3 position, in Quaternion rotation, in Vector3 extent, in Color4 color, float displayTime, EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None)
		{
			Matrix worldMatrix = MathUtilities.CreateLocalTransformationMatrix(extent, rotation, position);
			worldMatrix.Transpose();

			PrimitivePerInstanceData drawData = new PrimitivePerInstanceData()
			{
				color = color,
				worldMatrix = worldMatrix
			};

			CDebugDrawCommandList commandList = m_primitiveCommandLists[(int) flags];
			commandList.AddInstance(EDebugPrimitiveType.Cube, displayTime, in drawData);
		}

		public void DrawSphere(in Vector3 center, float radius, in Color4 color, float displayTime, EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None)
		{
			Matrix worldMatrix = Matrix.AffineTransformation(radius, Quaternion.Identity, center);
			worldMatrix.Transpose();

			PrimitivePerInstanceData drawData = new PrimitivePerInstanceData()
			{
				color = color,
				worldMatrix = worldMatrix
			};

			CDebugDrawCommandList commandList = m_primitiveCommandLists[(int) flags];
			commandList.AddInstance(EDebugPrimitiveType.Sphere, displayTime, in drawData);
		}
		
		public void DrawHemisphere(in Vector3 baseCenter, in Vector3 up, float radius, in Color4 color, float displayTime, EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None)
		{
			Vector3 right = Axis.Right;
			if (Vector3.NearEqual(right, up, new Vector3(0.01f)))
			{
				right = Axis.Forward;
			}

			Vector3 forward = Vector3.Cross(right, up);
			forward.Normalize();

			Quaternion rotation = MathUtilities.CreateLookAtQuaternion(forward, up);
			Matrix worldMatrix = Matrix.AffineTransformation(radius, rotation, baseCenter);
			worldMatrix.Transpose();

			PrimitivePerInstanceData drawData = new PrimitivePerInstanceData()
			{
				color = color,
				worldMatrix = worldMatrix
			};

			CDebugDrawCommandList commandList = m_primitiveCommandLists[(int)flags];
			commandList.AddInstance(EDebugPrimitiveType.Hemisphere, displayTime, in drawData);
		}

		public void DrawCylinder(in Vector3 center, float height, float radius, in Quaternion rotation, in Color4 color, float displayTime, EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None)
		{
			Vector3 scaling = new Vector3(radius, height, radius);
			Matrix worldMatrix = MathUtilities.CreateLocalTransformationMatrix(scaling, rotation, center);
			worldMatrix.Transpose();

			PrimitivePerInstanceData drawData = new PrimitivePerInstanceData()
			{
				color = color,
				worldMatrix = worldMatrix
			};

			CDebugDrawCommandList commandList = m_primitiveCommandLists[(int) flags];
			commandList.AddInstance(EDebugPrimitiveType.Cylinder, displayTime, in drawData);
		}

		public void DrawCapsule(in Vector3 center, float height, float radius, in Quaternion rotation, in Color4 color, float displayTime, EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None)
		{
			Vector3 upAxis = Vector3.Transform(Axis.Up, rotation);
			float sphereOffset = height / 2;
			DrawHemisphere(center + upAxis * sphereOffset, upAxis, radius, color, displayTime, flags);
			DrawHemisphere(center - upAxis * sphereOffset, -upAxis, radius, color, displayTime, flags);
			DrawCylinder(center, sphereOffset * 2, radius, rotation, color, displayTime, flags);
		}

		public void DrawCone(in Vector3 baseCenter, float height, float radius, in Quaternion rotation, in Color4 color, float displayTime, EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None)
		{
			Vector3 scaling = new Vector3(radius, height, radius);
			Matrix worldMatrix = MathUtilities.CreateLocalTransformationMatrix(scaling, rotation, baseCenter);
			worldMatrix.Transpose();

			PrimitivePerInstanceData drawData = new PrimitivePerInstanceData()
			{
				color = color,
				worldMatrix = worldMatrix
			};

			CDebugDrawCommandList commandList = m_primitiveCommandLists[(int)flags];
			commandList.AddInstance(EDebugPrimitiveType.Cone, displayTime, in drawData);
		}

		public void DrawPyramid(in Vector3 baseCenter, float baseLength, float baseWidth, float height, in Quaternion rotation, in Color4 color, float displayTime, EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None)
		{
			Vector3 scaling = new Vector3(baseWidth, height, baseLength);
			Matrix worldMatrix = MathUtilities.CreateLocalTransformationMatrix(scaling, rotation, baseCenter);
			worldMatrix.Transpose();

			PrimitivePerInstanceData drawData = new PrimitivePerInstanceData()
			{
				color = color,
				worldMatrix = worldMatrix
			};

			CDebugDrawCommandList commandList = m_primitiveCommandLists[(int) flags];
			commandList.AddInstance(EDebugPrimitiveType.Pyramid, displayTime, in drawData);
		}

		public void DrawLine(in Vector3 startPos, in Color4 startColor, in Vector3 endPos, in Color4 endColor, float displayTime, EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None)
		{
			LineData lineData = new LineData()
			{
				pos1 = startPos,
				col1 = startColor,
				pos2 = endPos,
				col2 = endColor
			};

			m_lineCommandLists[(int)flags].AddLine(displayTime, in lineData);
		}

		public void DrawPoint(in Vector3 location, float size, in Color4 color, float displayTime, EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None)
		{
			LineData pointX = new LineData()
			{
				pos1 = location - new Vector3(size / 2, 0, 0),
				col1 = color,
				pos2 = location + new Vector3(size / 2, 0, 0),
				col2 = color
			};

			LineData pointY = new LineData()
			{
				pos1 = location - new Vector3(0, size / 2, 0),
				col1 = color,
				pos2 = location + new Vector3(0, size / 2, 0),
				col2 = color
			};

			LineData pointZ = new LineData()
			{
				pos1 = location - new Vector3(0, 0, size / 2),
				col1 = color,
				pos2 = location + new Vector3(0, 0, size / 2),
				col2 = color
			};

			m_lineCommandLists[(int)flags].AddLine(displayTime, in pointX);
			m_lineCommandLists[(int)flags].AddLine(displayTime, in pointY);
			m_lineCommandLists[(int)flags].AddLine(displayTime, in pointZ);
		}

		public void DrawArrow(in Vector3 startPos, in Vector3 direction, float length, in Color4 color, float displayTime, EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None)
		{
			System.Diagnostics.Debug.Assert(!direction.IsZero, "Direction cannot be zero");

			direction.Normalize();
			Vector3 endPos = startPos + direction * length;

			Vector3 right;
			if (Vector3.NearEqual(direction, Axis.Up, new Vector3(0.1f)))
			{
				right = Vector3.Cross(direction, Axis.Right);
			}
			else
			{
				right = Vector3.Cross(direction, Axis.Up);
			}
			right.Normalize();
			Vector3 up = Vector3.Cross(direction, right);
			up.Normalize();

			Vector3 backOffset = -direction * 0.2f * length;
			Vector3 rightOffset = backOffset + right * 0.1f * length;
			Vector3 leftOffset = backOffset - right * 0.1f * length;
			Vector3 topOffset = backOffset + up * 0.1f * length;
			Vector3 botOffset = backOffset - up * 0.1f * length;

			LineData dirLine = LineData.CreateLine(startPos, endPos, color);
			LineData topLine = LineData.CreateLine(endPos, endPos + topOffset, color);
			LineData botLine = LineData.CreateLine(endPos, endPos + botOffset, color);
			LineData rightLine = LineData.CreateLine(endPos, endPos + rightOffset, color);
			LineData leftLine = LineData.CreateLine(endPos, endPos + leftOffset, color);
			
			m_lineCommandLists[(int)flags].AddLine(displayTime, in dirLine);
			m_lineCommandLists[(int)flags].AddLine(displayTime, in topLine);
			m_lineCommandLists[(int)flags].AddLine(displayTime, in botLine);
			m_lineCommandLists[(int)flags].AddLine(displayTime, in rightLine);
			m_lineCommandLists[(int)flags].AddLine(displayTime, in leftLine);
		}

		public void DrawCircle(in Vector3 center, in Vector3 axis, float radius, in Color4 color, float displayTime, int segments = 32, EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None)
		{
			axis.Normalize();
			Vector3 up = Axis.Up;
			if (Vector3.NearEqual(axis, up, new Vector3(0.01f)))
			{
				up = Axis.Right;
			}

			Matrix circleMatrix = Matrix.LookAtLH(center, center + axis, up);
			circleMatrix.Invert();
			float step = MathUtil.TwoPi / segments;
			float rad = 0;
			CDebugLineCommandList lineList = m_lineCommandLists[(int) flags];
			for (int i = 0; i < segments; i++)
			{
				Vector3 p1 = new Vector3(MathUtilities.Cosf(rad) * radius, MathUtilities.Sinf(rad) * radius, 0);
				p1 = Vector3.TransformCoordinate(p1, circleMatrix);

				rad += step;

				Vector3 p2 = new Vector3(MathUtilities.Cosf(rad) * radius, MathUtilities.Sinf(rad) * radius, 0);
				p2 = Vector3.TransformCoordinate(p2, circleMatrix);

				LineData l = LineData.CreateLine(p1, color, p2, color);
				lineList.AddLine(displayTime, in l);
			}
		}
		public void DrawCircleSegment(in Vector3 center, in Vector3 axis, in Vector3 up, float radius, float fraction, in Color4 color, float displayTime, int segments = 16, EDebugDrawCommandFlags flags = EDebugDrawCommandFlags.None)
		{
			Vector3 upAxis = up;
			fraction = MathUtil.Clamp(fraction, 0.0f, 1.0f);
			axis.Normalize();
			if (Vector3.NearEqual(axis, up, new Vector3(0.01f)))
			{
				upAxis = Axis.Right;
			}

			Matrix circleMatrix = Matrix.LookAtLH(center, center + axis, upAxis);
			circleMatrix.Invert();
			float step = MathUtil.TwoPi * fraction / segments;
			float rad = 0;
			CDebugLineCommandList lineList = m_lineCommandLists[(int)flags];
			for (int i = 0; i < segments; i++)
			{
				Vector3 p1 = new Vector3(MathUtilities.Cosf(rad) * radius, MathUtilities.Sinf(rad) * radius, 0);
				p1 = Vector3.TransformCoordinate(p1, circleMatrix);

				rad += step;

				Vector3 p2 = new Vector3(MathUtilities.Cosf(rad) * radius, MathUtilities.Sinf(rad) * radius, 0);
				p2 = Vector3.TransformCoordinate(p2, circleMatrix);

				LineData l = LineData.CreateLine(p1, color, p2, color);
				lineList.AddLine(displayTime, in l);
			}
		}

		public void DrawTextScreenRel(in Vector2 screenPos, string text, float size, in Color4 color, float displayTime)
		{
			STextDrawCommand textData = new STextDrawCommand(new Vector3(screenPos, 0.0f), size, false, false, text, color);
			m_textRenderer.AddText(in textData, displayTime);
		}

		public void DrawTextScreenAbs(in Vector2 screenPos, string text, float size, in Color4 color, float displayTime)
		{
			STextDrawCommand textData = new STextDrawCommand(new Vector3(screenPos, 0.0f), size, false, true, text, color);
			m_textRenderer.AddText(in textData, displayTime);
		}

		public void DrawTextWorld(in Vector3 worldPos, string text, float size, in Color4 color, float displayTime)
		{
			STextDrawCommand textData = new STextDrawCommand(worldPos, size, true, true, text, color);
			m_textRenderer.AddText(in textData, displayTime);
		}

		private readonly CDebugTextRenderer m_textRenderer = new CDebugTextRenderer();
		private readonly CDebugObjectShader m_primitiveShader = new CDebugObjectShader();
		private readonly CDebugLineShader m_lineShader = new CDebugLineShader();

		private readonly CDebugDrawCommandList[] m_primitiveCommandLists = new CDebugDrawCommandList[(int)EDebugDrawCommandFlags.All];
		private readonly CDebugLineCommandList[] m_lineCommandLists = new CDebugLineCommandList[(int)EDebugDrawCommandFlags.All];
		private readonly CDebugPrimitiveMap m_primitiveMap = new CDebugPrimitiveMap();

		private RasterizerState m_wireframeState;
		private RasterizerState m_solidState;
		private DepthStencilState m_depthEnabledState;
		private DepthStencilState m_depthDisabledState;

		private bool m_bWireframe;
		private bool m_bDepthEnabled;
	}
}

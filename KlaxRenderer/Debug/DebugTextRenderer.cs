using System.Collections.Generic;
using ImGuiNET;
using KlaxMath;
using KlaxRenderer.Camera;
using KlaxRenderer.Graphics.UI;
using KlaxRenderer.Scene;
using KlaxShared.Utilities;
using SharpDX;
using SharpDX.Direct3D11;
using Vector3 = SharpDX.Vector3;

namespace KlaxRenderer.Debug
{
	readonly struct STextDrawCommand
	{
		public readonly Vector3 position;
		public readonly float size;
		public readonly bool bWorldSpace;
		public readonly bool bAbsoluteScreen;
		public readonly string text;
		public readonly Color4 color;

		public STextDrawCommand(Vector3 inPos, float inSize, bool worldSpace, bool absScreen, string inText, Color4 inColor)
		{
			position = inPos;
			size = inSize;
			bWorldSpace = worldSpace;
			bAbsoluteScreen = absScreen;
			text = inText;
			color = inColor;
		}
	}
	class CDebugTextRenderer
	{
		public const int MAX_NUM_LABELS = 100000;
		public const int MAX_LABELS_PER_WINDOW = 1000;

		public void Init(CFontProvider fontProvider)
		{
			m_fontProvider = fontProvider;
		}

		public void Update(float deltaTime)
		{
			for (int i = m_remainingTimes.Count - 1; i >= 0; --i)
			{
				m_remainingTimes[i] -= deltaTime;				
				if (m_remainingTimes[i] <= 0.0f)
				{
					ContainerUtilities.RemoveSwapAt(m_remainingTimes, i);
					ContainerUtilities.RemoveSwapAt(m_persistentDrawCommands, i);
				}
			}
		}

		public void Draw(DeviceContext deviceContext, in SSceneViewInfo viewInfo)
		{
			m_currentLabelCount = 0;
			m_windowCount = 0;

			float screenWidth = viewInfo.ScreenWidth;
			float screenHeight = viewInfo.ScreenHeight;

			OpenWindow(screenWidth, screenHeight, out ImDrawListPtr windowDrawList);

			DrawCommandList(m_persistentDrawCommands, in viewInfo, ref windowDrawList);
			DrawCommandList(m_frameDrawCommand, in viewInfo,ref windowDrawList);

			ImGui.End();

			m_frameDrawCommand.Clear();
		}

		public void AddText(in STextDrawCommand text, float displayTime)
		{
			int totalTextCount = m_persistentDrawCommands.Count + m_remainingTimes.Count;
			if (totalTextCount >= MAX_NUM_LABELS)
			{
				LogUtility.Log("Could not add debug text: " + text.text + ", maximum number of labels reached");
			}

			if (displayTime > 0.0f)
			{
				m_persistentDrawCommands.Add(text);
				m_remainingTimes.Add(displayTime);
			}
			else
			{
				m_frameDrawCommand.Add(text);
			}
		}

		private void DrawCommandList(List<STextDrawCommand> commands, in SSceneViewInfo viewInfo, ref ImDrawListPtr drawList)
		{
			Vector3 viewForward = viewInfo.ViewMatrix.Column3.ToVector3();
			float screenWidth = viewInfo.ScreenWidth;
			float screenHeight = viewInfo.ScreenHeight;

			for (int i = 0; i < commands.Count; i++)
			{
				STextDrawCommand drawCommand = commands[i];
				Vector3 drawPos = drawCommand.position;
				if (drawCommand.bWorldSpace)
				{
					// Check that we only draw text that is in front of the camera
					Vector3 toDrawPos = drawPos - viewInfo.ViewLocation;
					toDrawPos.Normalize();
					float dotCam = Vector3.Dot(toDrawPos, viewForward);
					if (dotCam < 0.0f)
					{
						continue;
					}

					drawPos = viewInfo.WorldToScreenPoint(drawCommand.position);

					if (drawPos.X < 0 || drawPos.X > screenWidth || drawPos.Y < 0 || drawPos.Y > screenHeight)
					{
						continue;
					}
				}

				System.Numerics.Vector4 colorVector = new System.Numerics.Vector4(drawCommand.color.Red, drawCommand.color.Green, drawCommand.color.Blue, drawCommand.color.Alpha);
				System.Numerics.Vector2 drawScreen = new System.Numerics.Vector2(drawPos.X, drawPos.Y);
				if (!drawCommand.bAbsoluteScreen && !drawCommand.bWorldSpace)
				{
					drawScreen.X *= screenWidth;
					drawScreen.Y *= screenHeight;
				}

				ImFontPtr font = m_fontProvider.GetDefaultFont(drawCommand.size);
				drawList.AddText(font, drawCommand.size, drawScreen, ImGui.ColorConvertFloat4ToU32(colorVector), drawCommand.text);

				m_currentLabelCount++;
				if (m_currentLabelCount > MAX_LABELS_PER_WINDOW)
				{
					ImGui.End();
					OpenWindow(screenWidth, screenHeight, out drawList);
				}
			}

		}

		private void OpenWindow(float width, float height, out ImDrawListPtr drawList)
		{
			m_currentLabelCount = 0;
			ImGui.SetNextWindowSize(new System.Numerics.Vector2(width, height));
			ImGui.SetNextWindowPos(new System.Numerics.Vector2(0.0f, 0.0f));
			ImGui.SetNextWindowBgAlpha(0.0f);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
			ImGui.Begin("DebugText" + m_windowCount, ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoTitleBar);
			drawList = ImGui.GetWindowDrawList();
			m_windowCount++;
		}

		private readonly List<STextDrawCommand> m_persistentDrawCommands = new List<STextDrawCommand>();
		private readonly List<float> m_remainingTimes = new List<float>();
		private readonly List<STextDrawCommand> m_frameDrawCommand = new List<STextDrawCommand>();
		private CFontProvider m_fontProvider;

		private int m_currentLabelCount;
		private int m_windowCount;
	}
}

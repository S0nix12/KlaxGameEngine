using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;
using KlaxConfig;
using KlaxConfig.Console;
using KlaxIO.Input;
using KlaxIO.Log;

namespace KlaxCore.GameFramework.Console
{
	public class GameConsole
	{
		public const int MAX_LOG_HISTORY = 80000;

		public void Init()
		{
			LogUtility.Logged += OnLogged;
			Input.RegisterListener(OnInputEvents);
		}
		public void Open()
		{
			m_bIsOpen = true;
		}

		public void Close()
		{
			m_bIsOpen = false;
		}

		public bool IsOpen()
		{
			return m_bIsOpen;
		}

		public void ClearLog()
		{
			m_logHistory.Clear();
		}

		public void Draw(float screenWidth, float screenHeight)
		{
			if (!m_bIsOpen)
			{
				m_bOpenLastFrame = m_bIsOpen;
				return;
			}

			PushGeneralStyle();
			Vector2 windowPos = Vector2.Zero;
			Vector2 windowSize = new Vector2(screenWidth, screenHeight * 0.3f);
			ImGui.SetNextWindowSize(windowSize);
			ImGui.SetNextWindowPos(windowPos);

			ImGui.Begin("GameConsole", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize);
			float footerSize = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing(); // 1 seperator + 1 inputText


			ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(1,1));
			ImGui.BeginChild("TextRegion", new Vector2(0.0f, -footerSize), false, ImGuiWindowFlags.HorizontalScrollbar);

			if (ImGui.BeginPopupContextWindow())
			{
				if (ImGui.Selectable("Clear")) m_logHistory.Clear();
				if (ImGui.Selectable("Close")) Close();
				ImGui.Selectable("Auto Scroll", ref m_bAutoScroll);
				ImGui.EndPopup();
			}

			for (int i = 0; i < m_logHistory.Count; i++)
			{
				ImGui.Text(m_logHistory[i]);
			}

			if (m_bScrollToBottom)
			{
				ImGui.SetScrollHere(1.0f);
				m_bScrollToBottom = false;
			}

			ImGui.EndChild();
			ImGui.PopStyleVar();

			ImGui.Separator();

			unsafe
			{				
				byte[] inputBuffer = new byte[512];
				bool bRefocus = false;
				if (ImGui.InputText("Enter Command", inputBuffer, 512, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackAlways, TextEditCallback))
				{
					string encodedInput = Encoding.UTF8.GetString(inputBuffer);
					encodedInput = encodedInput.Trim('\0'); // String is null terminated
					ExecCommand(encodedInput);
					bRefocus = true;
					m_bScrollToBottom = m_bAutoScroll;
				}
				ImGui.SetItemDefaultFocus();

				if (!m_bOpenLastFrame) 
				{
					ImGui.SetKeyboardFocusHere();
				}
				
				if (bRefocus)
				{
					ImGui.SetKeyboardFocusHere(-1);
				}
			}
			
			ImGui.End();
			PopGeneralStyle();

			m_bOpenLastFrame = m_bIsOpen;
		}

		private unsafe int TextEditCallback(ImGuiInputTextCallbackData* dataPtr)
		{
			ImGuiInputTextCallbackDataPtr data = dataPtr;
			switch (data.EventFlag)
			{
				case ImGuiInputTextFlags.CallbackAlways:
					if (m_suggestionPosition >= 0 && data.BufTextLen != m_suggestionKeepLength)
					{						
						InvalidateSuggestions();
					}
					break;
				case ImGuiInputTextFlags.CallbackCompletion:
					ShowSuggestion(data);
					break;
				case ImGuiInputTextFlags.CallbackHistory:
					if ((data.EventKey == ImGuiKey.UpArrow || data.EventKey == ImGuiKey.DownArrow) && m_commandHistory.Count > 0)
					{
						SelectHistory(data.EventKey == ImGuiKey.UpArrow);
						data.DeleteChars(0, data.BufTextLen);
						data.InsertChars(0, m_commandHistory[m_historyPosition]);
					}
					break;
			}

			return 0;
		}

		private void OnInputEvents(ReadOnlyCollection<SInputButtonEvent> buttonEvents, string textInput)
		{
			foreach (SInputButtonEvent buttonEvent in buttonEvents)
			{
				if (buttonEvent.button == EInputButton.Grave && buttonEvent.buttonEvent == EButtonEvent.Pressed)
				{
					// Toggle Console
					m_bIsOpen = !m_bIsOpen;
				}
			}
		}

		private void SelectHistory(bool bSelectPrevious)
		{
			m_historyPosition = bSelectPrevious ? m_historyPosition - 1 : m_historyPosition + 1;
			if (m_historyPosition < 0)
			{
				m_historyPosition = m_commandHistory.Count - 1;
			}
			else if(m_historyPosition >= m_commandHistory.Count)
			{
				m_historyPosition = 0;
			}
		}

		private void OnLogged(CLogger logger, string withoutTimestamp, string loggedText)
		{
			m_logHistory.Add(loggedText);
			m_bScrollToBottom = m_bAutoScroll;

			if (m_logHistory.Count > MAX_LOG_HISTORY)
			{
				m_logHistory.RemoveRange(0, MAX_LOG_HISTORY / 10);
			}
		}

		private void PushGeneralStyle()
		{
			ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
			ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 0.1f);
			ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 3.0f);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));
		}

		private void PopGeneralStyle()
		{
			ImGui.PopStyleVar(4);
		}

		private void AddToHistory(string command)
		{
			m_commandHistory.Remove(command);
			m_commandHistory.Add(command);			

			m_historyPosition = m_commandHistory.Count;			
		}

		private void ExecCommand(string command)
		{
			if (command.StartsWith("?"))
			{
				string substring = command.Substring(1);
				List<string> suggestions = new List<string>();
				CConfigManager.Instance.GetConsoleSuggestionsContainingString(substring, ref suggestions);
				if (suggestions.Count > 0)
				{
					foreach (string suggestion in suggestions)
					{
						LogUtility.Log(suggestion);
					}
				}
				else
				{
					LogUtility.Log("No commands found");
				}
			}
			else
			{
				ConsoleUtility.ProcessConsoleString(command);
				LogUtility.Log("Executing: " + command);
			}
			AddToHistory(command);
		}		

		private void ShowSuggestion(ImGuiInputTextCallbackDataPtr data)
		{
			byte[] buffer = new byte[data.BufSize];
			Marshal.Copy(data.Buf, buffer, 0, data.BufSize);
			string encodedString = Encoding.UTF8.GetString(buffer);
			int nullTerminator = encodedString.IndexOf('\0');
			encodedString = encodedString.Substring(0, nullTerminator);

			if (!string.IsNullOrWhiteSpace(encodedString))
			{
				// If we are not having any suggestions currently get new ones
				if (m_suggestionPosition < 0)
				{
					List<string> fullSuggestions = new List<string>();
					CConfigManager.Instance.GetConsoleSuggestions(encodedString, ref fullSuggestions, ref m_currentSuggestions);
					foreach (string suggestion in fullSuggestions)
					{
						m_logHistory.Add("[Suggestion]"+suggestion);
					}

					if (m_currentSuggestions.Count > 0)
					{
						m_suggestionPosition = 0;
					}
					else
					{
						m_logHistory.Add("No suggestions for: " + encodedString);
					}
				}
				else
				{
					m_suggestionPosition++;
					if (m_suggestionPosition >= m_currentSuggestions.Count)
					{
						m_suggestionPosition = 0;
					}
				}

				if (m_suggestionPosition >= 0)
				{
					data.DeleteChars(0, data.BufTextLen);
					data.InsertChars(0, m_currentSuggestions[m_suggestionPosition]);
					m_suggestionKeepLength = data.BufTextLen;
				}
			}
		}

		private void InvalidateSuggestions()
		{
			m_suggestionPosition = -1;
			m_suggestionKeepLength = 0;
		}

		private List<string> m_logHistory = new List<string>();
		private int m_historyPosition;

		private List<string> m_commandHistory = new List<string>();
		private List<string> m_currentSuggestions = new List<string>();
		private int m_suggestionPosition = -1;
		private int m_suggestionKeepLength;
		private bool m_bIsOpen;
		private bool m_bOpenLastFrame;
		private bool m_bAutoScroll = true;
		private bool m_bScrollToBottom;
	}
}

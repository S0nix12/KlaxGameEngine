using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using KlaxShared;
using SharpDX;
using SharpDX.DirectInput;
using SharpDX.XInput;
using ResultCode = SharpDX.DirectInput.ResultCode;

namespace KlaxIO.Input
{
	public enum EInputClass
	{
		Default,
		AssetPreview,
		Custom1,
		Custom2,
		Custom3,
		Custom4,
		Count
	}

	public static class Input
	{
		public delegate void ProcessInputEvents(ReadOnlyCollection<SInputButtonEvent> buttonEvents, string textInput);

		private struct SInputListenerCollection
		{
			public volatile bool bIsActive;
			public List<ProcessInputEvents> callbacks;
		}

		private struct SInputListenerAction
		{
			public SInputListenerAction(EInputClass inputClass, ProcessInputEvents callback)
			{
				this.inputClass = inputClass;
				this.callback = callback;
			}

			public EInputClass inputClass;
			public ProcessInputEvents callback;
		}

		public static void InitInput()
		{
			if (!m_bIsInitialized)
			{
				lock (m_inputMutex)
				{
					for (int i = 0, count = (int)EInputClass.Count; i < count; i++)
					{
						m_inputListeners[i].callbacks = new List<ProcessInputEvents>(8);
						m_inputListeners[i].bIsActive = true;
					}

					m_directInput = new DirectInput();
					m_keyboard = new Keyboard(m_directInput);
					m_mouse = new Mouse(m_directInput);

					m_keyboard.Properties.BufferSize = 128;
					m_mouse.Properties.BufferSize = 128;

					m_keyboard.Acquire();
					m_mouse.Acquire();

					PollInput();
					m_bIsInitialized = true;
				}
			}
		}

		public static void SetReferenceHWND(IntPtr hwnd)
		{
			if (m_referenceHwnd == hwnd)
			{
				return;
			}

			lock (m_inputMutex)
			{
				m_keyboard.Unacquire();
				m_mouse.Unacquire();
				m_keyboard.SetCooperativeLevel(hwnd, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);
				m_mouse.SetCooperativeLevel(hwnd, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);
				m_referenceHwnd = hwnd;

				try
				{
					m_keyboard.Acquire();
					m_mouse.Acquire();
				}
				catch
				{
				}
			}
		}

		public static void Shutdown()
		{
			lock (m_inputMutex)
			{
				m_directInput.Dispose();
				m_keyboard.Dispose();
				m_mouse.Dispose();

				m_bIsInitialized = false;
			}
		}

		public static void PollInput()
		{
			if (!m_bIsInitialized)
			{
				return;
			}

			lock (m_inputMutex)
			{
				try
				{
					m_keyboard.Poll();
				}
				catch (SharpDXException e)
				{
					if (e.ResultCode == ResultCode.InputLost || e.ResultCode == ResultCode.NotAcquired)
					{
						try
						{
							m_keyboard.Acquire();
							m_keyboard.Poll();
						}
						catch
						{
							m_currentKeyboardState = new KeyboardState();
							m_currentMouseState = new MouseState();
							return;
						}
					}
					else
					{
						throw;
					}
				}

				try
				{
					m_mouse.Poll();
				}
				catch (SharpDXException e)
				{
					if (e.ResultCode == ResultCode.InputLost || e.ResultCode == ResultCode.NotAcquired)
					{
						try
						{
							m_mouse.Acquire();
							m_mouse.Poll();
						}
						catch
						{
							return;
						}
					}
					else
					{
						throw;
					}
				}


				List<SInputButtonEvent> buttonEvents = new List<SInputButtonEvent>(100);

				try
				{
					KeyboardUpdate[] keyboardUpdates = m_keyboard.GetBufferedData();
					for (int i = 0; i < keyboardUpdates.Length; i++)
					{
						KeyboardUpdate update = keyboardUpdates[i];
						SInputButtonEvent buttonEvent;
						buttonEvent.buttonEvent = update.IsPressed ? EButtonEvent.Pressed : EButtonEvent.Released;
						buttonEvent.button = (EInputButton)update.Key;
						buttonEvents.Add(buttonEvent);
					}

					MouseUpdate[] mouseUpdates = m_mouse.GetBufferedData();
					for (int i = 0; i < mouseUpdates.Length; i++)
					{
						MouseUpdate update = mouseUpdates[i];

						if (update.IsButton)
						{
							SInputButtonEvent buttonEvent;
							buttonEvent.buttonEvent = (update.Value & 0x80) != 0 ? EButtonEvent.Pressed : EButtonEvent.Released;
							buttonEvent.button = EInputButton.Unknown;
							switch (update.Offset)
							{
								case MouseOffset.Buttons0:
									buttonEvent.button = EInputButton.MouseLeftButton;
									break;
								case MouseOffset.Buttons1:
									buttonEvent.button = EInputButton.MouseRightButton;
									break;
								case MouseOffset.Buttons2:
									buttonEvent.button = EInputButton.MouseMiddleButton;
									break;
								case MouseOffset.Buttons3:
									buttonEvent.button = EInputButton.MouseButton3;
									break;
								case MouseOffset.Buttons4:
									buttonEvent.button = EInputButton.MouseButton4;
									break;
								case MouseOffset.Buttons5:
									buttonEvent.button = EInputButton.MouseButton5;
									break;
								case MouseOffset.Buttons6:
									buttonEvent.button = EInputButton.MouseButton6;
									break;
								case MouseOffset.Buttons7:
									buttonEvent.button = EInputButton.MouseButton7;
									break;
							}

							buttonEvents.Add(buttonEvent);
						}
					}

					lock (m_inputStateMutex)
					{
						m_currentMouseState = m_mouse.GetCurrentState();
						m_currentKeyboardState = m_keyboard.GetCurrentState();
					}
				}
				catch (SharpDXException e)
				{
					if (e.ResultCode == ResultCode.InputLost)
					{
						// We lost input we will try to reacquire next frame
						return;
					}
					else
					{
						throw;
					}
				}

				// TODO henning support multiple controller for now we take the first connected
				for (int i = 0; i < m_controllers.Length; ++i)
				{
					if (m_controllers[i].IsConnected)
					{
						State controllerState = m_controllers[i].GetState();
						if (controllerState.PacketNumber != m_currentGamepadState.PacketNumber)
						{
							ProcessGamepad(controllerState, buttonEvents);
							break;
						}
					}
				}

				UpdateAxis();

				string textInput = "";
				lock (m_textBufferMutex)
				{
					textInput = string.Copy(m_textInputBuffer);
					m_textInputBuffer = "";
				}

				ReadOnlyCollection<SInputButtonEvent> readOnlyButtonEvents = new ReadOnlyCollection<SInputButtonEvent>(buttonEvents);

				//Copy the current activation states of the input classes to separate array so that they cannot be deactivated during iteration
				for (int i = 0, count = (int)EInputClass.Count; i < count; i++)
				{
					m_cachedInputClassStates[i] = m_inputListeners[i].bIsActive;
				}

				//todo valentin: Multithreading needs cleanup once multiple threads need callback
				if (readOnlyButtonEvents.Count > 0 || !string.IsNullOrWhiteSpace(textInput))
				{
					// Notify Listener
					m_bIsIterating = true;
					for (int i = 0, count = (int)EInputClass.Count; i < count; i++)
					{
						if (m_cachedInputClassStates[i])
						{
							var callbacks = m_inputListeners[i].callbacks;
							for (int k = 0, callbackCount = callbacks.Count; k < callbackCount; k++)
							{
								callbacks[k](readOnlyButtonEvents, string.Copy(textInput));
							}
						}
					}
					m_bIsIterating = false;

					//Add/Remove callbacks that were altered during iteration
					for (int i = 0, count = m_iterationAdditions.Count; i < count; i++)
					{
						RegisterListener(m_iterationAdditions[i].callback, m_iterationAdditions[i].inputClass);
					}

					for (int i = 0, count = m_iterationRemovals.Count; i < count; i++)
					{
						UnregisterListener(m_iterationAdditions[i].callback, m_iterationAdditions[i].inputClass);
					}

					m_iterationAdditions.Clear();
					m_iterationRemovals.Clear();
				}
			}
		}

		public static void SetInputClassActive(EInputClass inputClass, bool bIsActive)
		{
			m_inputListeners[(int)inputClass].bIsActive = bIsActive;
		}

		public static bool IsInputClassActive(EInputClass inputClass)
		{
			return m_inputListeners[(int)inputClass].bIsActive;
		}

		public static void RegisterListener(ProcessInputEvents callback, EInputClass inputClass = EInputClass.Default)
		{
			lock (m_listenerMutex)
			{
				if (m_bIsIterating)
				{
					m_iterationAdditions.Add(new SInputListenerAction(inputClass, callback));
				}
				else
				{
					m_inputListeners[(int)inputClass].callbacks.Add(callback);
				}
			}
		}

		public static void UnregisterListener(ProcessInputEvents callback, EInputClass inputClass = EInputClass.Default)
		{
			lock (m_listenerMutex)
			{
				if (m_bIsIterating)
				{
					m_iterationRemovals.Add(new SInputListenerAction(inputClass, callback));
				}
				else
				{
					m_inputListeners[(int)inputClass].callbacks.Remove(callback);
				}
			}
		}

		public static bool ProcessCharacterInput(char inputChar)
		{
			if (!char.IsControl(inputChar))
			{
				lock (m_textBufferMutex)
				{
					m_textInputBuffer += inputChar;
				}
				return true;
			}

			return false;
		}

		public static Vector2 GetAbsoluteMousePosition()
		{
			return new Vector2(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
		}

		public static float GetNativeAxisValue(EInputAxis nativeAxis)
		{
			lock (m_axisMutex)
			{
				return m_nativeAxisValues[(int)nativeAxis];
			}
		}

		public static bool IsButtonPressed(EInputButton button)
		{
			lock (m_inputStateMutex)
			{
				int buttonIndex = (int)button;
				if (buttonIndex < (int)EInputButton.MouseFirst)
				{
					//Keyboard
					Key directInputKey = (Key)buttonIndex;
					return m_currentKeyboardState.IsPressed(directInputKey);
				}
				else if (buttonIndex < (int)EInputButton.ControllerFirst)
				{
					//Mouse
					int mouseButtonIndex = buttonIndex - (int)EInputButton.MouseFirst;
					return m_currentMouseState.Buttons[mouseButtonIndex];
				}
				else
				{
					GamepadButtonFlags gamepadButton = GetXInputFromInputButton(button);
					return m_currentGamepadState.Gamepad.Buttons.HasFlag(gamepadButton);
				}
			}
		}

		private static void UpdateAxis()
		{
			lock (m_axisMutex)
			{
				for (int i = 0; i < m_nativeAxisValues.Length; i++)
				{
					EInputAxis axis = (EInputAxis)i;
					switch (axis)
					{
						case EInputAxis.MouseX:
							m_nativeAxisValues[i] = m_currentMouseState.X;
							break;
						case EInputAxis.MouseY:
							m_nativeAxisValues[i] = m_currentMouseState.Y;
							break;
						case EInputAxis.MouseWheel:
							m_nativeAxisValues[i] = m_currentMouseState.Z;
							break;
						case EInputAxis.ControllerLeftStickX:
							m_nativeAxisValues[i] = ((float)m_currentGamepadState.Gamepad.LeftThumbX) / short.MaxValue;
							break;
						case EInputAxis.ControllerLeftStickY:
							m_nativeAxisValues[i] = ((float)m_currentGamepadState.Gamepad.LeftThumbY) / short.MaxValue;
							break;
						case EInputAxis.ControllerRightStickX:
							m_nativeAxisValues[i] = ((float)m_currentGamepadState.Gamepad.RightThumbX) / short.MaxValue;
							break;
						case EInputAxis.ControllerRightStickY:
							m_nativeAxisValues[i] = ((float)m_currentGamepadState.Gamepad.RightThumbY) / short.MaxValue;
							break;
						case EInputAxis.ControllerRightTrigger:
							m_nativeAxisValues[i] = ((float)m_currentGamepadState.Gamepad.RightTrigger) / byte.MaxValue;
							break;
						case EInputAxis.ControllerLeftTrigger:
							m_nativeAxisValues[i] = ((float)m_currentGamepadState.Gamepad.LeftTrigger) / byte.MaxValue;
							break;
						case EInputAxis.Count:
							break;
					}
				}
			}
		}

		private static void ProcessGamepad(State newGamepadState, List<SInputButtonEvent> outEvents)
		{
			Gamepad newGamepad = newGamepadState.Gamepad;
			ushort changedButtons = (ushort)(m_currentGamepadState.Gamepad.Buttons ^ newGamepad.Buttons);
			for (ushort i = 0; i < sizeof(ushort) * 8; i++)
			{
				if ((changedButtons & (1 << i)) != 0)
				{
					GamepadButtonFlags gamepadButton = (GamepadButtonFlags)(1 << i);
					SInputButtonEvent buttonEvent;
					buttonEvent.buttonEvent = newGamepad.Buttons.HasFlag(gamepadButton) ? EButtonEvent.Pressed : EButtonEvent.Released;
					buttonEvent.button = GetInputButtonFromXInput(gamepadButton);
					outEvents.Add(buttonEvent);
				}
			}

			lock (m_inputStateMutex)
			{
				m_currentGamepadState = newGamepadState;
			}
		}

		private static EInputButton GetInputButtonFromXInput(GamepadButtonFlags xinputButton)
		{
			switch (xinputButton)
			{
				case GamepadButtonFlags.None:
					return EInputButton.Unknown;
				case GamepadButtonFlags.DPadUp:
					return EInputButton.DPadUp;
				case GamepadButtonFlags.DPadDown:
					return EInputButton.DPadUp;
				case GamepadButtonFlags.DPadLeft:
					return EInputButton.DPadLeft;
				case GamepadButtonFlags.DPadRight:
					return EInputButton.DPadRight;
				case GamepadButtonFlags.Start:
					return EInputButton.Start;
				case GamepadButtonFlags.Back:
					return EInputButton.Options;
				case GamepadButtonFlags.LeftThumb:
					return EInputButton.LeftThumb;
				case GamepadButtonFlags.RightThumb:
					return EInputButton.RightThumb;
				case GamepadButtonFlags.LeftShoulder:
					return EInputButton.LeftShoulder;
				case GamepadButtonFlags.RightShoulder:
					return EInputButton.RightShoulder;
				case GamepadButtonFlags.A:
					return EInputButton.ControllerActionDown;
				case GamepadButtonFlags.B:
					return EInputButton.ControllerActionRight;
				case GamepadButtonFlags.X:
					return EInputButton.ControllerActionLeft;
				case GamepadButtonFlags.Y:
					return EInputButton.ControllerActionUp;
			}

			return EInputButton.Unknown;
		}

		private static GamepadButtonFlags GetXInputFromInputButton(EInputButton inputButton)
		{
			switch (inputButton)
			{
				case EInputButton.DPadUp:
					return GamepadButtonFlags.DPadUp;
				case EInputButton.DPadDown:
					return GamepadButtonFlags.DPadDown;
				case EInputButton.DPadLeft:
					return GamepadButtonFlags.DPadLeft;
				case EInputButton.DPadRight:
					return GamepadButtonFlags.DPadRight;
				case EInputButton.Start:
					return GamepadButtonFlags.Start;
				case EInputButton.Options:
					return GamepadButtonFlags.Back;
				case EInputButton.LeftThumb:
					return GamepadButtonFlags.LeftThumb;
				case EInputButton.RightThumb:
					return GamepadButtonFlags.RightThumb;
				case EInputButton.LeftShoulder:
					return GamepadButtonFlags.LeftShoulder;
				case EInputButton.RightShoulder:
					return GamepadButtonFlags.RightShoulder;
				case EInputButton.ControllerActionUp:
					return GamepadButtonFlags.Y;
				case EInputButton.ControllerActionDown:
					return GamepadButtonFlags.A;
				case EInputButton.ControllerActionLeft:
					return GamepadButtonFlags.X;
				case EInputButton.ControllerActionRight:
					return GamepadButtonFlags.B;
			}

			return GamepadButtonFlags.None;
		}

		public static void SetCursorVisibility(bool bIsVisible)
		{
			if (CursorVisibilitySetter != null && m_bCursorVisible != bIsVisible)
			{
				m_bCursorVisible = bIsVisible;
				CursorVisibilitySetter(bIsVisible);
			}
		}

		private static DirectInput m_directInput;
		private static Keyboard m_keyboard;
		private static Mouse m_mouse;

		private static string m_textInputBuffer = "";
		private static readonly object m_textBufferMutex = new object();

		private static MouseState m_currentMouseState;
		private static KeyboardState m_currentKeyboardState;
		private static State m_currentGamepadState;
		private static readonly object m_inputStateMutex = new object();

		private static float[] m_nativeAxisValues = new float[(int)EInputAxis.Count];
		private static readonly object m_axisMutex = new object();

		private static Controller[] m_controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };

		private static readonly object m_inputMutex = new object();

		private static readonly SInputListenerCollection[] m_inputListeners = new SInputListenerCollection[(int)EInputClass.Count];
		private static readonly List<SInputListenerAction> m_iterationAdditions = new List<SInputListenerAction>(16);
		private static readonly List<SInputListenerAction> m_iterationRemovals = new List<SInputListenerAction>(16);
		private static readonly bool[] m_cachedInputClassStates = new bool[(int)EInputClass.Count];

		private static object m_listenerMutex = new object();

		private static bool m_bIsInitialized;
		private static volatile bool m_bIsIterating;
		private static IntPtr m_referenceHwnd;

		private static bool m_bCursorVisible = true;
		public static Action<bool> CursorVisibilitySetter { get; set; }
	}
}

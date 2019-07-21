using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using KlaxCore.KlaxScript;
using KlaxEditor.Utility;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace KlaxEditor.ViewModels.KlaxScript
{
	struct PinColor
	{
		public PinColor(SolidColorBrush outer, SolidColorBrush inner)
		{
			Inner = inner;
			Outer = outer;
		}

		public SolidColorBrush Inner;
		public SolidColorBrush Outer;
	}

	static class PinColorHelpers
	{
		static PinColorHelpers()
		{
			m_registry = CKlaxScriptRegistry.Instance;

			Color outerColor = EditorConversionUtility.ConvertEngineColorToSystem(CKlaxScriptRegistry.DEFAULT_TYPE_COLOR);
			Color innerColor = outerColor;
			outerColor.ScA = 0.2f;
			m_defaultTypeColor = new PinColor(new SolidColorBrush(innerColor), new SolidColorBrush(outerColor));

			m_execPinColor = new PinColor(new SolidColorBrush(Colors.White), new SolidColorBrush(new Color() { ScA = 0.2f, ScR = 1.0f, ScG = 1.0f, ScB = 1.0f }));
		}

		public static PinColor GetColorForType(Type type)
		{
			if (m_registry.TryGetTypeInfo(type, out CKlaxScriptTypeInfo outTypeInfo))
			{
				Color outerColor = EditorConversionUtility.ConvertEngineColorToSystem(outTypeInfo.Color);
				Color innerColor = outerColor;
				outerColor.ScA = 0.2f;

				return new PinColor(new SolidColorBrush(innerColor), new SolidColorBrush(outerColor));
			}

			return m_defaultTypeColor;
		}

		public static PinColor GetExecutionPinColor()
		{
			return m_execPinColor;
		}

		private static CKlaxScriptRegistry m_registry;
		private static PinColor m_defaultTypeColor;
		private static PinColor m_execPinColor;
	}

	abstract class CPinViewModel : CViewModelBase
	{
		public event Action<System.Windows.Point> ConnectionPointChanged;

		protected CPinViewModel(CScriptNodeViewmodel nodeViewModel, int pinIndex)
		{
			NodeViewModel = nodeViewModel;
			MouseDownCommand = new CRelayCommand(OnMouseDown);
			PinIndex = pinIndex;
		}

		public virtual void AddConnection(CNodeConnectionViewModel newConnection, CPinViewModel otherPin)
		{
			int prevConnectionCount = Connections.Count;
			Connections.Add(newConnection);
			if (prevConnectionCount == 0)
			{
				RaisePropertyChanged("PinInnerColor");
				RaisePropertyChanged("IsConnected");
			}
		}

		public virtual void RemoveConnection(CNodeConnectionViewModel removedConnection, CPinViewModel otherPin)
		{
			Connections.Remove(removedConnection);
			if (Connections.Count == 0)
			{
				RaisePropertyChanged("PinInnerColor");
				RaisePropertyChanged("IsConnected");
			}
		}

		public virtual void DisconnectAll()
		{
			List<CNodeConnectionViewModel> connectionCopy = new List<CNodeConnectionViewModel>(Connections);
			foreach (var entry in connectionCopy)
			{
				entry.Disconnect();
			}
		}

		public abstract bool CanConnect(CPinViewModel pinToCheck, out string failReason);
		public abstract bool IsExecutionPin();
		public abstract Type GetPinType();

		public virtual bool CanBeDragged() { return true; }

		/// <summary>
		/// Get the connection point of this node in canvas space
		/// </summary>
		/// <returns></returns>
		public Point GetConnectionPointCanvas()
		{
			return new Point(NodeViewModel.PosX + ConnectionPoint.X, NodeViewModel.PosY + ConnectionPoint.Y);
		}

		private void OnMouseDown(object e)
		{
			MouseButtonEventArgs args = (MouseButtonEventArgs)e;
			if (args.ChangedButton != MouseButton.Left || !CanBeDragged())
			{
				return;
			}

			args.Handled = true;

			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
			{
				for (int i = Connections.Count - 1; i >= 0; i--)
				{
					Connections[i].DisconnectWithUndo();
				}
			}
			else
			{
				NodeViewModel.NodeGraph.DraggedPin = this;
				MouseHook.OnMouseUp += OnMouseUp;
			}
		}

		private void OnMouseUp(object sender, Point p)
		{
			NodeViewModel.NodeGraph.DraggedPin = null;
			MouseHook.OnMouseUp -= OnMouseUp;
		}

		public List<CNodeConnectionViewModel> Connections { get; } = new List<CNodeConnectionViewModel>();

		private string m_name;
		public string Name
		{
			get { return m_name; }
			set { m_name = value; RaisePropertyChanged(); }
		}

		private string m_tooltip;
		public string Tooltip
		{
			get { return m_tooltip; }
			set { m_tooltip = value; RaisePropertyChanged(); }
		}

		public bool IsConnected
		{
			get { return Connections.Count > 0; }
		}

		private System.Windows.Point m_connectionPoint;
		public Point ConnectionPoint
		{
			get { return m_connectionPoint; }
			set { m_connectionPoint = value; ConnectionPointChanged?.Invoke(m_connectionPoint); }
		}

		protected PinColor m_pinColor;
		public SolidColorBrush PinOuterColor
		{
			get { return m_pinColor.Outer; }
			set { m_pinColor.Outer = value; RaisePropertyChanged(); }
		}
		public SolidColorBrush PinInnerColor
		{
			get { return Connections.Count > 0 ? m_pinColor.Outer : m_pinColor.Inner; }
			set { m_pinColor.Inner = value; RaisePropertyChanged(); }
		}
		public ICommand MouseDownCommand { get; set; }

		public CScriptNodeViewmodel NodeViewModel { get; private set; }
		public int PinIndex { get; private set; }
	}

	class CInputPinViewModel : CPinViewModel
	{
		public CInputPinViewModel(CInputPin inputPin, CScriptNodeViewmodel nodeViewModel, int pinIndex) : base(nodeViewModel, pinIndex)
		{
			m_pin = inputPin;
			Name = inputPin.Name;
			Tooltip = EditorKlaxScriptUtility.GetTypeName(inputPin.Type);
			m_pinColor = PinColorHelpers.GetColorForType(inputPin.Type);
			m_nodeViewmodel = nodeViewModel;
			m_literal = inputPin.Literal;
			m_valueType = inputPin.Type;
			m_bIsLiteralOnly = inputPin.bIsLiteralOnly;
		}

		public override void AddConnection(CNodeConnectionViewModel newConnection, CPinViewModel otherPin)
		{
			if (otherPin is COutputPinViewModel outputPin)
			{
				// Input pin can only be connected to a single node
				if (Connections.Count > 0)
				{
					if (Connections.Count > 1)
					{
						List<CNodeConnectionViewModel> oldConnections = new List<CNodeConnectionViewModel>();
						foreach (CNodeConnectionViewModel connection in Connections)
						{
							oldConnections.Add(connection);
						}

						foreach (CNodeConnectionViewModel connection in oldConnections)
						{
							connection.Disconnect();
						}
					}

					Connections[0].Disconnect();
				}

				m_pin.SourceNode = outputPin.NodeViewModel.ScriptNode;
				m_pin.SourceParameterIndex = outputPin.PinIndex;

				CNodeChangeContext context = new CNodeChangeContext();
				m_nodeViewmodel.ScriptNode.OnInputConnectionChanged(context, m_pin, outputPin.Pin);
				foreach (var action in context.Actions)
				{
					m_nodeViewmodel.ExecuteNodeAction(action);
				}

				base.AddConnection(newConnection, otherPin);
			}
		}

		public override void RemoveConnection(CNodeConnectionViewModel removedConnection, CPinViewModel otherPin)
		{
			m_pin.SourceNode = null;
			m_pin.SourceParameterIndex = -1;

			if (otherPin is COutputPinViewModel outputVm)
			{
				CNodeChangeContext context = new CNodeChangeContext();
				m_nodeViewmodel.ScriptNode.OnInputConnectionChanged(context, m_pin, null);
				foreach (var action in context.Actions)
				{
					m_nodeViewmodel.ExecuteNodeAction(action);
				}
			}

			base.RemoveConnection(removedConnection, otherPin);
		}

		public override bool CanConnect(CPinViewModel pinToCheck, out string failReason)
		{
			if (IsLiteralOnly)
			{
				failReason = "This pin cannot be connected to another pin";
				return false;
			}

			if (pinToCheck == null)
			{
				failReason = "Source not existing";
				return false;
			}

			if (pinToCheck.NodeViewModel == NodeViewModel)
			{
				failReason = "Cannot connect to a pin on the same node";
				return false;
			}

			if (pinToCheck is CInputPinViewModel)
			{
				failReason = "Input is not compatible with another input";
				return false;
			}

			if (pinToCheck is CExecutionPinViewModel)
			{
				failReason = "Input is not compatible with execution";
				return false;
			}

			if (pinToCheck is COutputPinViewModel outputPin)
			{
				if (!ValueType.IsAssignableFrom(outputPin.ValueType))
				{
					failReason = "Output of type " + EditorKlaxScriptUtility.GetTypeName(outputPin.ValueType) + " is not compatible with input of type " + EditorKlaxScriptUtility.GetTypeName(ValueType);
					return false;
				}
			}

			failReason = "";
			return true;
		}

		public override bool IsExecutionPin()
		{
			return false;
		}

		public override Type GetPinType()
		{
			return ValueType;
		}

		public override bool CanBeDragged()
		{
			return !IsLiteralOnly;
		}

		private object m_literal;
		public object Literal
		{
			get { return m_literal; }
			set
			{
				m_literal = value;
				m_pin.Literal = value;
				RaisePropertyChanged();

				CNodeChangeContext context = new CNodeChangeContext();
				m_nodeViewmodel.ScriptNode.OnInputLiteralChanged(context, m_pin);
				foreach (var action in context.Actions)
				{
					m_nodeViewmodel.ExecuteNodeAction(action);
				}
			}
		}

		private Type m_valueType;
		public Type ValueType
		{
			get { return m_valueType; }
			set
			{
				if (m_valueType == value)
				{
					return;
				}

				m_valueType = value;

				Tooltip = EditorKlaxScriptUtility.GetTypeName(value);
				m_pinColor = PinColorHelpers.GetColorForType(value);
				RaisePropertyChanged(nameof(PinInnerColor));
				RaisePropertyChanged(nameof(PinOuterColor));

				if (Connections.Count > 0)
				{
					List<CNodeConnectionViewModel> copyConnections = new List<CNodeConnectionViewModel>(Connections);
					foreach (var connection in copyConnections)
					{
						if (connection.IsValidConnection())
						{
							connection.UpdateStrokeColor();
						}
						else
						{
							connection.Disconnect();
						}
					}
				}

				RaisePropertyChanged();
			}
		}

		private bool m_bIsLiteralOnly;
		public bool IsLiteralOnly
		{
			get { return m_bIsLiteralOnly; }
			set { m_bIsLiteralOnly = value; RaisePropertyChanged(); }
		}

		public CInputPin Pin => m_pin;

		private CInputPin m_pin;
		private CScriptNodeViewmodel m_nodeViewmodel;
	}

	class COutputPinViewModel : CPinViewModel
	{
		public COutputPinViewModel(COutputPin outputPin, CScriptNodeViewmodel nodeViewModel, int pinIndex) : base(nodeViewModel, pinIndex)
		{
			m_pin = outputPin;
			Name = outputPin.Name;
			ValueType = outputPin.Type;
		}

		public override bool CanConnect(CPinViewModel pinToCheck, out string failReason)
		{
			if (pinToCheck == null)
			{
				failReason = "Source not existing";
				return false;
			}

			if (pinToCheck.NodeViewModel == NodeViewModel)
			{
				failReason = "Cannot connect to a pin on the same node";
				return false;
			}

			if (pinToCheck is COutputPinViewModel)
			{
				failReason = "Output is not compatible with another output";
				return false;
			}

			if (pinToCheck is CExecutionPinViewModel)
			{
				failReason = "Output is not compatible with execution";
				return false;
			}

			if (pinToCheck is CInputPinViewModel inputPin)
			{
				if (inputPin.IsLiteralOnly)
				{
					failReason = $"Input pin {inputPin.Name} can only be assigned a value by changing its literal value";
					return false;
				}

				if (!inputPin.ValueType.IsAssignableFrom(ValueType))
				{
					failReason = "Output of type " + EditorKlaxScriptUtility.GetTypeName(ValueType) + " is not compatible with input of type " + EditorKlaxScriptUtility.GetTypeName(inputPin.ValueType);
					return false;
				}
			}

			failReason = "";
			return true;
		}

		public override bool IsExecutionPin()
		{
			return false;
		}

		public override Type GetPinType()
		{
			return ValueType;
		}

		private Type m_valueType;
		public Type ValueType
		{
			get { return m_valueType; }
			set
			{
				if (m_valueType != value)
				{
					m_valueType = value;

					Tooltip = EditorKlaxScriptUtility.GetTypeName(value);
					m_pinColor = PinColorHelpers.GetColorForType(value);
					RaisePropertyChanged(nameof(PinInnerColor));
					RaisePropertyChanged(nameof(PinOuterColor));

					if (Connections.Count > 0)
					{
						List<CNodeConnectionViewModel> copyConnections = new List<CNodeConnectionViewModel>(Connections);
						foreach (var connection in copyConnections)
						{
							if (connection.IsValidConnection())
							{
								connection.UpdateStrokeColor();
							}
							else
							{
								connection.Disconnect();
							}
						}
					}

					RaisePropertyChanged();
				}
			}
		}

		public COutputPin Pin => m_pin;
		private COutputPin m_pin;
	}

	class CExecutionPinViewModel : CPinViewModel
	{
		public CExecutionPinViewModel(CExecutionPin executionPin, CScriptNodeViewmodel nodeViewModel, int pinIndex, bool isIn) : base(nodeViewModel, pinIndex)
		{
			m_pin = executionPin;
			Name = executionPin.Name;
			Tooltip = "Execution Path";
			m_pinColor = PinColorHelpers.GetExecutionPinColor();
			IsIn = isIn;
		}

		public override void AddConnection(CNodeConnectionViewModel newConnection, CPinViewModel otherPin)
		{
			if (!IsIn)
			{
				if (otherPin is CExecutionPinViewModel otherExecution && otherExecution.IsIn)
				{
					// Out Execution pins can only be connected to a single node
					if (Connections.Count > 0)
					{
						if (Connections.Count > 1)
						{
							List<CNodeConnectionViewModel> oldConnections = new List<CNodeConnectionViewModel>();
							foreach (CNodeConnectionViewModel connection in Connections)
							{
								oldConnections.Add(connection);
							}

							foreach (CNodeConnectionViewModel connection in oldConnections)
							{
								connection.Disconnect();
							}
						}

						Connections[0].Disconnect();
					}

					m_pin.TargetNode = otherPin.NodeViewModel.ScriptNode;
					m_pin.TargetPin = otherPin.NodeViewModel.ScriptNode.InExecutionPins[otherPin.PinIndex];
					m_pin.TargetPinIndex = otherPin.PinIndex;
					base.AddConnection(newConnection, otherPin);
				}
			}
			else
			{
				base.AddConnection(newConnection, otherPin);
			}
		}

		public override void RemoveConnection(CNodeConnectionViewModel removedConnection, CPinViewModel otherPin)
		{
			m_pin.TargetNode = null;
			base.RemoveConnection(removedConnection, otherPin);
		}

		public override bool CanConnect(CPinViewModel pinToCheck, out string failReason)
		{
			if (pinToCheck == null)
			{
				failReason = "Source not existing";
				return false;
			}

			if (pinToCheck.NodeViewModel == NodeViewModel)
			{
				failReason = "Cannot connect to a pin on the same node";
				return false;
			}

			if (pinToCheck is COutputPinViewModel)
			{
				failReason = "Execution is not compatible with output";
				return false;
			}

			if (pinToCheck is CInputPinViewModel)
			{
				failReason = "Execution is not compatible with input";
				return false;
			}

			if (pinToCheck is CExecutionPinViewModel otherExecution)
			{
				if (IsIn == otherExecution.IsIn)
				{
					failReason = "Cannot connect to an execution pin with the same direction";
					return false;
				}
			}

			failReason = "";
			return true;
		}

		public override bool IsExecutionPin()
		{
			return true;
		}

		public override Type GetPinType()
		{
			return null;
		}

		public bool IsIn { get; private set; }
		public CExecutionPin Pin => m_pin;

		private CExecutionPin m_pin;
	}

	class CSwitchExecutionPinViewModel : CExecutionPinViewModel
	{
		public CSwitchExecutionPinViewModel(CExecutionPin executionPin, CScriptNodeViewmodel nodeViewModel, int pinIndex, bool isIn, Type controlType, object controlValue)
			: base(executionPin, nodeViewModel, pinIndex, isIn)
		{
			m_controlType = controlType;
			m_controlValue = controlValue;
		}

		private Type m_controlType;
		public Type ControlType
		{
			get { return m_controlType; }
			set { m_controlType = value; RaisePropertyChanged(); }
		}

		private object m_controlValue;
		public object ControlValue
		{
			get { return m_controlValue; }
			set
			{
				m_controlValue = value;

				(Pin as CSwitchExecutionPin).Value = value;
				RaisePropertyChanged();
			}
		}
	}
}

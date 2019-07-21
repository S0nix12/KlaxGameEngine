using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KlaxCore.KlaxScript;
using KlaxEditor.Utility.UndoRedo;
using SharpDX;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace KlaxEditor.ViewModels.KlaxScript
{
	class CScriptNodeViewmodel : CViewModelBase
	{
		public event Action OnNodeMoved;

		public CScriptNodeViewmodel(CNode scriptNode, CNodeGraphViewModel graphViewModel)
		{
			ScriptNode = scriptNode;
			NodeGraph = graphViewModel;
			Name = scriptNode.Name;

			ScriptNode.NodeRebuilt += OnNodeRebuilt;

			m_bCanAddInputPins = scriptNode.CanAddInputPins;
			m_bCanAddOutputPins = scriptNode.CanAddOutputPins;

			m_bIsUpdatingNode = true;
			PosX = scriptNode.NodePosX;
			PosY = scriptNode.NodePosY;
			for (var i = 0; i < scriptNode.InputPins.Count; i++)
			{
				m_inputPins.Add(new CInputPinViewModel(scriptNode.InputPins[i], this, i));
			}

			for (var i = 0; i < scriptNode.OutputPins.Count; i++)
			{
				m_outputPins.Add(new COutputPinViewModel(scriptNode.OutputPins[i], this, i));
			}

			for (var i = 0; i < scriptNode.InExecutionPins.Count; i++)
			{
				m_inExecutionPins.Add(new CExecutionPinViewModel(scriptNode.InExecutionPins[i], this, i, true));
			}

			for (var i = 0; i < scriptNode.OutExecutionPins.Count; i++)
			{
				m_outExecutionPins.Add(GetOutExecutionPinViewModel(scriptNode.OutExecutionPins[i], i));
			}
			m_bIsUpdatingNode = false;

			MouseDownCommand = new CRelayCommand(OnMouseDown);
			AddInputPinCommand = new CRelayCommand(OnAddInputPin);
			AddOutputPinCommand = new CRelayCommand(OnAddOutputPin);
		}

		public void RemoveNodeListener()
		{
			ScriptNode.NodeRebuilt -= OnNodeRebuilt;
		}

		private CExecutionPinViewModel GetOutExecutionPinViewModel(CExecutionPin pin, int pinIndex)
		{
			if (pin is CSwitchExecutionPin switchPin)
			{
				return new CSwitchExecutionPinViewModel(pin, this, pinIndex, false, switchPin.Type, switchPin.Value);
			}
			else
			{
				return new CExecutionPinViewModel(pin, this, pinIndex, false);
			}
		}

		public void OnNodeRebuilt()
		{
			if (m_inputPins.Count <= ScriptNode.InputPins.Count)
			{
				for (int i = 0; i < ScriptNode.InputPins.Count; i++)
				{
					CInputPin scriptInput = ScriptNode.InputPins[i];
					if (m_inputPins.Count > i)
					{
						CInputPinViewModel inputViewModel = m_inputPins[i];
						inputViewModel.Name = scriptInput.Name;
						if (inputViewModel.ValueType != scriptInput.Type)
						{
							inputViewModel.ValueType = scriptInput.Type;
							inputViewModel.Literal = scriptInput.Type.IsValueType ? Activator.CreateInstance(scriptInput.Type) : null;
						}
					}
					else
					{
						m_inputPins.Add(new CInputPinViewModel(scriptInput, this, i));
					}
				}
			}
			else
			{
				for (int i = m_inputPins.Count - 1; i >= 0; i--)
				{
					if (ScriptNode.InputPins.Count > i)
					{
						CInputPin scriptInput = ScriptNode.InputPins[i];
						CInputPinViewModel inputViewModel = m_inputPins[i];
						inputViewModel.Name = scriptInput.Name;
						if (inputViewModel.ValueType != scriptInput.Type)
						{
							inputViewModel.ValueType = scriptInput.Type;
							inputViewModel.Literal = scriptInput.Type.IsValueType ? Activator.CreateInstance(scriptInput.Type) : null;
						}
					}
					else
					{
						m_inputPins[i].DisconnectAll();
						m_inputPins.RemoveAt(i);
					}
				}
			}

			if (m_outputPins.Count <= ScriptNode.OutputPins.Count)
			{
				for (int i = 0; i < ScriptNode.OutputPins.Count; i++)
				{
					COutputPin scriptOutput = ScriptNode.OutputPins[i];
					if (m_outputPins.Count > i)
					{
						COutputPinViewModel outputViewModel = m_outputPins[i];
						outputViewModel.Name = scriptOutput.Name;
						outputViewModel.ValueType = scriptOutput.Type;
					}
					else
					{
						m_outputPins.Add(new COutputPinViewModel(scriptOutput, this, i));
					}
				}
			}
			else
			{
				for (int i = m_outputPins.Count - 1; i >= 0; i--)
				{
					if (ScriptNode.OutputPins.Count > i)
					{
						COutputPin scriptOutput = ScriptNode.OutputPins[i];
						COutputPinViewModel outputViewModel = m_outputPins[i];
						outputViewModel.Name = scriptOutput.Name;
						outputViewModel.ValueType = scriptOutput.Type;
					}
					else
					{
						m_outputPins[i].DisconnectAll();
						m_outputPins.RemoveAt(i);
					}
				}
			}
		}

		public void UpdateNode()
		{
			if (m_bIsUpdatingNode)
				return;

			m_bIsUpdatingNode = true;
			m_inputPins.Clear();
			m_outputPins.Clear();
			m_inExecutionPins.Clear();
			m_outExecutionPins.Clear();

			for (var i = 0; i < ScriptNode.InputPins.Count; i++)
			{
				m_inputPins.Add(new CInputPinViewModel(ScriptNode.InputPins[i], this, i));
			}

			for (var i = 0; i < ScriptNode.OutputPins.Count; i++)
			{
				m_outputPins.Add(new COutputPinViewModel(ScriptNode.OutputPins[i], this, i));
			}

			for (var i = 0; i < ScriptNode.InExecutionPins.Count; i++)
			{
				m_inExecutionPins.Add(new CExecutionPinViewModel(ScriptNode.InExecutionPins[i], this, i, true));
			}

			for (var i = 0; i < ScriptNode.OutExecutionPins.Count; i++)
			{
				m_outExecutionPins.Add(new CExecutionPinViewModel(ScriptNode.OutExecutionPins[i], this, i, false));
			}

			Name = ScriptNode.Name;
			m_bIsUpdatingNode = false;
		}

		public void GetConnections(List<CNodeConnectionViewModel> outConnections)
		{
			foreach (var executionPin in InExecutionPins)
			{
				outConnections.AddRange(executionPin.Connections);
			}

			foreach (var inputPin in InputPins)
			{
				outConnections.AddRange(inputPin.Connections);
			}

			foreach (var executionPin in OutExecutionPins)
			{
				outConnections.AddRange(executionPin.Connections);
			}

			foreach (var outputPin in OutputPins)
			{
				outConnections.AddRange(outputPin.Connections);
			}
		}

		public CPinViewModel GetPossibleTargetPin(CPinViewModel source)
		{
			foreach (var executionPin in InExecutionPins)
			{
				if (executionPin.CanConnect(source, out string failReason))
				{
					return executionPin;
				}
			}

			foreach (var executionPin in OutExecutionPins)
			{
				if (executionPin.CanConnect(source, out string failReason))
				{
					return executionPin;
				}
			}

			foreach (var inputPin in InputPins)
			{
				if (inputPin.CanConnect(source, out string failReason))
				{
					return inputPin;
				}
			}

			foreach (var outputPin in OutputPins)
			{
				if (outputPin.CanConnect(source, out string failReason))
				{
					return outputPin;
				}
			}

			return null;
		}

		internal CScriptNodeViewmodel()
		{
			Name = "SampleNode";
			MouseDownCommand = new CRelayCommand(OnMouseDown);
		}

		public void StartMove()
		{
			m_bIsDragging = true;
			PosXStart = PosX;
			PosYStart = PosY;
		}

		public void Move(in Vector delta)
		{
			if (!m_bIsDragging)
			{
				return;
			}

			PosX = PosXStart + delta.X;
			PosY = PosYStart + delta.Y;
			OnNodeMoved?.Invoke();
		}

		public void MoveTo(in Point newPos)
		{
			PosX = newPos.X;
			PosY = newPos.Y;
			OnNodeMoved?.Invoke();
		}

		public void StopMove()
		{
			m_bIsDragging = false;
		}


		private void OnMouseDown(object e)
		{
			MouseButtonEventArgs args = (MouseButtonEventArgs)e;
			if (args.ChangedButton != MouseButton.Left)
				return;

			args.Handled = true;

			NodeGraph.NodeClicked(this);
		}

		private void OnAddInputPin(object e)
		{
			CNodeChangeContext context = new CNodeChangeContext();
			ScriptNode.OnAddInputPinButtonClicked(context);
			foreach (var action in context.Actions)
			{
				ExecuteNodeAction(action);
			}
		}

		private void OnAddOutputPin(object e)
		{
			CNodeChangeContext context = new CNodeChangeContext();
			ScriptNode.OnAddOutputPinButtonClicked(context);
			foreach (var action in context.Actions)
			{
				ExecuteNodeAction(action);
			}
		}

		public void ExecuteNodeAction(CNodeAction abstractAction)
		{
			switch (abstractAction.ActionType)
			{
				case ENodeAction.PinTypeChange:
					{
						CPinTypeChangeAction action = (CPinTypeChangeAction)abstractAction;
						if (action.Pin is CInputPin input)
						{
							foreach (var inputPinViewModel in InputPins)
							{
								if (inputPinViewModel.Pin == input)
								{
									inputPinViewModel.ValueType = action.NewType;
									break;
								}
							}
						}
						else if (action.Pin is COutputPin output)
						{
							foreach (var outputPinViewModel in OutputPins)
							{
								if (outputPinViewModel.Pin == output)
								{
									outputPinViewModel.ValueType = action.NewType;
									break;
								}
							}
						}
					}
					break;
				case ENodeAction.PinNameChange:
					{
						CPinNameChangeAction action = (CPinNameChangeAction)abstractAction;
						if (action.Pin is CInputPin input)
						{
							foreach (var inputPinViewModel in InputPins)
							{
								if (inputPinViewModel.Pin == input)
								{
									inputPinViewModel.Name = action.NewName;
									break;
								}
							}
						}
						else if (action.Pin is COutputPin output)
						{
							foreach (var outputPinViewModel in OutputPins)
							{
								if (outputPinViewModel.Pin == output)
								{
									outputPinViewModel.Name = action.NewName;
									break;
								}
							}
						}
					}
					break;
				case ENodeAction.NodeNameChange:
					{
						CNodeNameChangeAction action = (CNodeNameChangeAction)abstractAction;
						Name = action.NewName;
					}
					break;
				case ENodeAction.SwitchNodeTypeChange:
					{
						CSwitchNodeTypeChangeAction action = (CSwitchNodeTypeChangeAction)abstractAction;

						for (int i = OutExecutionPins.Count - 1; i >= 1; i--)
						{
							OutExecutionPins[i].DisconnectAll();
							OutExecutionPins.RemoveAt(i);
						}
					}
					break;
				case ENodeAction.AddPinChange:
					{
						CAddPinChangeAction action = (CAddPinChangeAction)abstractAction;
						if (action.Pin is CExecutionPin execPin)
						{
							if (action.IsIn)
							{
								InExecutionPins.Insert(action.Index, new CExecutionPinViewModel(execPin, this, action.Index, true));
							}
							else
							{
								OutExecutionPins.Insert(action.Index, GetOutExecutionPinViewModel(execPin, action.Index));
							}
							//todo valentin: Clean up pin index in viewmodels when pin is inserted in place other than the last one
						}
						else if (action.Pin is COutputPin outPin)
						{
							OutputPins.Insert(action.Index, new COutputPinViewModel(outPin, this, action.Index));
						}
						else if (action.Pin is CInputPin inPin)
						{
							InputPins.Insert(action.Index, new CInputPinViewModel(inPin, this, action.Index));
						}
					}
					break;
			}
		}

		private string m_name;
		public string Name
		{
			get { return m_name; }
			set { m_name = value; RaisePropertyChanged(); }
		}

		private ObservableCollection<CInputPinViewModel> m_inputPins = new ObservableCollection<CInputPinViewModel>();
		public ObservableCollection<CInputPinViewModel> InputPins
		{
			get { return m_inputPins; }
			set { m_inputPins = value; RaisePropertyChanged(); }
		}

		private ObservableCollection<COutputPinViewModel> m_outputPins = new ObservableCollection<COutputPinViewModel>();
		public ObservableCollection<COutputPinViewModel> OutputPins
		{
			get { return m_outputPins; }
			set { m_outputPins = value; RaisePropertyChanged(); }
		}

		private ObservableCollection<CExecutionPinViewModel> m_inExecutionPins = new ObservableCollection<CExecutionPinViewModel>();
		public ObservableCollection<CExecutionPinViewModel> InExecutionPins
		{
			get { return m_inExecutionPins; }
			set { m_inExecutionPins = value; RaisePropertyChanged(); }
		}

		private ObservableCollection<CExecutionPinViewModel> m_outExecutionPins = new ObservableCollection<CExecutionPinViewModel>();
		public ObservableCollection<CExecutionPinViewModel> OutExecutionPins
		{
			get { return m_outExecutionPins; }
			set { m_outExecutionPins = value; RaisePropertyChanged(); }
		}

		private double m_posX;
		public double PosX
		{
			get { return m_posX; }
			set
			{
				m_posX = value;
				ScriptNode.NodePosX = value;
				RaisePropertyChanged();
			}
		}

		private double m_posY;
		public double PosY
		{
			get { return m_posY; }
			set
			{
				m_posY = value;
				ScriptNode.NodePosY = value;
				RaisePropertyChanged();
			}
		}

		private bool m_isSelected;
		public bool IsSelected
		{
			get { return m_isSelected; }
			set { m_isSelected = value; RaisePropertyChanged(); }
		}

		private bool m_bCanAddOutputPins;
		public bool CanAddOutputPins
		{
			get { return m_bCanAddOutputPins; }
			set { m_bCanAddOutputPins = value; RaisePropertyChanged(); }
		}

		private bool m_bCanAddInputPins;
		public bool CanAddInputPins
		{
			get { return m_bCanAddInputPins; }
			set { m_bCanAddInputPins = value; RaisePropertyChanged(); }
		}

		public double PosYStart { get; private set; }
		public double PosXStart { get; private set; }

		public CNode ScriptNode { get; }

		private bool m_bIsDragging;
		private bool m_bIsUpdatingNode;

		public ICommand MouseDownCommand { get; set; }
		public ICommand AddInputPinCommand { get; private set; }
		public ICommand AddOutputPinCommand { get; private set; }

		internal CNodeGraphViewModel NodeGraph { get; private set; }
	}
}

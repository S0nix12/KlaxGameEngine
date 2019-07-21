using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KlaxCore.GameFramework;
using KlaxCore.KlaxScript;
using KlaxCore.KlaxScript.Nodes;
using KlaxCore.KlaxScript.Serialization;
using KlaxCore.Physics.Components;
using KlaxEditor.Utility;
using KlaxEditor.Utility.UndoRedo;
using KlaxEditor.ViewModels.EditorWindows;
using KlaxEditor.Views.KlaxScript;
using KlaxRenderer.Graphics;
using KlaxShared.Definitions;

namespace KlaxEditor.ViewModels.KlaxScript
{
	class CNodeConnectionViewModel : CViewModelBase
	{
		public CNodeConnectionViewModel(CPinViewModel sourcePin, CPinViewModel targetPin, CNodeGraphViewModel nodeGraph)
		{
			MouseDownCommand = new CRelayCommand(OnMouseDown);

			m_nodeGraph = nodeGraph;
			m_sourceNode = sourcePin.NodeViewModel;
			m_targetNode = targetPin.NodeViewModel;
			m_sourcePin = sourcePin;
			m_targetPin = targetPin;
			m_sourceNode.OnNodeMoved += OnSourceMoved;
			m_sourcePin.ConnectionPointChanged += OnSourcePinChanged;
			m_targetNode.OnNodeMoved += OnTargetMoved;
			m_targetPin.ConnectionPointChanged += OnTargetPinChanged;

			StrokeColor = m_sourcePin.PinOuterColor;
		}

		public void Connect()
		{
			CreatePath();
			m_sourcePin.AddConnection(this, m_targetPin);
			m_targetPin.AddConnection(this, m_sourcePin);
			m_nodeGraph.Connections.Add(this);
		}

		public void Disconnect()
		{
			m_sourcePin.RemoveConnection(this, m_targetPin);
			m_targetPin.RemoveConnection(this, m_sourcePin);
			m_nodeGraph.RemoveConnection(this);
		}

		public bool IsValidConnection()
		{
			if (m_sourcePin == null || m_targetPin == null)
			{
				return false;
			}

			return m_sourcePin.CanConnect(m_targetPin, out string outFailReason);
		}

		public void DisconnectWithUndo()
		{
			void Redo()
			{
				Disconnect();
			}

			void Undo()
			{
				Connect();
			}

			Redo();
			UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
		}

		public void UpdateStrokeColor()
		{
			StrokeColor = m_sourcePin.PinOuterColor;
		}

		private void CreatePath()
		{
			StartPoint = new Point(m_sourceNode.PosX + m_sourcePin.ConnectionPoint.X, m_sourceNode.PosY + m_sourcePin.ConnectionPoint.Y);
			EndPoint = new Point(m_targetNode.PosX + m_targetPin.ConnectionPoint.X, m_targetNode.PosY + m_targetPin.ConnectionPoint.Y);
		}

		private void OnSourceMoved()
		{
			CreatePath();
		}

		private void OnSourcePinChanged(Point newConnectionPoint)
		{
			CreatePath();
		}

		private void OnTargetMoved()
		{
			CreatePath();
		}
		private void OnTargetPinChanged(Point newConnectionPoint)
		{
			CreatePath();
		}

		private void OnMouseDown(object e)
		{
			if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) return;

			DisconnectWithUndo();
		}

		private readonly CNodeGraphViewModel m_nodeGraph;
		private readonly CScriptNodeViewmodel m_sourceNode;
		private readonly CScriptNodeViewmodel m_targetNode;
		private readonly CPinViewModel m_sourcePin;
		private readonly CPinViewModel m_targetPin;

		private Point m_startPoint;
		public Point StartPoint
		{
			get { return m_startPoint; }
			set { m_startPoint = value; RaisePropertyChanged(); }
		}

		private Point m_endPoint;
		public Point EndPoint
		{
			get { return m_endPoint; }
			set { m_endPoint = value; RaisePropertyChanged(); }
		}

		private SolidColorBrush m_strokeColor;
		public SolidColorBrush StrokeColor
		{
			get { return m_strokeColor; }
			set { m_strokeColor = value; RaisePropertyChanged(); }
		}

		public ICommand MouseDownCommand { get; set; }
	}

	class CPreviewConnectionViewModel : CViewModelBase
	{
		private bool m_isVisible;
		public bool IsVisible
		{
			get { return m_isVisible; }
			set { m_isVisible = value; RaisePropertyChanged(); }
		}

		private Point m_startPoint;
		public Point StartPoint
		{
			get { return m_startPoint; }
			set { m_startPoint = value; RaisePropertyChanged(); }
		}

		private Point m_endPoint;
		public Point EndPoint
		{
			get { return m_endPoint; }
			set { m_endPoint = value; RaisePropertyChanged(); }
		}

		private bool m_isValidDrop;
		public bool IsValidDrop
		{
			get { return m_isValidDrop; }
			set { m_isValidDrop = value; RaisePropertyChanged(); }
		}

		private string m_errorMessage;
		public string ErrorMessage
		{
			get { return m_errorMessage; }
			set { m_errorMessage = value; RaisePropertyChanged(); }
		}

		private SolidColorBrush m_strokeColor;
		public SolidColorBrush StrokeColor
		{
			get { return m_strokeColor; }
			set { m_strokeColor = value; RaisePropertyChanged(); }
		}

		public CPinViewModel DropTarget { get; set; }
	}

	class CVariableDragPopupViewModel : CViewModelBase
	{
		public CVariableDragPopupViewModel(CNodeGraphViewModel nodeGraphViewModel)
		{
			AddSetVariableNodeCommand = new CRelayCommand(OnAddSetVariableNode);
			AddGetVariableNodeCommand = new CRelayCommand(OnAddGetVariableNode);

			m_nodeGraphViewModel = nodeGraphViewModel;
		}

		private void OnAddSetVariableNode(object argument)
		{
			CSetKlaxVariableNode newNode = new CSetKlaxVariableNode(m_variable.Variable);
			newNode.NodePosX = Position.X;
			newNode.NodePosY = Position.Y;
			m_nodeGraphViewModel.AddNode(newNode, true);

			IsVisible = false;
		}

		private void OnAddGetVariableNode(object argument)
		{
			CGetKlaxVariableNode newNode = new CGetKlaxVariableNode(m_variable.Variable);
			newNode.NodePosX = Position.X;
			newNode.NodePosY = Position.Y;
			m_nodeGraphViewModel.AddNode(newNode, true);

			IsVisible = false;
		}

		private bool m_bIsVisible;
		public bool IsVisible
		{
			get { return m_bIsVisible; }
			set { m_bIsVisible = value; RaisePropertyChanged(); }
		}

		private Point m_position;
		public Point Position
		{
			get { return m_position; }
			set { m_position = value; RaisePropertyChanged(); }
		}

		private CEntityVariableViewModel m_variable;
		public CEntityVariableViewModel Variable
		{
			get { return m_variable; }
			set
			{
				m_variable = value;
				GetButtonText = $"Get {value.Variable.Name}";
				SetButtonText = $"Set {value.Variable.Name}";

				RaisePropertyChanged();
			}
		}

		private string m_getButtonText;
		public string GetButtonText
		{
			get { return m_getButtonText; }
			set { m_getButtonText = value; RaisePropertyChanged(); }
		}

		private string m_setButtonText;
		public string SetButtonText
		{
			get { return m_setButtonText; }
			set { m_setButtonText = value; RaisePropertyChanged(); }
		}

		public ICommand AddSetVariableNodeCommand { get; }
		public ICommand AddGetVariableNodeCommand { get; }

		private CNodeGraphViewModel m_nodeGraphViewModel;
	}

	class CNodeGraphViewModel : CEditorWindowViewModel
	{
		public CNodeGraphViewModel() : base("NodeGraph")
		{
			SetIconSourcePath("Resources/Images/Tabs/assetbrowser.png");

			Content = new NodeGraphView();
			MouseDownCommand = new CRelayCommand(OnMouseDown);
			PreviewMouseDownCommand = new CRelayCommand(OnPreviewMouseDown);
			PreviewMouseUpCommand = new CRelayCommand(OnPreviewMouseUp);
			StartDragContentCommand = new CRelayCommand(OnStartDragContent);
			DeleteNodesCommand = new CRelayCommand(OnDeleteCommand);
			CopyNodesCommand = new CRelayCommand(OnCopyNodes);
			PasteNodesCommand = new CRelayCommand(OnPasteNodes);
			CutNodesCommand = new CRelayCommand(OnCutNodes);
			DuplicateNodesCommand = new CRelayCommand(OnDuplicateNodes);
			DragEnterCommand = new CRelayCommand(OnDragEnter);
			DragOverCommand = new CRelayCommand(OnDragOver);
			DropCommand = new CRelayCommand(OnDrop);

			CreateNewGraph();

			m_lodLevels.Add(1.0);
			m_lodLevels.Add(0.7);
			m_lodLevels.Add(0.5);
			m_lodLevels.Add(0.3);
			m_lodLevels.Add(0.1);

			AddNodeViewModel = new CAddNodeViewModel(CKlaxScriptNodeQueryContext.Empty);
			AddNodeViewModel.NodeSelected += OnNodeAddNodePopupSelected;

			VariablePopup = new CVariableDragPopupViewModel(this);
		}

		#region CopyPaste		
		private void OnCutNodes(object argument)
		{
			string serializedNodes = SerializeSelectedNodes();

			if (string.IsNullOrWhiteSpace(serializedNodes))
			{
				return;
			}

			DataObject data = new DataObject();
			data.SetData("KlaxScriptNodes", serializedNodes);
			data.SetText(serializedNodes);
			Clipboard.SetDataObject(data);
			DeleteSelectedNodes(m_selectedNodes);
		}

		private void OnPasteNodes(object argument)
		{
			if (ScriptGraph == null)
			{
				return;
			}

			if (Clipboard.ContainsData("KlaxScriptNodes"))
			{
				string nodeData = (string)Clipboard.GetData("KlaxScriptNodes");
				List<CNode> newNodes = CScriptSerializer.Instance.DeserializeObject<List<CNode>>(nodeData);
				ScriptGraph.ResolveNodesForPaste(newNodes);

				NodeGraphView graphView = (NodeGraphView)Content;
				AddNodesToGraph(newNodes, graphView.GetPasteReferenceLocation(), true);
			}
			else if (Clipboard.ContainsData(DataFormats.UnicodeText))
			{
				string data = (string) Clipboard.GetData(DataFormats.UnicodeText);
				try
				{
					List<CNode> newNodes = CScriptSerializer.Instance.DeserializeObject<List<CNode>>(data);
					ScriptGraph.ResolveNodesForPaste(newNodes);

					NodeGraphView graphView = (NodeGraphView)Content;
					AddNodesToGraph(newNodes, graphView.GetPasteReferenceLocation(), true);
				}
				catch
				{
					LogUtility.Log("Paste failed, input text was not in the correct format");
				}
			}
		}

		private void OnCopyNodes(object argument)
		{
			string serializedNodes = SerializeSelectedNodes();

			if (string.IsNullOrWhiteSpace(serializedNodes))
			{
				return;
			}

			DataObject data = new DataObject();
			data.SetData("KlaxScriptNodes", serializedNodes);
			data.SetText(serializedNodes);
			Clipboard.SetDataObject(data);
		}

		private void OnDuplicateNodes(object argument)
		{
			OnCopyNodes(argument);
			OnPasteNodes(argument);
		}

		private string SerializeSelectedNodes()
		{
			if (m_selectedNodes.Count <= 0)
			{
				return "";
			}

			List<CNode> nodesToSerialize = new List<CNode>(m_selectedNodes.Count);
			foreach (CScriptNodeViewmodel nodeViewmodel in m_selectedNodes)
			{
				if (nodeViewmodel.ScriptNode.AllowCopy)
				{
					nodesToSerialize.Add(nodeViewmodel.ScriptNode);
				}
			}

			ScriptGraph.PrepareNodesForCopy(nodesToSerialize);
			return CScriptSerializer.Instance.Serialize(nodesToSerialize);
		}
		#endregion
		
		#region DragDrop
		private void OnDragEnter(object e)
		{
			DragEventArgs args = (DragEventArgs)e;
			args.Effects = DragDropEffects.None;
			if (ScriptGraph != null && 
				(args.Data.GetDataPresent("klaxVariableEntry") || args.Data.GetDataPresent("builderSceneComponent") || args.Data.GetDataPresent("builderEntityComponent")))
			{
				args.Effects = DragDropEffects.Link;
			}

			args.Handled = true;
		}

		private void OnDragOver(object e)
		{
			DragEventArgs args = (DragEventArgs)e;
			args.Effects = DragDropEffects.None;
			if (ScriptGraph != null && 
				(args.Data.GetDataPresent("klaxVariableEntry") || args.Data.GetDataPresent("builderSceneComponent") || args.Data.GetDataPresent("builderEntityComponent")))
			{
				args.Effects = DragDropEffects.Link;
			}

			args.Handled = true;
		}

		private void OnDrop(object e)
		{
			if (ScriptGraph == null)
			{
				return;
			}

			DragEventArgs args = (DragEventArgs)e;
			args.Effects = DragDropEffects.None;
			List<CNode> newNodes = new List<CNode>();

			if (args.Data.GetDataPresent("klaxVariableEntry"))
			{
				CEntityVariableViewModel variableEntry = (CEntityVariableViewModel) args.Data.GetData("klaxVariableEntry");
				NodeGraphView graphView = (NodeGraphView)Content;

				if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
				{
					newNodes.Add(new CGetKlaxVariableNode(variableEntry.Variable));
				}
				else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
				{
					newNodes.Add(new CSetKlaxVariableNode(variableEntry.Variable));
				}
				else
				{
					VariablePopup.IsVisible = true;
					VariablePopup.Position = graphView.GetPasteReferenceLocation();
					VariablePopup.Variable = variableEntry;
				}
			}
			else if(args.Data.GetDataPresent("builderSceneComponent"))
			{
				CInspectorSceneComponentViewModel sceneComponentViewModel = (CInspectorSceneComponentViewModel) args.Data.GetData("builderSceneComponent");
				newNodes.Add(new CComponentVariableNode(sceneComponentViewModel.ComponentId.GetComponent()));
			}
			else if(args.Data.GetDataPresent("builderEntityComponent"))
			{
				CInspectorEntityComponentViewModel entityComponentViewModel = (CInspectorEntityComponentViewModel) args.Data.GetData("builderEntityComponent");
				newNodes.Add(new CComponentVariableNode(entityComponentViewModel.ComponentId.GetComponent()));
			}

			if (newNodes.Count > 0)
			{
				NodeGraphView graphView = (NodeGraphView)Content;
				AddNodesToGraph(newNodes, graphView.GetPasteReferenceLocation(), true);
			}
			args.Handled = true;
		}
		#endregion

		public void CreateNewGraph()
		{
			Connections.Clear();
			Nodes.Clear();

			CGraph oldGraph = ScriptGraph;
			ScriptGraph = new CGraph();

			OnGraphChanged?.Invoke(this, oldGraph, ScriptGraph);
		}

		private void AddNodesToGraph(IList<CNode> nodes, Point referencePoint, bool bSelectNewNodes, bool bRecordUndo = true)
		{
			Dictionary<CNode, CScriptNodeViewmodel> nodeToViewModel = new Dictionary<CNode, CScriptNodeViewmodel>();
			List<CScriptNodeViewmodel> addedNodes = new List<CScriptNodeViewmodel>(nodes.Count());
			Point nodesMin = new Point(double.MaxValue, double.MaxValue);
			Point nodesMax = new Point(double.MinValue, double.MinValue);

			foreach (CNode scriptNode in nodes)
			{
				CScriptNodeViewmodel viewmodel = AddNode(scriptNode, false);
				nodeToViewModel.Add(scriptNode, viewmodel);
				addedNodes.Add(viewmodel);

				if (viewmodel.PosX < nodesMin.X) nodesMin.X = viewmodel.PosX;

				if (viewmodel.PosY < nodesMin.Y) nodesMin.Y = viewmodel.PosY;

				if (viewmodel.PosX > nodesMax.X) nodesMax.X = viewmodel.PosX;

				if (viewmodel.PosY > nodesMax.Y) nodesMax.Y = viewmodel.PosY;
			}

			Point nodesMid = new Point((nodesMin.X + nodesMax.X) / 2, (nodesMin.Y + nodesMax.Y) / 2);
			foreach (CScriptNodeViewmodel nodeViewmodel in addedNodes)
			{
				Vector toMid = new Point(nodeViewmodel.PosX, nodeViewmodel.PosY) - nodesMid;
				Point newPos = referencePoint + toMid;
				nodeViewmodel.PosX = newPos.X;
				nodeViewmodel.PosY = newPos.Y;
			}

			if (bSelectNewNodes)
			{
				SelectNodes(addedNodes);
			}

			ResolveScriptNodeConnections(nodes, nodeToViewModel);

			if (bRecordUndo)
			{
				void Redo()
				{
					foreach (CScriptNodeViewmodel nodeViewmodel in addedNodes)
					{
						Nodes.Add(nodeViewmodel);
						ScriptGraph.m_nodes.Add(nodeViewmodel.ScriptNode);

						Vector toMid = new Point(nodeViewmodel.PosX, nodeViewmodel.PosY) - nodesMid;
						Point newPos = referencePoint + toMid;
						nodeViewmodel.PosX = newPos.X;
						nodeViewmodel.PosY = newPos.Y;

						if (bSelectNewNodes)
						{
							SelectNodes(addedNodes);
						}

						ResolveScriptNodeConnections(nodes, nodeToViewModel);
					}
				}

				void Undo()
				{
					DeleteSelectedNodes(addedNodes, false);
				}

				UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
			}
		}

		private void ResolveScriptNodeConnections(IEnumerable<CNode> nodes, Dictionary<CNode, CScriptNodeViewmodel> nodeToViewModel)
		{
			foreach (CNode graphNode in nodes)
			{
				CScriptNodeViewmodel sourceViewModel = nodeToViewModel[graphNode];
				for (int i = 0; i < graphNode.OutExecutionPins.Count; i++)
				{
					CExecutionPin inExecutionPin = graphNode.OutExecutionPins[i];
					if (inExecutionPin.TargetNode == null)
					{
						continue;
					}

					CScriptNodeViewmodel targetViewModel = nodeToViewModel[inExecutionPin.TargetNode];
					if (targetViewModel.InExecutionPins.Count > 0)
					{
						CNodeConnectionViewModel newConnection = new CNodeConnectionViewModel(sourceViewModel.OutExecutionPins[i], targetViewModel.InExecutionPins[inExecutionPin.TargetPinIndex], this);
						newConnection.Connect();
					}
					else
					{
						inExecutionPin.TargetNode = null;
						LogUtility.Log("[ScriptLoadError] node {0} tried to connect to {1} but the target does not have an in execution pin", sourceViewModel.Name, targetViewModel.Name);
					}
				}

				for (int i = 0; i < graphNode.InputPins.Count; i++)
				{
					CInputPin inputPin = graphNode.InputPins[i];
					if (inputPin.SourceNode == null)
					{
						continue;
					}

					CScriptNodeViewmodel inputSourceVm = nodeToViewModel[inputPin.SourceNode];
					if (inputSourceVm.OutputPins.Count > inputPin.SourceParameterIndex)
					{
						COutputPinViewModel sourcePinVM = inputSourceVm.OutputPins[inputPin.SourceParameterIndex];
						if (inputPin.Type.IsAssignableFrom(sourcePinVM.ValueType))
						{
							CNodeConnectionViewModel newConnection = new CNodeConnectionViewModel(sourcePinVM, sourceViewModel.InputPins[i], this);
							newConnection.Connect();
						}
						else
						{
							inputPin.SourceNode = null;
							inputPin.SourceParameterIndex = -1;
							LogUtility.Log("[ScriptLoadWarning] Node {0} tried to connect to {1} at output pin index {2} but the pin types are not compatible, connection removed", sourceViewModel.Name, inputSourceVm.Name, inputPin.SourceParameterIndex);
						}
					}
					else
					{
						inputPin.SourceNode = null;
						inputPin.SourceParameterIndex = -1;
						LogUtility.Log("[ScriptLoadError] node {0} tried to connect to {1} at output pin index {2} but there are not enough pins", sourceViewModel.Name, inputSourceVm.Name, inputPin.SourceParameterIndex);
					}
				}
			}
		}

		public void LoadGraph(CGraph graph)
		{
			CGraph oldGraph = ScriptGraph;
			ScriptGraph = graph;
			GraphName = ScriptGraph.Name;

			Dictionary<CNode, CScriptNodeViewmodel> nodeToViewModel = new Dictionary<CNode, CScriptNodeViewmodel>();
			List<CScriptNodeViewmodel> newNodes = new List<CScriptNodeViewmodel>();

			foreach (CNode graphNode in ScriptGraph.m_nodes)
			{
				CScriptNodeViewmodel viewmodel = new CScriptNodeViewmodel(graphNode, this);
				nodeToViewModel.Add(graphNode, viewmodel);
				newNodes.Add(viewmodel);
			}

			m_selectedNodes.Clear();
			Nodes = new ObservableCollection<CScriptNodeViewmodel>(newNodes);
			Connections.Clear();

			ResolveScriptNodeConnections(ScriptGraph.m_nodes, nodeToViewModel);
			UndoRedoUtility.Purge(null);

			OnGraphChanged?.Invoke(this, oldGraph, ScriptGraph);
		}

		public void CloseGraph()
		{
			ScriptGraph = null;
			GraphName = "";

			foreach (CScriptNodeViewmodel nodeViewmodel in Nodes)
			{
				nodeViewmodel.RemoveNodeListener();
			}

			Nodes.Clear();
			Connections.Clear();			
		}

		public CScriptNodeViewmodel AddNode(CNode node, bool bRecordUndo = true)
		{
			CScriptNodeViewmodel nodeViewModel = new CScriptNodeViewmodel(node, this);

			void Redo()
			{
				Nodes.Add(nodeViewModel);
				ScriptGraph.m_nodes.Add(nodeViewModel.ScriptNode);
			}

			void Undo()
			{
				DeleteNode(nodeViewModel, false);
			}

			Redo();
			if (bRecordUndo)
			{
				UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
			}
			return nodeViewModel;
		}

		public void RemoveConnection(CNodeConnectionViewModel connection)
		{
			Connections.Remove(connection);
		}

		#region Node Move and Selection		
		internal void NodeClicked(CScriptNodeViewmodel clickedNode)
		{
			AddNodeViewModel.IsOpen = false;
			m_clickedNode = clickedNode;

			var cursorPoint = System.Windows.Forms.Cursor.Position;
			m_mouseStartPoint = new Point(cursorPoint.X, cursorPoint.Y);
			MouseHook.OnMouseMove += OnMouseMoveNode;
			MouseHook.OnMouseUp += OnMouseUpNode;
		}

		private void OnMouseMoveNode(object e, Point mousePos)
		{
			Vector mouseDelta = mousePos - m_mouseStartPoint;
			mouseDelta *= 1 / GraphScaleFactor;
			if (!m_bIsDraggingNodes)
			{
				if (mouseDelta.LengthSquared > m_nodeMoveThresholdSq)
				{
					if (!m_clickedNode.IsSelected)
					{
						NotifySelected(m_clickedNode);
					}

					foreach (var node in m_selectedNodes)
					{
						node.StartMove();
					}

					m_bIsDraggingNodes = true;
				}
				else
				{
					return;
				}
			}

			foreach (var node in m_selectedNodes)
			{
				node.Move(in mouseDelta);
			}
		}

		private void OnMouseUpNode(object e, Point mousePos)
		{
			if (!m_bIsDraggingNodes)
			{
				NotifySelected(m_clickedNode);
			}
			else
			{
				List<(CScriptNodeViewmodel, Point, Point)> nodeMoves = new List<(CScriptNodeViewmodel, Point, Point)>();
				foreach (var node in m_selectedNodes)
				{
					Point oldPos = new Point(node.PosXStart, node.PosYStart);
					Point newPos = new Point(node.PosX, node.PosY);
					nodeMoves.Add((node, oldPos, newPos));

					node.StopMove();
				}

				void Redo()
				{
					foreach (var nodeMove in nodeMoves)
					{
						nodeMove.Item1.MoveTo(in nodeMove.Item3);
					}
				}

				void Undo()
				{
					foreach (var nodeMove in nodeMoves)
					{
						nodeMove.Item1.MoveTo(in nodeMove.Item2);
					}
				}

				UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
			}

			MouseHook.OnMouseMove -= OnMouseMoveNode;
			MouseHook.OnMouseUp -= OnMouseUpNode;
			m_bIsDraggingNodes = false;
		}

		private double m_nodeMoveThresholdSq = 1;
		private CScriptNodeViewmodel m_clickedNode;
		private Point m_mouseStartPoint;
		private bool m_bIsDraggingNodes;

		public void SelectNodes(IEnumerable<CScriptNodeViewmodel> nodes)
		{
			DeselectAll();
			foreach (var node in nodes)
			{
				node.IsSelected = true;
				m_selectedNodes.Add(node);
			}
		}

		public void DeselectAll()
		{
			foreach (var node in m_selectedNodes)
			{
				node.IsSelected = false;
			}
			m_selectedNodes.Clear();
		}

		/// <summary>
		/// Called to notify the graph that a node wants to get selected
		/// </summary>
		/// <param name="selectedNode"></param>
		public void NotifySelected(CScriptNodeViewmodel selectedNode)
		{
			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
			{
				int index = m_selectedNodes.IndexOf(selectedNode);
				if (index >= 0)
				{
					selectedNode.IsSelected = false;
					m_selectedNodes.RemoveAt(index);
					return;
				}
			}
			else
			{
				if (m_selectedNodes.Contains(selectedNode))
				{
					return;
				}

				DeselectAll();
			}

			selectedNode.IsSelected = true;
			m_selectedNodes.Add(selectedNode);
		}
		#endregion

		private void DeleteNode(CScriptNodeViewmodel nodeToDelete, bool bRecordUndo = true)
		{
			if (!nodeToDelete.ScriptNode.AllowDelete)
			{
				return;
			}

			List<CNodeConnectionViewModel> deletedConnections = new List<CNodeConnectionViewModel>();
			nodeToDelete.GetConnections(deletedConnections);

			void Redo()
			{
				foreach (var connection in deletedConnections)
				{
					connection.Disconnect();
				}

				ScriptGraph.m_nodes.Remove(nodeToDelete.ScriptNode);
				Nodes.Remove(nodeToDelete);

				DeselectAll();
			}

			void Undo()
			{
				ScriptGraph.m_nodes.Add(nodeToDelete.ScriptNode);
				Nodes.Add(nodeToDelete);

				foreach (var connection in deletedConnections)
				{
					connection.Connect();
				}
			}

			Redo();

			if (bRecordUndo)
			{
				UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
			}
		}

		private void DeleteSelectedNodes(IList<CScriptNodeViewmodel> nodesToDelete, bool bRecordUndo = true)
		{
			List<CScriptNodeViewmodel> deletedNodes = new List<CScriptNodeViewmodel>();
			List<CNodeConnectionViewModel> deletedConnections = new List<CNodeConnectionViewModel>();
			foreach (var node in nodesToDelete)
			{
				if (node.ScriptNode.AllowDelete)
				{
					deletedNodes.Add(node);
					node.GetConnections(deletedConnections);
				}
			}

			if (deletedNodes.Count <= 0)
			{
				return;
			}

			void Redo()
			{
				foreach (var connection in deletedConnections)
				{
					connection.Disconnect();
				}

				foreach (var deletedNode in deletedNodes)
				{
					ScriptGraph.m_nodes.Remove(deletedNode.ScriptNode);
					Nodes.Remove(deletedNode);
				}

				DeselectAll();
			}

			void Undo()
			{
				foreach (var deletedNode in deletedNodes)
				{
					ScriptGraph.m_nodes.Add(deletedNode.ScriptNode);
					Nodes.Add(deletedNode);
				}

				foreach (var connection in deletedConnections)
				{
					connection.Connect();
				}
			}

			Redo();

			if (bRecordUndo)
			{
				UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
			}
		}

		private void OnNodeAddNodePopupSelected(CNodeEntryViewModel addNodeEntry)
		{
			CNode scriptNode = addNodeEntry.NodeFactory.CreateNode();
			scriptNode.NodePosX = m_addNodePoint.X;
			scriptNode.NodePosY = m_addNodePoint.Y;
			CScriptNodeViewmodel newNode = AddNode(scriptNode);

			if (m_addNodeContextPin != null)
			{
				CPinViewModel possibleTarget = newNode.GetPossibleTargetPin(m_addNodeContextPin);
				if (possibleTarget != null)
				{
					ConnectPins(m_addNodeContextPin, possibleTarget);
				}
			}

			m_addNodeContextPin = null;
			AddNodeViewModel.IsOpen = false;
		}

		private void OnScaleChanged()
		{
			for (int i = 0; i < m_lodLevels.Count; i++)
			{
				if (m_lodLevels[i] < m_graphScaleFactor)
				{
					Lod = i;
					break;
				}
			}
		}

		private void OnPreviewMouseDown(object e)
		{
			MouseButtonEventArgs args = (MouseButtonEventArgs)e;
			if (args.ChangedButton == MouseButton.Right)
			{
				m_mouseDownPoint = args.GetPosition(null);
				NodeGraphView graphView = (NodeGraphView)Content;
				m_addNodePoint = graphView.GetMouseEventPosInNodeCanvas(args);
			}
		}
		private void OnMouseDown(object e)
		{
			DeselectAll();
			AddNodeViewModel.IsOpen = false;
		}

		private void OnPreviewMouseUp(object e)
		{
			MouseButtonEventArgs args = (MouseButtonEventArgs)e;
			if (args.ChangedButton == MouseButton.Right && ScriptGraph != null)
			{
				Vector delta = m_mouseDownPoint - args.GetPosition(null);
				if (delta.LengthSquared < m_nodeMoveThresholdSq)
				{
					m_addNodeContextPin = null;
					AddNodeViewModel.SetContext(new CKlaxScriptNodeQueryContext(ScriptGraph.ScriptableObject));
					AddNodeViewModel.IsOpen = true;
				}
			}
		}

		private void OnStartDragContent(object e)
		{
			AddNodeViewModel.IsOpen = false;
		}

		private void OnDeleteCommand(object e)
		{
			DeleteSelectedNodes(m_selectedNodes);
		}

		#region PinDragging
		private void StartDraggingPin()
		{
			IsDraggingPin = true;
			PreviewConnection.IsVisible = true;
			PreviewConnection.StartPoint = DraggedPin.GetConnectionPointCanvas();
			PreviewConnection.EndPoint = DraggedPin.GetConnectionPointCanvas();
			PreviewConnection.StrokeColor = DraggedPin.PinOuterColor;
			AddNodeViewModel.IsOpen = false;
			m_addNodeContextPin = null;
		}

		private void StopDraggingPin()
		{
			if (IsDraggingPin)
			{
				if (PreviewConnection.IsValidDrop && PreviewConnection.DropTarget != null)
				{
					ConnectPins(DraggedPin, PreviewConnection.DropTarget);
				}
				else
				{
					CKlaxScriptNodeQueryContext queryContext = new CKlaxScriptNodeQueryContext
					{
						InputPinType = DraggedPin.GetPinType(),
						IsExecPin = DraggedPin.IsExecutionPin(),
						QueryObject = ScriptGraph.ScriptableObject,
					};
					m_addNodePoint = ((NodeGraphView)Content).GetPasteReferenceLocation();
					AddNodeViewModel.SetContext(queryContext);
					AddNodeViewModel.IsOpen = true;
					m_addNodeContextPin = DraggedPin;
				}
			}

			PreviewConnection.IsVisible = false;
			PreviewConnection.IsValidDrop = false;
			PreviewConnection.DropTarget = null;
			IsDraggingPin = false;
			Mouse.OverrideCursor = null;
		}

		private void ConnectPins(CPinViewModel source, CPinViewModel target)
		{
			CNodeConnectionViewModel newConnection = new CNodeConnectionViewModel(source, target, this);

			void Redo()
			{
				newConnection.Connect();
			}

			void Undo()
			{
				newConnection.Disconnect();
			}

			Redo();
			UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
		}

		private void OnMouseMovePin(object sender, Point mouseLocation)
		{
			if (!IsDraggingPin)
			{
				Vector delta = mouseLocation - m_dragStartPoint;
				if (DragDropHelpers.IsMovementBiggerThreshold(delta))
				{
					StartDraggingPin();
				}
			}
			else
			{
				bool bPinHit = ((NodeGraphView)Content).HitTestConnection(mouseLocation, out CPinViewModel hitPin, out Point canvasHit);
				PreviewConnection.EndPoint = canvasHit;
				if (bPinHit)
				{
					PreviewConnection.IsValidDrop = hitPin.CanConnect(DraggedPin, out string errorMessage);
					PreviewConnection.ErrorMessage = errorMessage;
					Mouse.OverrideCursor = PreviewConnection.IsValidDrop ? Cursors.Hand : Cursors.No;
					PreviewConnection.DropTarget = hitPin;
				}
				else
				{
					PreviewConnection.DropTarget = null;
					PreviewConnection.IsValidDrop = true;
					PreviewConnection.ErrorMessage = "";
					Mouse.OverrideCursor = null;
				}
			}
		}

		private void OnMouseUpPin(object sender, Point p)
		{
			MouseHook.OnMouseMove -= OnMouseMovePin;
			MouseHook.OnMouseUp -= OnMouseUpPin;
			StopDraggingPin();
			DraggedPin = null;
		}

		private bool m_bIsDraggingPin;
		public bool IsDraggingPin
		{
			get { return m_bIsDraggingPin; }
			set { m_bIsDraggingPin = value; RaisePropertyChanged(); }
		}

		private Point m_dragStartPoint;
		private CPinViewModel m_draggedPin;
		public CPinViewModel DraggedPin
		{
			get { return m_draggedPin; }
			set
			{
				if (value != null)
				{
					System.Drawing.Point mousePoint = System.Windows.Forms.Cursor.Position;
					m_dragStartPoint.X = mousePoint.X;
					m_dragStartPoint.Y = mousePoint.Y;
					MouseHook.OnMouseMove += OnMouseMovePin;
					MouseHook.OnMouseUp += OnMouseUpPin;
				}
				m_draggedPin = value;
			}
		}
		#endregion

		#region BoundProperties
		private ObservableCollection<CScriptNodeViewmodel> m_nodes = new ObservableCollection<CScriptNodeViewmodel>();
		public ObservableCollection<CScriptNodeViewmodel> Nodes
		{
			get { return m_nodes; }
			set { m_nodes = value; RaisePropertyChanged(); }
		}

		private ObservableCollection<CNodeConnectionViewModel> m_connections = new ObservableCollection<CNodeConnectionViewModel>();
		public ObservableCollection<CNodeConnectionViewModel> Connections
		{
			get { return m_connections; }
			set { m_connections = value; RaisePropertyChanged(); }
		}

		private CPreviewConnectionViewModel m_previewConnection = new CPreviewConnectionViewModel();
		public CPreviewConnectionViewModel PreviewConnection
		{
			get { return m_previewConnection; }
			set { m_previewConnection = value; RaisePropertyChanged(); }

		}

		private CVariableDragPopupViewModel m_variablePopup;
		public CVariableDragPopupViewModel VariablePopup
		{
			get { return m_variablePopup; }
			set { m_variablePopup = value; RaisePropertyChanged(); }
		}

		private int m_lod;
		public int Lod
		{
			get { return m_lod; }
			set { m_lod = value; RaisePropertyChanged(); }
		}

		private double m_graphScaleFactor;
		public double GraphScaleFactor
		{
			get { return m_graphScaleFactor; }
			set { m_graphScaleFactor = value; OnScaleChanged(); RaisePropertyChanged(); }
		}

		private CAddNodeViewModel m_addNodeViewModel;
		public CAddNodeViewModel AddNodeViewModel
		{
			get { return m_addNodeViewModel; }
			set
			{
				m_addNodeViewModel = value;
				RaisePropertyChanged();
			}
		}

		private string m_graphName;
		public string GraphName
		{
			get { return m_graphName; }
			set
			{
				m_graphName = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region Commands
		public ICommand MouseDownCommand { get; set; }
		public ICommand PreviewMouseDownCommand { get; set; }
		public ICommand PreviewMouseUpCommand { get; set; }
		public ICommand StartDragContentCommand { get; set; }
		public ICommand DeleteNodesCommand { get; set; }
		public ICommand SaveGraphCommand { get; set; }
		public ICommand CopyNodesCommand { get; set; }
		public ICommand PasteNodesCommand { get; set; }
		public ICommand CutNodesCommand { get; set; }
		public ICommand DuplicateNodesCommand { get; set; }
		public ICommand DragEnterCommand { get; set; }
		public ICommand DragOverCommand { get; set; }
		public ICommand DropCommand { get; set; }
		#endregion

		#region Events
		public delegate void GraphChangedSignature(CNodeGraphViewModel viewModel, CGraph oldGraph, CGraph newGraph);
		public event GraphChangedSignature OnGraphChanged;
		#endregion

		public CGraph ScriptGraph { get; private set; }
		private readonly List<CScriptNodeViewmodel> m_selectedNodes = new List<CScriptNodeViewmodel>(10);
		private readonly List<double> m_lodLevels = new List<double>();

		private Point m_mouseDownPoint;
		private Point m_addNodePoint;
		private CPinViewModel m_addNodeContextPin;
	}
}

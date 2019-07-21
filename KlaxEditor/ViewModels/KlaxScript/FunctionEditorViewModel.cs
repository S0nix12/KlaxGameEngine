using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using KlaxCore.KlaxScript;
using KlaxCore.KlaxScript.Interfaces;
using KlaxEditor.Utility.UndoRedo;
using KlaxEditor.ViewModels.EditorWindows;

namespace KlaxEditor.ViewModels.KlaxScript
{
    class CFunctionEditorViewModel : CViewModelBase
	{
		public CFunctionEditorViewModel()
		{
			AddInputCommand = new CRelayCommand(OnAddInput);
			AddOutputCommand = new CRelayCommand(OnAddOutput);
		}

		public void OpenFunctionGraph(CCustomFunctionGraph functionGraph)
		{
			m_functionGraph = functionGraph;

			InputParameters.Clear();
			OutputParameters.Clear();
			foreach (CKlaxVariable inputParameter in functionGraph.InputParameters)
			{
				AddInput(inputParameter, false);
			}

			foreach (CKlaxVariable outputParameter in functionGraph.OutputParameters)
			{
				AddOutput(outputParameter, false);
			}
			RaisePropertyChanged(nameof(IsVisible));
		}

		public void CloseFunctionEditor()
		{
			m_functionGraph = null;

			InputParameters.Clear();
			OutputParameters.Clear();

			RaisePropertyChanged(nameof(IsVisible));
		}

		private void OnParameterPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			m_functionGraph?.RebuildFunctionNodes();
		}

		private void OnAddInput(object e)
		{
			if (m_functionGraph == null)
			{
				return;
			}

			CKlaxVariable newInput = new CKlaxVariable()
			{
				Name = "NewVariable",
				Type = typeof(int),
				Value = 0
			};

			AddInput(newInput, true);
		}

		private void AddInput(CKlaxVariable parameter, bool bIsNewParameter)
		{
			var newViewmodel = new CEntityVariableViewModel(parameter);
			var functionGraph = m_functionGraph;

			void Redo()
			{
				newViewmodel.PropertyChanged += OnParameterPropertyChanged;
				functionGraph.InputParameters.Add(parameter);
				InputParameters.Add(newViewmodel);
				functionGraph.RebuildFunctionNodes();
			}

			void Undo()
			{
				newViewmodel.PropertyChanged -= OnParameterPropertyChanged;
				InputParameters.Remove(newViewmodel);
				functionGraph.InputParameters.Remove(parameter);
				functionGraph.RebuildFunctionNodes();
			}

			newViewmodel.DeleteCommand = new CRelayCommand(arg =>
			{
				Undo();
				UndoRedoUtility.Record(new CRelayUndoItem(Redo, Undo));
			});

			if (bIsNewParameter)
			{
				Redo();
			}
			else
			{
				newViewmodel.PropertyChanged += OnParameterPropertyChanged;
				InputParameters.Add(newViewmodel);
			}

			if (bIsNewParameter)
			{
				UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
			}
		}

		private void OnAddOutput(object e)
		{
			if (m_functionGraph == null)
			{
				return;
			}

			CKlaxVariable newOutput = new CKlaxVariable()
			{
				Name = "NewVariable",
				Type = typeof(int),
				Value = 0
			};

			AddOutput(newOutput, true);
		}

		private void AddOutput(CKlaxVariable parameter, bool bIsNewParameter)
		{
			var newViewmodel = new CEntityVariableViewModel(parameter);
			var functionGraph = m_functionGraph;

			void Redo()
			{
				newViewmodel.PropertyChanged += OnParameterPropertyChanged;
				functionGraph.OutputParameters.Add(parameter);
				OutputParameters.Add(newViewmodel);
				functionGraph.RebuildFunctionNodes();
			}

			void Undo()
			{
				newViewmodel.PropertyChanged -= OnParameterPropertyChanged;
				OutputParameters.Remove(newViewmodel);
				functionGraph.OutputParameters.Remove(parameter);
				functionGraph.RebuildFunctionNodes();
			}

			newViewmodel.DeleteCommand = new CRelayCommand(arg =>
			{
				Undo();
				UndoRedoUtility.Record(new CRelayUndoItem(Redo, Undo));
			});

			if (bIsNewParameter)
			{
				Redo();
			}
			else
			{
				newViewmodel.PropertyChanged += OnParameterPropertyChanged;
				OutputParameters.Add(newViewmodel);
			}

			if (bIsNewParameter)
			{
				UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
			}
		}

		private ObservableCollection<CEntityVariableViewModel> m_inputParameters = new ObservableCollection<CEntityVariableViewModel>();

		public ObservableCollection<CEntityVariableViewModel> InputParameters
		{
			get { return m_inputParameters; }
			set
			{
				m_inputParameters = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CEntityVariableViewModel> m_outputParameters = new ObservableCollection<CEntityVariableViewModel>();

		public ObservableCollection<CEntityVariableViewModel> OutputParameters
		{
			get { return m_outputParameters; }
			set
			{
				m_outputParameters = value;
				RaisePropertyChanged();
			}
		}

		public bool IsVisible
		{
			get { return m_functionGraph != null; }
		}

		public ICommand AddInputCommand { get; set; }
		public ICommand AddOutputCommand { get; set; }
		public ICommand DeleteCommand { get; set; }

		private CCustomFunctionGraph m_functionGraph;
	}
}

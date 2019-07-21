using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using KlaxCore.KlaxScript;
using KlaxCore.KlaxScript.Interfaces;
using KlaxEditor.Utility.UndoRedo;
using KlaxEditor.ViewModels.EditorWindows;
using KlaxEditor.Views.KlaxScript;
using KlaxIO.AssetManager.Assets;

namespace KlaxEditor.ViewModels.KlaxScript
{
	class CInterfaceFunctionViewModel : CViewModelBase
	{
		public CInterfaceFunctionViewModel(CKlaxScriptInterfaceFunction interfaceFunction, CInterfaceEditorViewmodel parentViewModel)
		{
			m_parentViewModel = parentViewModel;
			m_interfaceFunction = interfaceFunction;
			m_name = interfaceFunction.Name;

			foreach (CKlaxVariable inputParameter in m_interfaceFunction.InputParameters)
			{
				AddInput(inputParameter, false);
			}

			foreach (CKlaxVariable outputParameter in m_interfaceFunction.OutputParameters)
			{
				AddOutput(outputParameter, false);
			}

			AddInputCommand = new CRelayCommand(OnAddInput);
			AddOutputCommand = new CRelayCommand(OnAddOutput);
			DeleteCommand = new CRelayCommand(OnDelete);
		}

		private void OnDelete(object e)
		{
			void Redo()
			{
				m_parentViewModel.CurrentInterface.Functions.Remove(m_interfaceFunction);
				m_parentViewModel.Functions.Remove(this);
			}

			void Undo()
			{
				m_parentViewModel.CurrentInterface.Functions.Add(m_interfaceFunction);
				m_parentViewModel.Functions.Add(this);
			}

			Redo();
			UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
		}

		private void OnAddInput(object e)
		{
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

			void Redo()
			{
				m_interfaceFunction.InputParameters.Add(parameter);
				InputParameters.Add(newViewmodel);
			}

			void Undo()
			{
				InputParameters.Remove(newViewmodel);
				m_interfaceFunction.InputParameters.Remove(parameter);
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
				InputParameters.Add(newViewmodel);	
			}

			if (bIsNewParameter)
			{
				UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
			}
		}

		private void OnAddOutput(object e)
		{
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

			void Redo()
			{
				m_interfaceFunction.OutputParameters.Add(parameter);
				OutputParameters.Add(newViewmodel);
			}

			void Undo()
			{
				OutputParameters.Remove(newViewmodel);
				m_interfaceFunction.OutputParameters.Remove(parameter);
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

		private string m_name;

		public string Name
		{
			get { return m_name; }
			set
			{
				m_name = value;
				m_interfaceFunction.Name = value;
				RaisePropertyChanged();
			}
		}

		public ICommand AddInputCommand { get; set; }
		public ICommand AddOutputCommand { get; set; }
		public ICommand DeleteCommand { get; set; }

		private CKlaxScriptInterfaceFunction m_interfaceFunction;
		private CInterfaceEditorViewmodel m_parentViewModel;
	}

	class CInterfaceEditorViewmodel : CEditorWindowViewModel
	{
		public CInterfaceEditorViewmodel() : base("InterfaceEditor")
		{
			Content = new InterfaceEditorView();
			AddFunctionCommand = new CRelayCommand(OnAddFunction);
			SaveCommand = new CRelayCommand(args => SaveAsset());
		}

		public void OpenAsset(CKlaxScriptInterfaceAsset interfaceAsset)
		{
			SaveAsset();
			CloseInterface();
			m_openAsset = interfaceAsset;

			// Make sure the asset is loaded
			m_openAsset.WaitUntilLoaded();
			InterfaceName = interfaceAsset.Name;
			OpenInterface(m_openAsset.Interface);
			IsVisible = true;
		}

		public void SaveAsset()
		{
			if (m_openAsset == null)
			{
				return;				
			}

			CAssetRegistry.Instance.SaveAsset(m_openAsset);
		}

		public void OpenInterface(CKlaxScriptInterface scriptInterface)
		{
			CurrentInterface = scriptInterface;
			foreach (var interfaceFunction in scriptInterface.Functions)
			{
				Functions.Add(new CInterfaceFunctionViewModel(interfaceFunction, this));
			}
			RaisePropertyChanged(nameof(IsInterfaceOpen));
		}

		public void CloseInterface()
		{
			Functions.Clear();
			InterfaceName = "";
			CurrentInterface = null;
			RaisePropertyChanged(nameof(IsInterfaceOpen));
		}

		private void OnAddFunction(object e)
		{
			if (CurrentInterface == null)
			{
				return;
			}

			CKlaxScriptInterfaceFunction newFunction = new CKlaxScriptInterfaceFunction();
			newFunction.Name = "NewFunction";
			CInterfaceFunctionViewModel newViewModel = new CInterfaceFunctionViewModel(newFunction, this);

			void Redo()
			{
				CurrentInterface.Functions.Add(newFunction);
				Functions.Add(newViewModel);
			}

			void Undo()
			{
				CurrentInterface.Functions.Remove(newFunction);
				Functions.Remove(newViewModel);
			}

			Redo();
			UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
		}

		private ObservableCollection<CInterfaceFunctionViewModel> m_functions = new ObservableCollection<CInterfaceFunctionViewModel>();

		public ObservableCollection<CInterfaceFunctionViewModel> Functions
		{
			get { return m_functions; }
			set
			{
				m_functions = value;
				RaisePropertyChanged();
			}
		}

		private string m_interfaceName;

		public string InterfaceName
		{
			get { return m_interfaceName; }
			set
			{
				m_interfaceName = value;
				RaisePropertyChanged();
			}
		}

		public bool IsInterfaceOpen
		{
			get { return CurrentInterface != null; }
		}

		public ICommand AddFunctionCommand { get; set; }
		public ICommand SaveCommand { get; set; }

		private CKlaxScriptInterfaceAsset m_openAsset;
		public CKlaxScriptInterface CurrentInterface { get; private set; }
	}
}
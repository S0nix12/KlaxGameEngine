using KlaxCore.Core;
using KlaxCore.EditorHelper;
using KlaxCore.GameFramework;
using KlaxCore.GameFramework.Assets;
using KlaxCore.KlaxScript;
using KlaxEditor.UserControls.InspectorControls;
using KlaxEditor.Utility;
using KlaxEditor.Utility.UndoRedo;
using KlaxEditor.Views;
using KlaxIO.AssetManager.Assets;
using KlaxShared.Utilities;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using KlaxCore.KlaxScript.Interfaces;
using KlaxEditor.ViewModels.KlaxScript;
using System.Reflection;
using KlaxShared.Attributes;

namespace KlaxEditor.ViewModels.EditorWindows
{
	enum EVariableContainerType
	{
		Single,
		List,
		Dictionary
	}

	class CEBInspectorEntityViewModel : CInspectorEntityViewModel
	{
		public CEBInspectorEntityViewModel(CEntityBuilderInspectorViewModel vm, string name, SEntityId entityId)
			: base(vm, name, entityId)
		{
		}

		protected override void OnSelectedChanged(bool bNewValue)
		{
			CEntityBuilderInspectorViewModel vm = m_viewModel as CEntityBuilderInspectorViewModel;

			if (bNewValue)
			{
				vm.InspectObject(new CEditableObject(EntityId));
			}
			else
			{
				if (vm.DesiredTarget == EntityId)
				{
					vm.InspectObject(null);
				}
			}
		}
	}

	class CEBInspectorSceneComponentViewModel : CInspectorSceneComponentViewModel
	{
		public CEBInspectorSceneComponentViewModel(IInspectorViewModel viewModel, string name, SEntityComponentId componentId, string dragIdentifier)
			: base(viewModel, name, componentId, dragIdentifier)
		{
		}

		protected override void OnSelectedChanged(bool bNewValue)
		{
			CEntityBuilderInspectorViewModel vm = m_viewModel as CEntityBuilderInspectorViewModel;

			if (bNewValue)
			{
				vm.InspectObject(new CEditableObject(ComponentId));
			}
			else
			{
				if (vm.DesiredTarget == ComponentId)
				{
					vm.InspectObject(null);
				}
			}
		}

		protected override void OnDeleteComponent(object argument)
		{
			CEntityBuilderInspectorViewModel vm = m_viewModel as CEntityBuilderInspectorViewModel;

			CEntityComponent component = ComponentId.GetComponent();
			component.Destroy();
			UndoRedoUtility.Purge(null);

			vm.UpdateEntityInformation();
		}

		protected override void OnMakeRoot(object argument)
		{
			CEntityBuilderInspectorViewModel vm = m_viewModel as CEntityBuilderInspectorViewModel;
			EditorEntityUtility.MakeComponentRoot(ComponentId, false);

			vm.UpdateEntityInformation();
		}
	}

	class CEBInspectorEntityComponentViewModel : CInspectorEntityComponentViewModel
	{
		public CEBInspectorEntityComponentViewModel(IInspectorViewModel viewModel, string name, SEntityComponentId componentId)
			: base(viewModel, name, componentId)
		{
		}

		protected override void OnSelectedChanged(bool bNewValue)
		{
			CEntityBuilderInspectorViewModel vm = m_viewModel as CEntityBuilderInspectorViewModel;

			if (bNewValue)
			{
				vm.InspectObject(new CEditableObject(ComponentId));
			}
			else
			{
				if (vm.DesiredTarget == ComponentId)
				{
					vm.InspectObject(null);
				}
			}
		}
	}

	class CEntityVariableViewModel : CViewModelBase
	{
		static CEntityVariableViewModel()
		{
			CKlaxScriptRegistry.Instance.TryGetTypeInfo(typeof(int), out s_intKlaxType);
		}

		public CEntityVariableViewModel(CKlaxVariable variable)
		{
			Variable = variable;
			m_name = variable.Name;
			m_value = variable.Value;

			CKlaxScriptRegistry registry = CKlaxScriptRegistry.Instance;
			if (variable.Type.IsGenericType)
			{
				Type genericType = variable.Type.GetGenericTypeDefinition();
				if (genericType == typeof(List<>))
				{
					m_selectedVariableContainerType = EVariableContainerType.List;
					registry.TryGetTypeInfo(variable.Type.GenericTypeArguments[0], out CKlaxScriptTypeInfo outInfo);
					m_klaxType = outInfo;
				}
				else if (genericType == typeof(Dictionary<,>))
				{
					m_selectedVariableContainerType = EVariableContainerType.Dictionary;
					registry.TryGetTypeInfo(variable.Type.GenericTypeArguments[0], out m_klaxType);
					registry.TryGetTypeInfo(variable.Type.GenericTypeArguments[1], out m_secondaryKlaxType);
				}
				else
				{
					m_selectedVariableContainerType = EVariableContainerType.Single;
					registry.TryGetTypeInfo(variable.Type, out CKlaxScriptTypeInfo outInfo);
					m_klaxType = outInfo;
				}
			}
			else
			{
				m_selectedVariableContainerType = EVariableContainerType.Single;
				registry.TryGetTypeInfo(variable.Type, out CKlaxScriptTypeInfo outInfo);
				m_klaxType = outInfo;
			}

			RefreshInspectorProperties();
		}

		private void RefreshInspectorProperties()
		{
			List<CObjectBase> newProperties = new List<CObjectBase>(8);

			CCategoryInfo variableCategory = new CCategoryInfo { Name = "Variable Info" };
			Type thisType = typeof(CEntityVariableViewModel);

			CObjectProperty collectionTypeProperty = new CObjectProperty("Container Type", variableCategory, this, m_selectedVariableContainerType, typeof(EVariableContainerType), thisType.GetProperty(nameof(SelectedVariableCollectionType)), null);
			newProperties.Add(collectionTypeProperty);

			if (IsDictionary)
			{
				CObjectProperty valueTypeProperty = new CObjectProperty("Value Type", variableCategory, this, SecondaryKlaxType, typeof(CKlaxScriptTypeInfo), thisType.GetProperty(nameof(SecondaryKlaxType)), null);
				newProperties.Add(valueTypeProperty);
			}

			CObjectProperty defaultValueProperty = new CObjectProperty("Default Value", variableCategory, this, Value, Variable.Type, thisType.GetProperty(nameof(Value)), null);
			newProperties.Add(defaultValueProperty);

			Properties = newProperties;
			RaisePropertyChanged(nameof(Properties));
		}

		private void ChangeCollectionType(EVariableContainerType newType)
		{
			switch (m_selectedVariableContainerType)
			{
				case EVariableContainerType.Single:
					switch (newType)
					{
						case EVariableContainerType.Single:
							Variable.Value = Variable.Type.IsValueType ? Activator.CreateInstance(Variable.Type) : null;
							break;
						case EVariableContainerType.List:
							Type newListType = typeof(List<>).MakeGenericType(Variable.Type);
							Variable.Type = newListType;
							Variable.Value = null;
							break;
						case EVariableContainerType.Dictionary:
							Type newDictionaryType = typeof(Dictionary<,>).MakeGenericType(Variable.Type, typeof(int));
							Variable.Type = newDictionaryType;
							Variable.Value = null;
							m_secondaryKlaxType = s_intKlaxType;
							break;
					}
					break;
				case EVariableContainerType.List:
					switch (newType)
					{
						case EVariableContainerType.Single:
							Type newSingleType = Variable.Type.GenericTypeArguments[0];
							Variable.Type = newSingleType;
							Variable.Value = newSingleType.IsValueType ? Activator.CreateInstance(newSingleType) : null;
							break;
						case EVariableContainerType.List:
							Type newListType = typeof(List<>).MakeGenericType(Variable.Type);
							Variable.Type = newListType;
							Variable.Value = null;
							break;
						case EVariableContainerType.Dictionary:
							Type newDictionaryType = typeof(Dictionary<,>).MakeGenericType(Variable.Type.GenericTypeArguments[0], typeof(int));
							Variable.Type = newDictionaryType;
							Variable.Value = null;
							m_secondaryKlaxType = s_intKlaxType;
							break;
					}
					break;
				case EVariableContainerType.Dictionary:
					switch (newType)
					{
						case EVariableContainerType.Single:
							Type newSingleType = Variable.Type.GenericTypeArguments[0];
							Variable.Type = newSingleType;
							Variable.Value = newSingleType.IsValueType ? Activator.CreateInstance(newSingleType) : null;
							break;
						case EVariableContainerType.List:
							Type newListType = typeof(List<>).MakeGenericType(Variable.Type.GenericTypeArguments[0]);
							Variable.Type = newListType;
							Variable.Value = null;
							break;
						case EVariableContainerType.Dictionary:
							Type newDictionaryType = typeof(Dictionary<,>).MakeGenericType(Variable.Type, typeof(int));
							Variable.Type = newDictionaryType;
							Variable.Value = null;
							m_secondaryKlaxType = s_intKlaxType;
							break;
					}
					break;
			}

			m_value = Variable.Value;
			m_selectedVariableContainerType = newType;
			RefreshInspectorProperties();
		}

		private string m_name;
		public string Name
		{
			get { return m_name; }
			set
			{
				m_name = value;
				Variable.Name = value;
				RaisePropertyChanged();
			}
		}

		private EVariableContainerType m_selectedVariableContainerType;
		public EVariableContainerType SelectedVariableCollectionType
		{
			get { return m_selectedVariableContainerType; }
			set { ChangeCollectionType(value); RaisePropertyChanged(); }
		}

		public Color4 Color
		{
			get { return KlaxType.Color; }
			set { KlaxType.Color = value; }
		}

		private CKlaxScriptTypeInfo m_klaxType;
		public CKlaxScriptTypeInfo KlaxType
		{
			get { return m_klaxType; }
			set
			{
				m_klaxType = value;
				Variable.Type = value.Type;
				ChangeCollectionType(m_selectedVariableContainerType);

				RaisePropertyChanged();
			}
		}

		private object m_value;
		public object Value
		{
			get { return m_value; }
			set
			{
				m_value = value;
				Variable.Value = m_value;
				RaisePropertyChanged();
			}
		}

		private CKlaxScriptTypeInfo m_secondaryKlaxType;
		public CKlaxScriptTypeInfo SecondaryKlaxType
		{
			get { return m_secondaryKlaxType; }
			set
			{
				m_secondaryKlaxType = value;

				if (m_selectedVariableContainerType == EVariableContainerType.Dictionary)
				{
					Type newDictionaryType = typeof(Dictionary<,>).MakeGenericType(KlaxType.Type, value.Type);
					Variable.Type = newDictionaryType;
					Variable.Value = null;
					m_value = null;
					RefreshInspectorProperties();
				}

				RaisePropertyChanged();
			}
		}

		private bool IsSingleElement
		{
			get { return m_selectedVariableContainerType == EVariableContainerType.Single; }
		}

		private bool IsList
		{
			get { return m_selectedVariableContainerType == EVariableContainerType.List; }
		}

		private bool IsDictionary
		{
			get { return m_selectedVariableContainerType == EVariableContainerType.Dictionary; }
		}

		private ICommand m_deleteCommand;
		public ICommand DeleteCommand
		{
			get { return m_deleteCommand; }
			set { m_deleteCommand = value; RaisePropertyChanged(); }
		}

		public List<CObjectBase> Properties { get; set; } = new List<CObjectBase>(8);

		public CKlaxVariable Variable { get; }
		private static CKlaxScriptTypeInfo s_intKlaxType;
	}

	class CScriptEventGraphEntryViewmodel : CViewModelBase
	{
		public CScriptEventGraphEntryViewmodel(CEventGraph graph, CEntityBuilderInspectorViewModel viewModel)
		{
			m_parentViewModel = viewModel;
			ScriptGraph = graph;
			Name = graph.Name;
			DeleteCommand = new CRelayCommand(OnDelete);
			DoubleClickCommand = new CRelayCommand(OnDoubleClick);
		}

		private void OnDelete(object e)
		{
			int graphIndex = m_parentViewModel.EventGraphs.IndexOf(this);
			m_parentViewModel.DeleteEventGraph(graphIndex);
		}

		private void OnDoubleClick(object e)
		{
			CNodeGraphViewModel nodeGraph = CWorkspace.Instance.GetTool<CNodeGraphViewModel>();
			nodeGraph.LoadGraph(ScriptGraph);
		}

		private string m_name;
		public string Name
		{
			get { return m_name; }
			set
			{
				m_name = value;
				m_parentViewModel.RenameEventGraph(m_parentViewModel.EventGraphs.IndexOf(this), m_name);
				RaisePropertyChanged();
			}
		}

		public ICommand DoubleClickCommand { get; set; }
		public ICommand DeleteCommand { get; set; }
		public CEventGraph ScriptGraph { get; }

		private CEntityBuilderInspectorViewModel m_parentViewModel;
	}

	class CKlaxEventEntryViewmodel : CViewModelBase
	{
		public CKlaxEventEntryViewmodel(CKlaxScriptEventInfo klaxEvent, CEntityBuilderInspectorViewModel viewmodel)
		{
			m_parentViewModel = viewmodel;
			KlaxEvent = klaxEvent;
			Name = KlaxEvent.displayName;
			Category = KlaxEvent.category;
			MouseDownCommand = new CRelayCommand(OnMouseDown);
		}
		public CKlaxEventEntryViewmodel(CKlaxScriptEventInfo klaxEvent, CEntityComponent sourceComponent, CEntityBuilderInspectorViewModel viewmodel)
		{
			m_parentViewModel = viewmodel;
			KlaxEvent = klaxEvent;
			Name = sourceComponent.Name + " " + KlaxEvent.displayName;
			m_sourceComponentName = sourceComponent.Name;
			Category = KlaxEvent.category;
			MouseDownCommand = new CRelayCommand(OnMouseDown);

			m_componentGuid = sourceComponent.ComponentGuid;
		}

		private void OnMouseDown(object e)
		{
			if (m_componentGuid == Guid.Empty)
			{
				m_parentViewModel.AddEventGraph(KlaxEvent);
			}
			else
			{
				m_parentViewModel.AddEventGraph(KlaxEvent, m_componentGuid, m_sourceComponentName);
			}
		}

		public string Category { get; set; }
		public string Name { get; set; }		

		public ICommand MouseDownCommand { get; set; }
		public CKlaxScriptEventInfo KlaxEvent { get; }

		private string m_sourceComponentName;
		private Guid m_componentGuid;
		private CEntityBuilderInspectorViewModel m_parentViewModel;
	}

	class CKlaxInterfaceFunctionEntry : CViewModelBase
	{
		public CKlaxInterfaceFunctionEntry(CKlaxScriptInterfaceFunction function, string parentInterfaceName, CEntityBuilderInspectorViewModel parentViewModel)
		{
			m_interfaceFunctionDescription = function;
			m_parentViewModel = parentViewModel;

			Name = parentInterfaceName + "_" + function.Name;
			MouseDownCommand = new CRelayCommand(OnMouseDown);
		}
		private void OnMouseDown(object e)
		{
			m_parentViewModel.AddInterfaceGraph(m_interfaceFunctionDescription, Name);
		}

		public string Name { get; }
		public ICommand MouseDownCommand { get; set; }

		private CKlaxScriptInterfaceFunction m_interfaceFunctionDescription;
		private CEntityBuilderInspectorViewModel m_parentViewModel;
	}
	class CInterfaceGraphEntryViewmodel : CViewModelBase
	{
		public CInterfaceGraphEntryViewmodel(CInterfaceFunctionGraph graph, CEntityBuilderInspectorViewModel viewModel)
		{
			m_parentViewModel = viewModel;
			ScriptGraph = graph;
			Name = graph.Name;
			DeleteCommand = new CRelayCommand(OnDelete);
			DoubleClickCommand = new CRelayCommand(OnDoubleClick);
		}

		private void OnDelete(object e)
		{
			int graphIndex = m_parentViewModel.InterfaceGraphs.IndexOf(this);
			m_parentViewModel.DeleteInterfaceGraph(graphIndex);
		}

		private void OnDoubleClick(object e)
		{
			CNodeGraphViewModel nodeGraph = CWorkspace.Instance.GetTool<CNodeGraphViewModel>();
			nodeGraph.LoadGraph(ScriptGraph);
		}

		private string m_name;
		public string Name
		{
			get { return m_name; }
			set
			{
				m_name = value;
				m_parentViewModel.RenameInterfaceGraph(m_parentViewModel.InterfaceGraphs.IndexOf(this), m_name);
				RaisePropertyChanged();
			}
		}

		public ICommand DoubleClickCommand { get; set; }
		public ICommand DeleteCommand { get; set; }
		public CInterfaceFunctionGraph ScriptGraph { get; }

		private CEntityBuilderInspectorViewModel m_parentViewModel;
	}

	class CFunctionGraphEntryViewModel : CViewModelBase
	{
		public CFunctionGraphEntryViewModel(CCustomFunctionGraph graph, CEntityBuilderInspectorViewModel viewModel)
		{
			m_parentViewModel = viewModel;
			ScriptGraph = graph;
			Name = graph.Name;
			DeleteCommand = new CRelayCommand(OnDelete);
			DoubleClickCommand = new CRelayCommand(OnDoubleClick);
		}

		private void OnDelete(object e)
		{
			int graphIndex = m_parentViewModel.FunctionGraphs.IndexOf(this);
			m_parentViewModel.DeleteFunctionGraph(graphIndex);
		}

		private void OnDoubleClick(object e)
		{
			CNodeGraphViewModel nodeGraph = CWorkspace.Instance.GetTool<CNodeGraphViewModel>();
			nodeGraph.LoadGraph(ScriptGraph);
		}

		private string m_name;
		public string Name
		{
			get { return m_name; }
			set
			{
				m_name = value;
				ScriptGraph.Name = value;
				RaisePropertyChanged();
			}
		}

		public ICommand DoubleClickCommand { get; set; }
		public ICommand DeleteCommand { get; set; }
		public CCustomFunctionGraph ScriptGraph { get; }

		private CEntityBuilderInspectorViewModel m_parentViewModel;
	}

	class CIncludedInterfaceEntry : CViewModelBase
	{
		public CIncludedInterfaceEntry(CKlaxScriptInterfaceReference interfaceReference, CEntityBuilderInspectorViewModel parentViewModel)
		{
			m_interfaceAsset = interfaceReference.InterfaceAsset;
			m_interfaceReference = interfaceReference;
			m_parentViewModel = parentViewModel;

			DeleteCommand = new CRelayCommand(OnDelete);
		}

		private void OnDelete(object e)
		{
			if (m_parentViewModel.InspectedEntity != null)
			{
				CKlaxScriptObject scriptObject = m_parentViewModel.InspectedEntity.KlaxScriptObject;

				void Redo()
				{
					scriptObject.IncludedInterfaces.Remove(m_interfaceReference);
					m_parentViewModel.IncludedInterfaces.Remove(this);
				}

				void Undo()
				{
					scriptObject.IncludedInterfaces.Add(m_interfaceReference);
					m_parentViewModel.IncludedInterfaces.Add(this);
				}

				Redo();
				UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
			}
		}

		private CAssetReference<CKlaxScriptInterfaceAsset> m_interfaceAsset;
		public CAssetReference<CKlaxScriptInterfaceAsset> InterfaceAsset
		{
			get { return m_interfaceAsset; }
			set
			{
				if (m_interfaceAsset != value)
				{
					m_interfaceAsset = value;
					m_interfaceReference.InterfaceAsset = value;
					m_parentViewModel.UpdatePossibleInterfaceFunctions(m_parentViewModel.InspectedEntity.KlaxScriptObject);
					RaisePropertyChanged();
				}
			}
		}

		public Type ControlType { get; } = typeof(CAssetReference<CKlaxScriptInterfaceAsset>);
		public ICommand DeleteCommand { get; set; }

		private CKlaxScriptInterfaceReference m_interfaceReference;
		private CEntityBuilderInspectorViewModel m_parentViewModel;
	}

	class CEntityBuilderInspectorViewModel : CEditorWindowViewModel, IInspectorViewModel
	{
		public CEntityBuilderInspectorViewModel()
			: base("KlaxScript Inspector")
		{
			SetIconSourcePath("Resources/Images/Tabs/entitybuilder.png");

			Content = new EntityBuilderInspector();
			m_view = Content as IInspectorView;

			InitializeEntityMenus();

			RenameEntityCommand = new CRelayCommand(InternalRenameEntity);
			AddComponentCommand = new CRelayCommand(InternalAddComponent);
			ToggleAddComponentMenuCommand = new CRelayCommand((obj) => AddComponentMenuOpen = !AddComponentMenuOpen);
			ToggleEntityCommandsMenuCommand = new CRelayCommand((obj) => EntityCommandsMenuOpen = !EntityCommandsMenuOpen);
			CreateVariableCommand = new CRelayCommand(InternalCreateVariable);
			CreateLocalVariableCommand = new CRelayCommand(InternalCreateLocalVariable);
			AddInterfaceIncludeCommand = new CRelayCommand(InternalAddInterfaceInclude);
			AddFunctionGraphCommand = new CRelayCommand(OnAddFunctionGraph);
		}

		public override void PostToolInit()
		{
			CWorkspace.Instance.GetTool<CEntityBuilderViewModel>().OnAssetOpened += OpenAsset;

			CNodeGraphViewModel nodeGraphViewModel = CWorkspace.Instance.GetTool<CNodeGraphViewModel>();
			OpenedGraph = nodeGraphViewModel.ScriptGraph;
			nodeGraphViewModel.OnGraphChanged += (vm, oldGraph, newGraph) =>
			{
				OpenedGraph = newGraph;
			};
		}

		private void OpenAsset(CEntityAsset<CEntity> entityAsset, CEntity entity)
		{
			IsVisible = true;

			InspectedEntity = entity;

			AssetName = entityAsset.Name;

			CKlaxScriptRegistry scriptRegistry = CKlaxScriptRegistry.Instance;
			if (scriptRegistry.TryGetTypeInfo(InspectedEntity.GetType(), out CKlaxScriptTypeInfo klaxTypeInfo))
			{
				InspectedType = klaxTypeInfo;
			}
			else if (scriptRegistry.TryGetTypeInfo(typeof(CEntity), out CKlaxScriptTypeInfo entityType))
			{
				InspectedType = entityType;
			}

			InspectedEntityAsset = entityAsset;

			UpdateEntityInformation();
			InspectObject(new CEditableObject(new SEntityId(InspectedEntity, true)));
			CloseCurrentGraph();
		}

		public void InspectObject(CEditableObject newObj)
		{
			m_view?.ClearInspector();

			if (newObj != null)
			{
				m_view.SetInspectorVisible(true);

				switch (newObj.Type)
				{
					case CEditableObject.EObjectType.Entity:
						InspectEntity(newObj.EntityId);
						break;
					case CEditableObject.EObjectType.Component:
						InspectComponent(newObj.ComponentId);
						break;
				}
			}
			else
			{
				m_view?.SetInspectorVisible(false);
				Reset();
			}
		}

		private void InitializeEntityMenus()
		{
			List<CAddComponentEntryViewModel> tempList = new List<CAddComponentEntryViewModel>(128);
			Dictionary<string, List<CAddComponentEntryViewModel>> components = new Dictionary<string, List<CAddComponentEntryViewModel>>(12);

			foreach (var type in CKlaxScriptRegistry.Instance.Types)
			{
				if (type.Type.IsSubclassOf(typeof(CEntityComponent)))
				{
					KlaxComponentAttribute attribute = type.Type.GetCustomAttribute<KlaxComponentAttribute>();

					if (attribute.HideInEditor)
						continue;

					CAddComponentEntryViewModel vm = new CAddComponentEntryViewModel(type.Name, type.Type);

					if (components.TryGetValue(attribute.Category, out List<CAddComponentEntryViewModel> list))
					{
						list.Add(vm);
					}
					else
					{
						List<CAddComponentEntryViewModel> newList = new List<CAddComponentEntryViewModel>(8);
						components.Add(attribute.Category, newList);

						newList.Add(vm);
					}
				}
			}

			List<CAddComponentCategoryViewModel> orderedList = new List<CAddComponentCategoryViewModel>(components.Count);

			foreach (var entry in components)
			{
				CAddComponentCategoryViewModel vm = new CAddComponentCategoryViewModel(entry.Key, entry.Value);
				orderedList.Add(vm);
			}

			orderedList = orderedList.OrderBy(p => p.Name).ToList();
			//Always show Common on top
			for (int i = 0, count = orderedList.Count; i < count; i++)
			{
				if (orderedList[i].Name == "Common")
				{
					ContainerUtilities.Swap(orderedList, i, 0);
					break;
				}
			}

			m_addComponentMenuCategories = new ObservableCollection<CAddComponentCategoryViewModel>(orderedList);
			m_entityCommands = new ObservableCollection<CEntityCommandViewModel>();

			m_entityCommands.Add(new CEntityCommandViewModel("Overwrite asset", new CRelayCommand((obj) =>
			{
				EntityCommandsMenuOpen = false;

				InspectedEntityAsset.SetEntity(InspectedEntity);
				CAssetRegistry.Instance.SaveAsset(InspectedEntityAsset);
			})));
		}

		public void AttachComponent(SEntityComponentId child, SEntityComponentId parent)
		{
			CSceneComponent parentObj = parent.GetComponent<CSceneComponent>();
			CSceneComponent childObj = child.GetComponent<CSceneComponent>();

			if (parentObj != null && childObj != null)
			{
				CSceneComponent oldParent = childObj.ParentComponent;
				SEntityComponentId oldParentid = new SEntityComponentId(oldParent, true);

				if (childObj.AttachToComponent(parentObj))
				{
					UpdateEntityInformation();
					InspectObject(DesiredTarget);

					void Undo()
					{
						CSceneComponent oldParentInst = oldParentid.GetComponent<CSceneComponent>();
						CSceneComponent childInst = child.GetComponent<CSceneComponent>();

						if (oldParentInst != null && childInst != null)
						{
							childInst.AttachToComponent(oldParentInst);
							UpdateEntityInformation();
						}
					}

					void Redo()
					{
						CSceneComponent newParentInst = parent.GetComponent<CSceneComponent>();
						CSceneComponent childInst = child.GetComponent<CSceneComponent>();

						if (newParentInst != null && childInst != null)
						{
							childInst.AttachToComponent(newParentInst);
							UpdateEntityInformation();
						}
					}

					CRelayUndoItem item = new CRelayUndoItem(Undo, Redo);
					UndoRedoUtility.Record(item);
				}
			}
		}

		private void InternalAddComponent(object argument)
		{
			AddComponentMenuOpen = false;

			if ((argument is Type type) && InspectedEntity != null)
			{
				InspectedEntity.AddComponent(type, true, false);

				UpdateEntityInformation();
				InspectObject(DesiredTarget);

				UndoRedoUtility.Purge(null);
			}
		}

		private void InternalRenameEntity(object argument)
		{
			string name = argument as string;

			EntityInfo[0].Name = name;
			EntityName = name;
		}

		private void InternalCreateVariable(object argument)
		{
			if (InspectedEntity != null)
			{
				CKlaxVariable newVariable = new CKlaxVariable()
				{
					Name = "NewVariable",
					Type = typeof(int),
					Value = 0
				};
				InspectedEntity.KlaxScriptObject.KlaxVariables.Add(newVariable);
				UpdateEntityInformation();
			}
		}

		private void InternalCreateLocalVariable(object argument)
		{
			if (OpenedGraph != null)
			{
				CKlaxVariable newVariable = new CKlaxVariable()
				{
					Name = "NewVariable",
					Type = typeof(int),
					Value = 0
				};
				OpenedGraph.LocalVariables.Add(newVariable);

				var viewmodel = new CEntityVariableViewModel(newVariable);
				viewmodel.DeleteCommand = new CRelayCommand(arg =>
				{
					OpenedGraph.LocalVariables.Remove(newVariable);
					LocalVariables.Remove(viewmodel);
				});

				LocalVariables.Add(viewmodel);
			}
		}

		private void InternalAddInterfaceInclude(object argument)
		{
			if (InspectedEntity != null)
			{
				CKlaxScriptInterfaceReference interfaceReference = new CKlaxScriptInterfaceReference();

				void Redo()
				{
					InspectedEntity.KlaxScriptObject.IncludedInterfaces.Add(interfaceReference);
					IncludedInterfaces.Add(new CIncludedInterfaceEntry(interfaceReference, this));
				}

				void Undo()
				{
					InspectedEntity.KlaxScriptObject.IncludedInterfaces.Remove(interfaceReference);
					IncludedInterfaces.Remove(new CIncludedInterfaceEntry(interfaceReference, this));
				}

				Redo();
				UndoRedoUtility.Record(new CRelayUndoItem(Undo, Redo));
			}
		}

		private void InspectEntity(SEntityId entityId)
		{
			CEngine engine = CEngine.Instance;

			CEntity entity = entityId.OverrideEntity;
			if (entity != null)
			{
				DesiredTarget = new CEditableObject(entityId);
				UnmarkEverything();
				SetEntityMarked(true);
			}

			UpdateProperties();
		}

		private void InspectComponent(SEntityComponentId componentId)
		{
			CEntityComponent component = componentId.GetComponent();
			if (component != null)
			{
				DesiredTarget = new CEditableObject(componentId);
				UnmarkEverything();
				SetComponentMarked(componentId, true);

			}
			else
			{
				DesiredTarget = null;
				UnmarkEverything();
			}

			UpdateProperties();
		}

		private void UnmarkEverything()
		{
			SetComponentMarked(SEntityComponentId.Invalid, false);
			SetEntityMarked(false);
		}

		private void SetComponentMarked(SEntityComponentId componentId, bool bMark)
		{
			bool bMarkEverything = componentId == SEntityComponentId.Invalid;

			void Mark(CInspectorSceneComponentViewModel sceneVm)
			{
				if (sceneVm.ComponentId == componentId || bMarkEverything)
					sceneVm.SetViewModelMarked(bMark);

				foreach (var child in sceneVm.Children)
					Mark(child);
			}

			if (SceneComponents.Count > 0)
			{
				Mark(SceneComponents[0]);
			}

			foreach (var entComp in EntityComponents)
			{
				if (entComp.ComponentId == componentId || bMarkEverything)
					entComp.SetViewModelMarked(bMark);
			}
		}

		private void SetEntityMarked(bool bMark)
		{
			if (EntityInfo.Count > 0)
			{
				EntityInfo[0].SetViewModelMarked(bMark);
			}
		}

		private void Reset()
		{
			void DeselectSceneComponents(CInspectorSceneComponentViewModel vm)
			{
				vm.IsSelected = false;
				foreach (var child in vm.Children)
					DeselectSceneComponents(child);
			}

			if (SceneComponents.Count > 0)
			{
				DeselectSceneComponents(SceneComponents[0]);
			}

			if (EntityInfo.Count > 0)
			{
				EntityInfo[0].IsSelected = false;
			}

			foreach (var entityComp in EntityComponents)
			{
				entityComp.IsSelected = false;
			}

			UpdateProperties();
			DesiredTarget = null;
		}

		private void UpdateProperties()
		{
			if (DesiredTarget == null)
				return;

			object target = DesiredTarget.GetEngineObject_EngineThread();
			CObjectProperties properties = EditorHelpers.GetObjectProperties(target);

			if (properties != null)
			{
				if (m_view != null && properties.properties.Count > 0)
				{
					m_view.ShowInspectors(properties.properties);
				}
			}
		}

		public void QueueEntityInformationUpdate(SEntityId entityId, bool bReselectTarget = false)
		{
			UpdateEntityInformation();
		}

		public void UpdateEntityInformation()
		{
			void CreateViewModel(CSceneComponent component, CInspectorSceneComponentViewModel parent, CEntity owner)
			{
				if (component.Owner != owner || !component.ShowInInspector || component.MarkedForDestruction)
				{
					return;
				}

				SEntityComponentId id = new SEntityComponentId(component, true);

				CInspectorSceneComponentViewModel vm = new CEBInspectorSceneComponentViewModel(this, component.Name, id, "builderSceneComponent");
				parent.Children.Add(vm);

				foreach (var child in component.Children)
				{
					CreateViewModel(child, vm, owner);
				}
			}

			if (InspectedEntity != null)
			{
				CEBInspectorEntityViewModel entityInfo = null;
				CInspectorSceneComponentViewModel sceneComponentsInfo = null;
				List<CInspectorEntityComponentViewModel> entityComponentsInfo = null;

				sceneComponentsInfo = null;
				if (InspectedEntity.RootComponent != null)
				{
					sceneComponentsInfo = new CEBInspectorSceneComponentViewModel(this, InspectedEntity.RootComponent.Name, new SEntityComponentId(InspectedEntity.RootComponent, true), "builderSceneComponent");
					foreach (var child in InspectedEntity.RootComponent.Children)
					{
						CreateViewModel(child, sceneComponentsInfo, InspectedEntity);
					}
				}

				List<CEntityComponent> components = new List<CEntityComponent>(8);
				entityComponentsInfo = new List<CInspectorEntityComponentViewModel>(4);
				InspectedEntity.GetComponents(components);

				for (int i = components.Count - 1; i >= 0; i--)
				{
					if (!(components[i] is CSceneComponent) && components[i].ShowInInspector && !components[i].MarkedForDestruction)
					{
						entityComponentsInfo.Add(new CEBInspectorEntityComponentViewModel(this, components[i].Name, new SEntityComponentId(components[i], true)));
					}
				}

				entityInfo = new CEBInspectorEntityViewModel(this, InspectedEntity.Name, new SEntityId(InspectedEntity, true));

				List<CEntityVariableViewModel> variables = new List<CEntityVariableViewModel>(InspectedEntity.KlaxScriptObject.KlaxVariables.Count);
				for (int i = 0, count = InspectedEntity.KlaxScriptObject.KlaxVariables.Count; i < count; i++)
				{
					CKlaxVariable variable = InspectedEntity.KlaxScriptObject.KlaxVariables[i];
					variables.Add(new CEntityVariableViewModel(variable)
					{
						DeleteCommand = new CRelayCommand(arg =>
						{
							InspectedEntity.KlaxScriptObject.KlaxVariables.Remove(variable);
							UpdateEntityInformation();
						})
					});
				}

				List<CScriptEventGraphEntryViewmodel> eventGraphs = new List<CScriptEventGraphEntryViewmodel>(InspectedEntity.KlaxScriptObject.EventGraphs.Count);
				foreach (CEventGraph eventGraph in InspectedEntity.KlaxScriptObject.EventGraphs)
				{
					eventGraphs.Add(new CScriptEventGraphEntryViewmodel(eventGraph, this));
				}

				List<CInterfaceGraphEntryViewmodel> interfaceGraphs = new List<CInterfaceGraphEntryViewmodel>(InspectedEntity.KlaxScriptObject.InterfaceGraphs.Count);
				foreach (CInterfaceFunctionGraph interfaceGraph in InspectedEntity.KlaxScriptObject.InterfaceGraphs)
				{
					interfaceGraphs.Add(new CInterfaceGraphEntryViewmodel(interfaceGraph, this));
				}

				List<CIncludedInterfaceEntry> includedInterfaces = new List<CIncludedInterfaceEntry>(InspectedEntity.KlaxScriptObject.IncludedInterfaces.Count);
				foreach (var interfaceAsset in InspectedEntity.KlaxScriptObject.IncludedInterfaces)
				{
					CIncludedInterfaceEntry interfaceEntry = new CIncludedInterfaceEntry(interfaceAsset, this);
					includedInterfaces.Add(interfaceEntry);
				}

				List<CFunctionGraphEntryViewModel> functionGraphs = new List<CFunctionGraphEntryViewModel>(InspectedEntity.KlaxScriptObject.FunctionGraphs.Count);
				foreach (var functionGraph in InspectedEntity.KlaxScriptObject.FunctionGraphs)
				{
					CFunctionGraphEntryViewModel functionGraphEntry = new CFunctionGraphEntryViewModel(functionGraph, this);
					functionGraphs.Add(functionGraphEntry);
				}

				DisplayEntityInformation(sceneComponentsInfo, entityComponentsInfo, entityInfo, variables, eventGraphs, includedInterfaces, interfaceGraphs, functionGraphs);
				UpdatePossibleEvents(InspectedType, InspectedEntity);
				UpdatePossibleInterfaceFunctions(InspectedEntity.KlaxScriptObject);

				RaisePropertyChanged(nameof(EntityName));
				m_view.SetEntityName(InspectedEntity.Name);
			}
			else
			{
				//Clear component list when selecting invalid entity
				DisplayEntityInformation(null, null, null, null, null, null, null, null); 
			}
		}

		private void DisplayEntityInformation(CInspectorSceneComponentViewModel sceneComponentRoot,
			List<CInspectorEntityComponentViewModel> entityComponents,
			CEBInspectorEntityViewModel entity,
			List<CEntityVariableViewModel> variables,
			List<CScriptEventGraphEntryViewmodel> eventGraphs,
			List<CIncludedInterfaceEntry> includedInterfaces,
			List<CInterfaceGraphEntryViewmodel> interfaceGraphs,
			List<CFunctionGraphEntryViewModel> functionGraphs)
		{
			EntityVariables.Clear();
			SceneComponents.Clear();
			EntityComponents.Clear();
			EventGraphs.Clear();
			EntityInfo.Clear();
			IncludedInterfaces.Clear();

			if (sceneComponentRoot != null)
			{
				SceneComponents.Add(sceneComponentRoot);
			}

			if (entityComponents != null)
			{
				EntityComponents = new ObservableCollection<CInspectorEntityComponentViewModel>(entityComponents);
			}

			if (entity != null)
			{
				EntityInfo.Add(entity);
			}

			if (variables != null)
			{
				EntityVariables = new ObservableCollection<CEntityVariableViewModel>(variables);
			}

			if (eventGraphs != null)
			{
				EventGraphs = new ObservableCollection<CScriptEventGraphEntryViewmodel>(eventGraphs);
			}

			if (includedInterfaces != null)
			{
				IncludedInterfaces = new ObservableCollection<CIncludedInterfaceEntry>(includedInterfaces);
			}

			if (interfaceGraphs != null)
			{
				InterfaceGraphs = new ObservableCollection<CInterfaceGraphEntryViewmodel>(interfaceGraphs);
			}

			if (functionGraphs != null)
			{
				FunctionGraphs = new ObservableCollection<CFunctionGraphEntryViewModel>(functionGraphs);
			}
		}

		private void UpdatePossibleEvents(CKlaxScriptTypeInfo inspectedType, CEntity inspectedEntity)
		{
			PossibleEvents.Clear();
			foreach (var eventInfo in inspectedType.Events)
			{
				if (EventGraphs.Any((e) => e.ScriptGraph.TargetEvent.klaxEventInfo == eventInfo.klaxEventInfo))
				{
					continue;
				}

				PossibleEvents.Add(new CKlaxEventEntryViewmodel(eventInfo, this));
			}

			List<CEntityComponent> components = new List<CEntityComponent>();
			inspectedEntity.GetComponents(components);
			CKlaxScriptRegistry scriptRegistry = CKlaxScriptRegistry.Instance;
			foreach (var component in components)
			{
				if (scriptRegistry.TryGetTypeInfo(component.GetType(), out CKlaxScriptTypeInfo componentType))
				{
					foreach (var eventInfo in componentType.Events)
					{
						if (EventGraphs.Any((e) => e.ScriptGraph.TargetEvent.klaxEventInfo == eventInfo.klaxEventInfo && e.ScriptGraph.TargetComponentGuid == component.ComponentGuid))
						{
							continue;
						}

						PossibleEvents.Add(new CKlaxEventEntryViewmodel(eventInfo, component, this));
					}
				}
			}
		}

		public void UpdatePossibleInterfaceFunctions(CKlaxScriptObject scriptObject)
		{
			PossibleInterfaceFunctions.Clear();
			if (scriptObject == null)
			{
				return;
			}

			foreach (var scriptInterfaceReference in scriptObject.IncludedInterfaces)
			{
				CKlaxScriptInterface klaxInterface = scriptInterfaceReference.GetInterface();
				string interfaceAssetName = scriptInterfaceReference.InterfaceAsset.GetAsset().Name;
				foreach (var interfaceFunction in klaxInterface.Functions)
				{
					if (scriptObject.InterfaceGraphs.Any(e => e.InterfaceFunctionGuid == interfaceFunction.Guid))
					{
						continue;
					}

					PossibleInterfaceFunctions.Add(new CKlaxInterfaceFunctionEntry(interfaceFunction, interfaceAssetName, this));
				}
			}
		}

		public void AddEventGraph(CKlaxScriptEventInfo eventInfo)
		{
			CEventGraph newGraph = new CEventGraph(eventInfo);
			newGraph.ScriptableObject = InspectedEntity.KlaxScriptObject;
			InspectedEntity.KlaxScriptObject.EventGraphs.Add(newGraph);
			EventGraphs.Add(new CScriptEventGraphEntryViewmodel(newGraph, this));
			UpdatePossibleEvents(InspectedType, InspectedEntity);
		}

		public void AddEventGraph(CKlaxScriptEventInfo eventInfo, Guid sourceComponentGuid, string componentName)
		{
			CEventGraph newGraph = new CEventGraph(eventInfo, sourceComponentGuid, componentName);
			newGraph.ScriptableObject = InspectedEntity.KlaxScriptObject;
			InspectedEntity.KlaxScriptObject.EventGraphs.Add(newGraph);
			EventGraphs.Add(new CScriptEventGraphEntryViewmodel(newGraph, this));
			UpdatePossibleEvents(InspectedType, InspectedEntity);
		}

		public void AddInterfaceGraph(CKlaxScriptInterfaceFunction interfaceFunction, string name)
		{
			CInterfaceFunctionGraph interfaceGraph = new CInterfaceFunctionGraph(interfaceFunction);
			interfaceGraph.ScriptableObject = InspectedEntity.KlaxScriptObject;
			interfaceGraph.Name = name;
			
			InspectedEntity.KlaxScriptObject.InterfaceGraphs.Add(interfaceGraph);
			InterfaceGraphs.Add(new CInterfaceGraphEntryViewmodel(interfaceGraph, this));
			UpdatePossibleInterfaceFunctions(InspectedEntity.KlaxScriptObject);
		}

		private void OnAddFunctionGraph(object e)
		{
			CCustomFunctionGraph functionGraph = new CCustomFunctionGraph();
			functionGraph.ScriptableObject = InspectedEntity.KlaxScriptObject;
			functionGraph.Name = "NewFunction";

			InspectedEntity.KlaxScriptObject.FunctionGraphs.Add(functionGraph);
			FunctionGraphs.Add(new CFunctionGraphEntryViewModel(functionGraph, this));
		}

		public void RenameEventGraph(int index, string newName)
		{
			if (index >= 0 && EventGraphs.Count > index)
			{
				InspectedEntity.KlaxScriptObject.EventGraphs[index].Name = newName;
			}
		}

		public void RenameInterfaceGraph(int index, string newName)
		{
			if (index >= 0 && EventGraphs.Count > index)
			{
				InspectedEntity.KlaxScriptObject.InterfaceGraphs[index].Name = newName;
			}
		}

		public void DeleteEventGraph(int index)
		{
			if (index >= 0 && EventGraphs.Count > index)
			{
				CEventGraph deletedGraph = InspectedEntity.KlaxScriptObject.EventGraphs[index];

				InspectedEntity.KlaxScriptObject.EventGraphs.RemoveAt(index);
				EventGraphs.RemoveAt(index);

				if (OpenedGraph == deletedGraph)
				{
					CloseCurrentGraph();
				}

				UpdatePossibleEvents(InspectedType, InspectedEntity);
			}
		}

		public void DeleteInterfaceGraph(int index)
		{
			if (index >= 0 && InterfaceGraphs.Count > index)
			{
				CGraph deletedGraph = InspectedEntity.KlaxScriptObject.InterfaceGraphs[index];

				InspectedEntity.KlaxScriptObject.InterfaceGraphs.RemoveAt(index);
				InterfaceGraphs.RemoveAt(index);

				if (OpenedGraph == deletedGraph)
				{
					CloseCurrentGraph();
				}

				UpdatePossibleInterfaceFunctions(InspectedEntity.KlaxScriptObject);
			}
		}

		public void DeleteFunctionGraph(int index)
		{
			if (index >= 0 && FunctionGraphs.Count > index)
			{
				CGraph deletedGraph = InspectedEntity.KlaxScriptObject.FunctionGraphs[index];

				InspectedEntity.KlaxScriptObject.FunctionGraphs.RemoveAt(index);
				FunctionGraphs.RemoveAt(index);

				if (OpenedGraph == deletedGraph)
				{
					CloseCurrentGraph();
				}				
			}
		}

		private void CloseCurrentGraph()
		{
			CNodeGraphViewModel nodeGraph = CWorkspace.Instance.GetTool<CNodeGraphViewModel>();
			nodeGraph.CloseGraph();
			OpenedGraph = null;
		}

		private ObservableCollection<CEBInspectorEntityViewModel> m_entityInfo = new ObservableCollection<CEBInspectorEntityViewModel>();
		public ObservableCollection<CEBInspectorEntityViewModel> EntityInfo
		{
			get { return m_entityInfo; }
			set
			{
				m_entityInfo = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CInspectorSceneComponentViewModel> m_sceneComponents = new ObservableCollection<CInspectorSceneComponentViewModel>();
		public ObservableCollection<CInspectorSceneComponentViewModel> SceneComponents
		{
			get { return m_sceneComponents; }
			set
			{
				m_sceneComponents = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CInspectorEntityComponentViewModel> m_entityComponents = new ObservableCollection<CInspectorEntityComponentViewModel>();
		public ObservableCollection<CInspectorEntityComponentViewModel> EntityComponents
		{
			get { return m_entityComponents; }
			set
			{
				m_entityComponents = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CEntityVariableViewModel> m_entityVariables = new ObservableCollection<CEntityVariableViewModel>();
		public ObservableCollection<CEntityVariableViewModel> EntityVariables
		{
			get { return m_entityVariables; }
			set
			{
				m_entityVariables = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CEntityVariableViewModel> m_localVariables = new ObservableCollection<CEntityVariableViewModel>();
		public ObservableCollection<CEntityVariableViewModel> LocalVariables
		{
			get { return m_localVariables; }
			set
			{
				m_localVariables = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CScriptEventGraphEntryViewmodel> m_eventGraphs = new ObservableCollection<CScriptEventGraphEntryViewmodel>();
		public ObservableCollection<CScriptEventGraphEntryViewmodel> EventGraphs
		{
			get { return m_eventGraphs; }
			set
			{
				m_eventGraphs = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CKlaxEventEntryViewmodel> m_possibleEvents = new ObservableCollection<CKlaxEventEntryViewmodel>();
		public ObservableCollection<CKlaxEventEntryViewmodel> PossibleEvents
		{
			get { return m_possibleEvents; }
			set
			{
				m_possibleEvents = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CInterfaceGraphEntryViewmodel> m_interfaceGraphs = new ObservableCollection<CInterfaceGraphEntryViewmodel>();

		public ObservableCollection<CInterfaceGraphEntryViewmodel> InterfaceGraphs
		{
			get { return m_interfaceGraphs; }
			set
			{
				m_interfaceGraphs = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CFunctionGraphEntryViewModel> m_functionGraphs = new ObservableCollection<CFunctionGraphEntryViewModel>();
		public ObservableCollection<CFunctionGraphEntryViewModel> FunctionGraphs
		{
			get { return m_functionGraphs; }
			set
			{
				m_functionGraphs = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CKlaxInterfaceFunctionEntry> m_possibleInterfaceFunctions = new ObservableCollection<CKlaxInterfaceFunctionEntry>();
		public ObservableCollection<CKlaxInterfaceFunctionEntry> PossibleInterfaceFunctions
		{
			get { return m_possibleInterfaceFunctions; }
			set
			{
				m_possibleInterfaceFunctions = value;
				RaisePropertyChanged();
			}
		}

		private string m_entityName;
		public string EntityName
		{
			get { return m_entityName; }
			set
			{
				if (m_entityName != value)
				{
					m_entityName = value;
					RaisePropertyChanged();

					if (EntityInfo.Count > 0)
					{
						SEntityId entityId = EntityInfo[0].EntityId;
						CEntity entity = entityId.GetEntity();
						if (entity != null)
						{
							entity.Name = value;
						}

						EntityInfo[0].Name = value;
					}
				}
			}
		}

		private string m_assetName;
		public string AssetName
		{
			get { return m_assetName; }
			set { m_assetName = value; RaisePropertyChanged(); }
		}

		private bool m_bAddComponentMenuOpen;
		public bool AddComponentMenuOpen
		{
			get { return m_bAddComponentMenuOpen; }
			set
			{
				if (m_bAddComponentMenuOpen != value)
				{
					m_bAddComponentMenuOpen = value;
					RaisePropertyChanged();
				}
			}
		}

		private ObservableCollection<CAddComponentCategoryViewModel> m_addComponentMenuCategories;
		public ObservableCollection<CAddComponentCategoryViewModel> AddComponentMenuCategories
		{
			get { return m_addComponentMenuCategories; }
			set
			{
				m_addComponentMenuCategories = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CIncludedInterfaceEntry> m_includedInterfaces = new ObservableCollection<CIncludedInterfaceEntry>();
		public ObservableCollection<CIncludedInterfaceEntry> IncludedInterfaces
		{
			get { return m_includedInterfaces; }
			set
			{
				m_includedInterfaces = value;
				RaisePropertyChanged();
			}
		}

		private bool m_bEntityCommandMenuOpen;
		public bool EntityCommandsMenuOpen
		{
			get { return m_bEntityCommandMenuOpen; }
			set
			{
				m_bEntityCommandMenuOpen = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CEntityCommandViewModel> m_entityCommands;
		public ObservableCollection<CEntityCommandViewModel> EntityCommands
		{
			get { return m_entityCommands; }
			set
			{
				m_entityCommands = value;
				RaisePropertyChanged();
			}
		}

		private CEntity m_inspectedEntity;
		public CEntity InspectedEntity
		{
			get { return m_inspectedEntity; }
			private set
			{
				m_inspectedEntity = value;
				RaisePropertyChanged();
			}
		}

		public CFunctionEditorViewModel FunctionEditor { get; } = new CFunctionEditorViewModel();

		private CGraph m_openedGraph;
		public CGraph OpenedGraph
		{
			get { return m_openedGraph; }
			set
			{
				m_openedGraph = value;
				if (m_openedGraph is CCustomFunctionGraph functionGraph)
				{
					FunctionEditor.OpenFunctionGraph(functionGraph);
				}
				else
				{
					FunctionEditor.CloseFunctionEditor();
				}
				RaisePropertyChanged();

				m_localVariables.Clear();
				if (m_openedGraph != null)
				{
					foreach (var variable in m_openedGraph.LocalVariables)
					{
						var viewmodel = new CEntityVariableViewModel(variable);
						viewmodel.DeleteCommand = new CRelayCommand(arg =>
						{
							OpenedGraph.LocalVariables.Remove(variable);
							LocalVariables.Remove(viewmodel);
						});
						LocalVariables.Add(viewmodel);
					}
				}
			}
		}

		public CEditableObject DesiredTarget { get; private set; }
		public CEntityAsset<CEntity> InspectedEntityAsset { get; private set; }
		public CKlaxScriptTypeInfo InspectedType { get; private set; }

		public ICommand RenameEntityCommand { get; private set; }
		public ICommand AddComponentCommand { get; private set; }
		public ICommand ToggleAddComponentMenuCommand { get; private set; }
		public ICommand ToggleEntityCommandsMenuCommand { get; private set; }
		public ICommand CreateVariableCommand { get; private set; }
		public ICommand CreateLocalVariableCommand { get; private set; }
		public ICommand AddInterfaceIncludeCommand { get; private set; }
		public ICommand AddFunctionGraphCommand { get; private set; }

		private IInspectorView m_view;
	}
}

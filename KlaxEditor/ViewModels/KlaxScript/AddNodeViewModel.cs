using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using KlaxCore.KlaxScript;

namespace KlaxEditor.ViewModels.KlaxScript
{
	class CCategoryViewModel : CViewModelBase
	{
		public CCategoryViewModel(string name, CAddNodeViewModel parentViewModel, CCategoryViewModel parentCategory)
		{
			Name = name;
			m_combinedCollection = new CompositeCollection();
			CollectionContainer categoriesContainer = new CollectionContainer();
			categoriesContainer.Collection = SubCategories;
			CollectionContainer nodesContainer = new CollectionContainer();
			nodesContainer.Collection = Nodes;
			CombinedCollection.Add(categoriesContainer);
			CombinedCollection.Add(nodesContainer);

			MouseDownCommand = new CRelayCommand(OnMouseDown);

			m_parentCategory = parentCategory;
			m_parentViewModel = parentViewModel;
		}

		public void ResetCategory()
		{
			foreach (var subCategory in SubCategories)
			{
				subCategory.ResetCategory();
			}

			foreach (var node in Nodes)
			{
				node.IsSelected = false;
			}
			
			IsSelected = false;
			IsExpanded = false;
		}

		public void SelectNextSubCategory()
		{
			if (m_parentCategory == null)
			{
				int categoryIndex = m_parentViewModel.Categories.IndexOf(this);
				if (m_parentViewModel.Categories.Count > categoryIndex + 1)
				{
					m_parentViewModel.Categories[categoryIndex + 1].SelectFirstNode();
				}				
			}
			else
			{

				int categoryIndex = m_parentCategory.SubCategories.IndexOf(this);
				if (m_parentCategory.SubCategories.Count > categoryIndex + 1)
				{
					m_parentCategory.SubCategories[categoryIndex + 1].SelectFirstNode();
				}
				else
				{
					m_parentCategory.SelectNextSubCategory();
				}
			}
		}

		public void SelectPreviousCategory()
		{
			if (m_parentCategory == null)
			{
				int categoryIndex = m_parentViewModel.Categories.IndexOf(this);
				if (categoryIndex > 0)
				{
					m_parentViewModel.Categories[categoryIndex - 1].SelectLastNode();
				}
			}
			else
			{

				int categoryIndex = m_parentCategory.SubCategories.IndexOf(this);
				if (categoryIndex > 0)
				{
					m_parentCategory.SubCategories[categoryIndex - 1].SelectLastNode();
				}
				else
				{
					m_parentCategory.SelectPreviousCategory();
				}
			}
		}

		public bool SelectFirstNode()
		{
			if (Nodes.Count > 0)
			{
				Nodes[0].IsSelected = true;
				return true;
			}

			foreach (var subCategory in SubCategories)
			{
				if (subCategory.SelectFirstNode())
				{
					return true;
				}
			}
			return false;
		}

		public bool SelectLastNode()
		{
			if (Nodes.Count > 0)
			{
				Nodes[Nodes.Count - 1].IsSelected = true;
				return true;
			}

			for (var index = SubCategories.Count - 1; index >= 0; index--)
			{
				var subCategory = SubCategories[index];
				if (subCategory.SelectLastNode())
				{
					return true;
				}
			}

			return false;
		}

		private void OnMouseDown(object e)
		{
			MouseButtonEventArgs args = (MouseButtonEventArgs) e;
			if (args.ChangedButton == MouseButton.Left)
			{
				IsExpanded = true;
			}
		}

		public string Name { get; set; }

		private ObservableCollection<CNodeEntryViewModel> m_nodes = new ObservableCollection<CNodeEntryViewModel>();
		public ObservableCollection<CNodeEntryViewModel> Nodes
		{
			get { return m_nodes; }
			set
			{
				m_nodes = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CCategoryViewModel> m_subCategories = new ObservableCollection<CCategoryViewModel>();
		public ObservableCollection<CCategoryViewModel> SubCategories
		{
			get { return m_subCategories; }
			set
			{
				m_subCategories = value;
				RaisePropertyChanged();
			}
		}

		private CompositeCollection m_combinedCollection;
		public CompositeCollection CombinedCollection
		{
			get { return m_combinedCollection; }
			set
			{
				m_combinedCollection = value;
				RaisePropertyChanged();
			}
		}

		private bool m_bIsSelected;
		public bool IsSelected
		{
			get { return m_bIsSelected; }
			set
			{
				m_bIsSelected = value;
				RaisePropertyChanged();
			}
		}

		private bool m_bIsExpanded;
		public bool IsExpanded
		{
			get { return m_bIsExpanded; }
			set
			{
				m_bIsExpanded = value;
				RaisePropertyChanged();
			}
		}

		public ICommand MouseDownCommand { get; set; }

		private CCategoryViewModel m_parentCategory;
		private CAddNodeViewModel m_parentViewModel;
	}

	class CNodeEntryViewModel : CViewModelBase
	{
		public CNodeEntryViewModel(CKlaxScriptNodeFactory nodeFactory, CAddNodeViewModel parentViewModel, CCategoryViewModel parentCategory)
		{
			NodeFactory = nodeFactory;
			Name = nodeFactory.Name;
			m_parentCategory = parentCategory;
			m_parentViewModel = parentViewModel;

			LeftDoubleClickCommand = new CRelayCommand(OnLeftDoubleClick);			
			
			if (nodeFactory.TargetType != null)
			{
				Tooltip = "Target is " + nodeFactory.TargetType.Name;
			}

			if (!string.IsNullOrWhiteSpace(nodeFactory.Tooltip))
			{
				if (Tooltip != null)
				{
					Tooltip = Tooltip + System.Environment.NewLine + nodeFactory.Tooltip;
				}
				else
				{
					Tooltip = nodeFactory.Tooltip;
				}
			}
		}

		public void SelectNextNode()
		{
			int nodeIndex = m_parentCategory.Nodes.IndexOf(this);
			if (m_parentCategory.Nodes.Count > nodeIndex + 1)
			{
				m_parentCategory.Nodes[nodeIndex + 1].IsSelected = true;
			}
			else
			{
				m_parentCategory.SelectNextSubCategory();
			}
		}

		public void SelectPreviousNode()
		{
			int nodeIndex = m_parentCategory.Nodes.IndexOf(this);
			if (nodeIndex > 0)
			{
				m_parentCategory.Nodes[nodeIndex - 1].IsSelected = true;
			}
			else
			{
				m_parentCategory.SelectPreviousCategory();
			}
		}

		private void OnLeftDoubleClick(object e)
		{
			m_parentViewModel.ConfirmNode(this);
		}

		public string Name { get; set; }

		private bool m_bIsSelected;
		public bool IsSelected
		{
			get { return m_bIsSelected; }
			set
			{
				if (m_bIsSelected != value)
				{
					m_bIsSelected = value;
					if (m_bIsSelected)
					{
						m_parentViewModel.SelectNode(this);
					}
					RaisePropertyChanged();
				}
			}
		}

		public bool IsExpanded { get; set; }
		public CKlaxScriptNodeFactory NodeFactory { get; }
		public string Tooltip { get; }

		public ICommand LeftDoubleClickCommand { get; set; }

		private CAddNodeViewModel m_parentViewModel;
		private CCategoryViewModel m_parentCategory;
	}

	class CAddNodeViewModel : CViewModelBase
	{
		public event Action<CNodeEntryViewModel> NodeSelected;

		public CAddNodeViewModel(CKlaxScriptNodeQueryContext context)
		{
			m_queryContext = context;
			CKlaxScriptRegistry.Instance.GetNodeSuggestions(m_queryContext, m_nodeFactories);

			EnterCommand = new CRelayCommand(OnEnterPressed);
			PreviewKeyDownCommand = new CRelayCommand(OnPreviewKeyDown);
			TreeGotFocusCommand = new CRelayCommand(OnTreeGotFocus);
			TreeLostFocusCommand = new CRelayCommand(OnTreeLostFocus);
			PopulatePossibleNodes();
		}

		public void SetContext(CKlaxScriptNodeQueryContext context)
		{
			if (CKlaxScriptNodeQueryContext.AreEqual(context, m_queryContext))
			{
				return;
			}

			m_queryContext = context;
			CKlaxScriptRegistry.Instance.GetNodeSuggestions(m_queryContext, m_nodeFactories);
			PopulatePossibleNodes();
		}

		public void SelectNode(CNodeEntryViewModel selectedNode)
		{
			m_selectedNode = selectedNode;
		}

		public void ConfirmNode(CNodeEntryViewModel confirmedNode)
		{
			NodeSelected?.Invoke(confirmedNode);
		}

		private void OnEnterPressed(object e)
		{
			if (m_selectedNode != null)
			{
				NodeSelected?.Invoke(m_selectedNode);
			}
		}

		private void PopulatePossibleNodes()
		{
			m_selectedNode = null;
			List<CCategoryViewModel> categories = new List<CCategoryViewModel>(128);
			Dictionary<string, CCategoryViewModel> categoryToViewModel = new Dictionary<string, CCategoryViewModel>();
			CCategoryViewModel memberCategory = new CCategoryViewModel("Variables", this, null);
			bool bFilterTextEmpty = string.IsNullOrWhiteSpace(FilterText);
			if (!bFilterTextEmpty)
			{
				memberCategory.IsExpanded = true;
			}

			foreach (CKlaxScriptNodeFactory nodeFactory in m_nodeFactories)
			{
				if (!bFilterTextEmpty
					&& nodeFactory.Name.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) < 0
					&& nodeFactory.Category.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) < 0)
				{
					continue;
				}

				if (nodeFactory.IsMemberNode)
				{
					string baseCategory = "Variables/" + nodeFactory.Category;
					if (!categoryToViewModel.TryGetValue(baseCategory, out CCategoryViewModel baseCategoryViewModel))
					{
						baseCategoryViewModel = new CCategoryViewModel(nodeFactory.Category, this, memberCategory);
						memberCategory.SubCategories.Add(baseCategoryViewModel);
						categoryToViewModel.Add(baseCategory, baseCategoryViewModel);

						if (!bFilterTextEmpty)
						{
							baseCategoryViewModel.IsExpanded = true;
						}
					}

					if (nodeFactory.TargetType != null)
					{
						string fullCategory = baseCategory + "/" + nodeFactory.TargetType.Name;
						if (!categoryToViewModel.TryGetValue(fullCategory, out CCategoryViewModel fullCategoryViewModel))
						{
							fullCategoryViewModel = new CCategoryViewModel(nodeFactory.TargetType.Name, this, baseCategoryViewModel);
							baseCategoryViewModel.SubCategories.Add(fullCategoryViewModel);
							categoryToViewModel.Add(fullCategory, fullCategoryViewModel);

							if (!bFilterTextEmpty)
							{
								fullCategoryViewModel.IsExpanded = true;
							}
						}
						fullCategoryViewModel.Nodes.Add(new CNodeEntryViewModel(nodeFactory, this, fullCategoryViewModel));
					}
					else
					{
						baseCategoryViewModel.Nodes.Add(new CNodeEntryViewModel(nodeFactory, this, baseCategoryViewModel));
					}
				}
				else
				{
					if (categoryToViewModel.TryGetValue(nodeFactory.Category, out CCategoryViewModel viewModel))
					{
						viewModel.Nodes.Add(new CNodeEntryViewModel(nodeFactory, this, viewModel));
					}
					else
					{
						CCategoryViewModel newCategory = new CCategoryViewModel(nodeFactory.Category, this, null);
						categories.Add(newCategory);
						categoryToViewModel.Add(nodeFactory.Category, newCategory);
						newCategory.Nodes.Add(new CNodeEntryViewModel(nodeFactory, this, newCategory));

						if (!bFilterTextEmpty)
						{
							newCategory.IsExpanded = true;
						}
					}
				}

			}

			if (memberCategory.Nodes.Count > 0 || memberCategory.SubCategories.Count > 0)
			{
				categories.Add(memberCategory);
			}

			Categories = new ObservableCollection<CCategoryViewModel>(categories);
			if (!bFilterTextEmpty && Categories.Count > 0)
			{
				SelectFirstEntry(Categories[0]);
			}
		}

		private void OnPreviewKeyDown(object e)
		{
			if (m_selectedNode == null || m_bHasTreeFocus)
			{
				return;
			}

			KeyEventArgs args = (KeyEventArgs)e;
			switch (args.Key)
			{
				case Key.Down:
					m_selectedNode.SelectNextNode();
					break;
				case Key.Up:
					m_selectedNode.SelectPreviousNode();
					break;
			}
		}

		private void OnTreeGotFocus(object e)
		{
			m_bHasTreeFocus = true;
		}

		private void OnTreeLostFocus(object e)
		{
			m_bHasTreeFocus = false;
		}

		private bool SelectFirstEntry(CCategoryViewModel category)
		{
			foreach (var subCategory in category.SubCategories)
			{
				if (SelectFirstEntry(subCategory))
				{
					return true;
				}
			}

			if (category.Nodes.Count > 0)
			{
				category.Nodes[0].IsSelected = true;
				return true;
			}

			return false;
		}

		private void ResetTreeItems()
		{
			m_selectedNode = null;
			foreach (var category in Categories)
			{
				category.ResetCategory();
			}
		}

		private ObservableCollection<CCategoryViewModel> m_categories;
		public ObservableCollection<CCategoryViewModel> Categories
		{
			get { return m_categories; }
			set
			{
				m_categories = value;
				RaisePropertyChanged();
			}
		}

		private string m_filterText;
		public string FilterText
		{
			get { return m_filterText; }
			set
			{
				if (value != m_filterText)
				{
					m_filterText = value;
					PopulatePossibleNodes();
					RaisePropertyChanged();
				}
			}
		}

		private bool m_bIsOpen;
		public bool IsOpen
		{
			get { return m_bIsOpen; }
			set
			{
				m_bIsOpen = value;
				if (value)
				{
					if (string.IsNullOrWhiteSpace(FilterText))
					{
						ResetTreeItems();
					}
					else
					{
						FilterText = "";
					}
				}
				RaisePropertyChanged();
			}
		}

		public ICommand EnterCommand { get; set; }
		public ICommand PreviewKeyDownCommand { get; set; }
		public ICommand TreeGotFocusCommand { get; set; }
		public ICommand TreeLostFocusCommand { get; set; }

		private CNodeEntryViewModel m_selectedNode;
		private readonly List<CKlaxScriptNodeFactory> m_nodeFactories = new List<CKlaxScriptNodeFactory>(128);
		private CKlaxScriptNodeQueryContext m_queryContext;
		private bool m_bHasTreeFocus;
	}
}

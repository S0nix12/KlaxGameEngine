using KlaxEditor.ViewModels;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfSharpDxControl;

namespace KlaxEditor.ViewModels
{
	class CEditorWindowViewModel : CViewModelBase
	{
		public CEditorWindowViewModel(string name)
		{
			Title = name;
			Name = name;
			ContentId = GetType().FullName;
		}

		public virtual void PostToolInit()
		{

		}

		public virtual void PostWorldLoad()
		{

		}

		protected void SetIconSourcePath(string path)
		{
			BitmapImage bi = new BitmapImage(new Uri("pack://application:,,,/" + path));
			IconSource = bi;
		}

		public bool CanBeInvisible { get; protected set; } = true;

		private string _title = null;
		public string Title
		{
			get { return _title; }
			set
			{
				if (_title != value)
				{
					_title = value;
					RaisePropertyChanged();
				}
			}
		}

		public ImageSource IconSource
		{
			get;
			protected set;
		}

		private string _contentId = null;
		public string ContentId
		{
			get { return _contentId; }
			set
			{
				if (_contentId != value)
				{
					_contentId = value;
					RaisePropertyChanged();
				}
			}
		}

		protected virtual void OnSelectedChanged(bool bNewValue) { }
		private bool _isSelected = false;
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					OnSelectedChanged(_isSelected);
					RaisePropertyChanged();
				}
			}
		}

		protected virtual void OnActiveChanged(bool bNewValue) { }
		private bool _isActive = false;
		public bool IsActive
		{
			get { return _isActive; }
			set
			{
				if (_isActive != value)
				{
					_isActive = value;
					OnActiveChanged(_isActive);
					RaisePropertyChanged();
				}
			}
		}

		private bool m_bIsMaximized;
		public bool IsMaximized
		{
			get { return m_bIsMaximized; }
			set
			{
				if (m_bIsMaximized != value)
				{
					m_bIsMaximized = value;

					RaisePropertyChanged();
				}
			}
		}

		private string m_name;
		public string Name
		{
			get { return m_name; }
			set
			{
				m_name = value;
				RaisePropertyChanged();
			}
		}

		private bool _isVisible = true;
		public bool IsVisible
		{
			get { return _isVisible; }
			set
			{
				if (_isVisible != value && (value || CanBeInvisible))
				{
					_isVisible = value;
					RaisePropertyChanged();
				}
			}
		}

		private UIElement m_content;
		public UIElement Content
		{
			get { return m_content; }
			set
			{
				m_content = value;
				RaisePropertyChanged();
			}
		}

		private bool m_bIsAlwaysHidden = false;
		public bool IsAlwaysHidden
		{
			get { return m_bIsAlwaysHidden; }
			set
			{
				m_bIsAlwaysHidden = value;
				RaisePropertyChanged();
			}
		}
	}
}

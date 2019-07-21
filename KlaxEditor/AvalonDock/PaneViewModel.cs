using KlaxEditor.ViewModels;
using System.Windows.Media;

namespace KlaxEditor
{
    class CPaneViewModel : CViewModelBase
    {
        public CPaneViewModel()
        { }


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

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}

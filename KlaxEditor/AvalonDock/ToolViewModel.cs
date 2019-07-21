namespace KlaxEditor
{
    class CToolViewModel : CPaneViewModel
    {
        public CToolViewModel(string name)
        {
            Name = name;
            Title = name;
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


        #region IsVisible

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion


    }
}

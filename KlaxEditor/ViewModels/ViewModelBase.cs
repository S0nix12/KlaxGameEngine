using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KlaxEditor.ViewModels
{
    public class CViewModelBase : INotifyPropertyChanged
    {

        protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}

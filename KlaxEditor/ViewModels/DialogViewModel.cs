using KlaxEditor;
using KlaxEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace KlaxEditor.ViewModels
{
    class CDialogViewModel : CViewModelBase
    {
        public CDialogViewModel(string message, MessageBoxButton buttons, Action closeAction, ImageSource source = null)
        {
            m_closeAction = closeAction;
            m_messageBoxButtons = buttons;

            Message = message;
            Image = source;

            AssignButtonTexts(buttons);
            SetupCommands();
        }

        private void AssignButtonTexts(MessageBoxButton buttons)
        {
            bool secondVisibility = false;
            bool thirdVisibility = false;

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    PrimaryButtonText = "Ok";
                    break;
                case MessageBoxButton.OKCancel:
                    PrimaryButtonText = "Ok";
                    secondVisibility = true;
                    SecondaryButtonText = "Cancel";
                    break;
                case MessageBoxButton.YesNoCancel:
                    PrimaryButtonText = "Yes";
                    SecondaryButtonText = "No";
                    TertiaryButtonText = "Cancel";
                    secondVisibility = true;
                    thirdVisibility = true;
                    break;
                case MessageBoxButton.YesNo:
                    PrimaryButtonText = "Yes";
                    SecondaryButtonText = "No";
                    secondVisibility = true;
                    break;
            }

            SecondaryButtonVisibility = secondVisibility;
            TertiaryButtonVisibility = thirdVisibility;
        }

        private void SetupCommands()
        {
            PrimaryButtonClicked = new CRelayCommand(OnPrimaryButtonClicked);
            SecondaryButtonClicked = new CRelayCommand(OnSecondaryButtonClicked);
            TertiaryButtonClicked = new CRelayCommand(OnTertiaryButtonClicked);
        }

        private void OnPrimaryButtonClicked(object arguments)
        {
            Result = true;
            m_closeAction();
        }

        private void OnSecondaryButtonClicked(object arguments)
        {
            switch (m_messageBoxButtons)
            {
                case MessageBoxButton.OKCancel:
                    Result = null;
                    break;
                case MessageBoxButton.YesNoCancel:
                case MessageBoxButton.YesNo:
                    Result = false;
                    break;
            }

            m_closeAction();
        }

        private void OnTertiaryButtonClicked(object arguments)
        {
            switch (m_messageBoxButtons)
            {
                case MessageBoxButton.YesNoCancel:
                    Result = null;
                    break;
            }

            m_closeAction();
        }

        private ImageSource m_image;
        public ImageSource Image
        {
            get { return m_image; }
            set
            {
                m_image = value;
                RaisePropertyChanged();
            }
        }

        private string m_message;
        public string Message
        {
            get { return m_message; }
            set
            {
                if (m_message == value) return;
                m_message = value;
                RaisePropertyChanged();
            }
        }

        private string m_primaryButtonText;
        public string PrimaryButtonText
        {
            get { return m_primaryButtonText; }
            set
            {
                if (m_primaryButtonText == value) return;
                m_primaryButtonText = value;
                RaisePropertyChanged();
            }
        }

        private string m_secondaryButtonText;
        public string SecondaryButtonText
        {
            get { return m_secondaryButtonText; }
            set
            {
                if (m_secondaryButtonText == value) return;
                m_secondaryButtonText = value;
                RaisePropertyChanged();
            }
        }

        private bool m_secondaryButtonVisibility;
        public bool SecondaryButtonVisibility
        {
            get { return m_secondaryButtonVisibility; }
            set
            {
                if (m_secondaryButtonVisibility == value) return;
                m_secondaryButtonVisibility = value;
                RaisePropertyChanged();
            }
        }

        private string m_tertiaryButtonText;
        public string TertiaryButtonText
        {
            get { return m_tertiaryButtonText; }
            set
            {
                if (m_tertiaryButtonText == value) return;
                m_tertiaryButtonText = value;
                RaisePropertyChanged();
            }
        }

        private bool m_tertiaryButtonVisibility;
        public bool TertiaryButtonVisibility
        {
            get { return m_tertiaryButtonVisibility; }
            set
            {
                if (m_tertiaryButtonVisibility == value) return;
                m_tertiaryButtonVisibility = value;
                RaisePropertyChanged();
            }
        }

        public ICommand PrimaryButtonClicked { get; private set; }
        public ICommand SecondaryButtonClicked { get; private set; }
        public ICommand TertiaryButtonClicked { get; private set; }

        public bool? Result { get; private set; }

        private MessageBoxButton m_messageBoxButtons;
        private Action m_closeAction;
    }
}

using KlaxConfig;
using KlaxConfig.Console;
using KlaxEditor;
using KlaxEditor.ViewModels;
using KlaxEditor.ViewModels.EditorWindows.Interfaces;
using KlaxIO.Log;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace KlaxEditor.ViewModels.EditorWindows
{
    class CConsoleViewModel : CEditorWindowViewModel
    {
        public CConsoleViewModel()
            : base("Console")
        {
			SetIconSourcePath("Resources/Images/Tabs/console.png");

			Content = new Views.Console();

            SetConsoleView = new CRelayCommand(obj => ConsoleView = obj as IConsoleView);
            CommandEntered = new CRelayCommand(OnConsoleCommandEntered);
            SuggestionChangedUp = new CRelayCommand(OnSuggestionChangedUp);
            SuggestionChangedDown = new CRelayCommand(OnSuggestionChangedDown);
            ClearSuggestions = new CRelayCommand(OnClearSuggestions);
            UpdateSuggestions = new CRelayCommand(OnUpdateSuggestions);

            LogUtility.Logged += OnLogged;
        }

        private void OnLogged(CLogger logger, string rawContent, string content)
        {
            m_logHistoryBuilder.Append(content);
            m_logHistoryBuilder.AppendLine();
            LogHistory = m_logHistoryBuilder.ToString();
        }

        private void OnConsoleCommandEntered(object textObject)
        {
            string text = textObject as string;

            if (SelectedConsoleSuggestion != -1)
            {
                ConsoleInputText = m_rawConsoleSuggestions[SelectedConsoleSuggestion];
                SelectedConsoleSuggestion = -1;
                ConsoleView.SelectEndOfConsoleInput();
                OnUpdateSuggestions(ConsoleInputText);
            }
            else if (text.StartsWith("?"))
            {
                string argument = ConsoleUtility.GetConsoleStringArgument(text.Substring(1), 0);
                if (argument != null)
                {
                    LogUtility.Log("----------------------------------");
                    LogUtility.Log("Suggestions for search term '{0}':", argument);
                    List<string> suggestions = new List<string>();
                    CConfigManager.Instance.GetConsoleSuggestionsContainingString(argument, ref suggestions);

                    foreach (string suggestion in suggestions)
                    {
                        LogUtility.Log(suggestion);
                    }
                    ConsoleInputText = "";
                }
            }
            else
            {
                ConsoleUtility.ProcessConsoleString(text);
                ConsoleInputText = "";
                OnClearSuggestions(null);
            }
        }

        private void OnSuggestionChangedUp(object arg)
        {
            int finalSelection = SelectedConsoleSuggestion - 1;

            if (finalSelection < 0)
            {
                finalSelection = ConsoleSuggestions.Count - 1;
            }

            SelectedConsoleSuggestion = finalSelection;
        }

        private void OnSuggestionChangedDown(object arg)
        {
            int finalSelection = SelectedConsoleSuggestion + 1;
            if (SelectedConsoleSuggestion == -1 || finalSelection > ConsoleSuggestions.Count - 1)
            {
                finalSelection = 0;
            }

            SelectedConsoleSuggestion = finalSelection;
        }

        private void OnClearSuggestions(object arg)
        {
            ConsoleSuggestions = new List<string>();
            m_rawConsoleSuggestions.Clear();
        }

        private void OnUpdateSuggestions(object arg)
        {
            if (arg is string argument)
            {
                if (string.IsNullOrWhiteSpace(argument))
                {
                    OnClearSuggestions(null);
                }
                else
                {
                    string firstArgument = ConsoleUtility.GetConsoleStringArgument(argument, 0);
                    if (firstArgument != null)
                    {
                        CConfigManager.Instance.GetConsoleSuggestions(firstArgument, ref m_consoleSuggestions, ref m_rawConsoleSuggestions);
                        RaisePropertyChanged(nameof(ConsoleSuggestions));
                    }
                }
            }

            SelectedConsoleSuggestion = -1;
        }

        private string m_logHistory = "";
        public string LogHistory
        {
            get { return m_logHistory; }
            set
            {
                m_logHistory = value;
                RaisePropertyChanged();
            }
        }

        private bool m_bPreviewVisibility = true;
        public bool PreviewVisibility
        {
            get { return m_bPreviewVisibility; }
            set
            {
                m_bPreviewVisibility = value;
                RaisePropertyChanged();
            }
        }

        private string m_consoleInputText = "";
        public string ConsoleInputText
        {
            get { return m_consoleInputText; }
            set
            {
                m_consoleInputText = value;
                RaisePropertyChanged();
            }
        }

        private int m_selectedConsoleSuggestion = -1;
        public int SelectedConsoleSuggestion
        {
            get { return m_selectedConsoleSuggestion; }
            set
            {
                m_selectedConsoleSuggestion = value;
                RaisePropertyChanged();
            }
        }

        private List<string> m_consoleSuggestions = new List<string>();
        public List<string> ConsoleSuggestions
        {
            get { return m_consoleSuggestions; }
            set
            {
                m_consoleSuggestions = value;
                RaisePropertyChanged();
            }
        }

        private IConsoleView m_consoleView;
        public IConsoleView ConsoleView
        {
            get { return m_consoleView; }
            set
            {
                m_consoleView = value;
                RaisePropertyChanged();
            }
        }

        public ICommand SetConsoleView { get; private set; }
        public ICommand CommandEntered { get; private set; }
        public ICommand SuggestionChangedUp { get; private set; }
        public ICommand SuggestionChangedDown { get; private set; }
        public ICommand UpdateSuggestions { get; private set; }
        public ICommand ClearSuggestions { get; private set; }

        private List<string> m_rawConsoleSuggestions = new List<string>();
        private readonly StringBuilder m_logHistoryBuilder = new StringBuilder();
    }
}

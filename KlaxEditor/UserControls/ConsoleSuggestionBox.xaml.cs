using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KlaxEditor.UserControls
{
    /// <summary>
    /// Interaction logic for ConsoleSuggestionBox.xaml
    /// </summary>
    public partial class ConsoleSuggestionBox : UserControl
    {
        public ConsoleSuggestionBox()
        {
            InitializeComponent();
        }

        private void UpdateDisplay()
        {
            foreach (TextBlock block in m_suggestions)
            {
                block.Visibility = Visibility.Collapsed;
            }

            while (m_suggestions.Count < Suggestions.Count)
            {
                TextBlock newBlock = new TextBlock();
                SuggestionPanel.Children.Add(newBlock);
                m_suggestions.Add(newBlock);
            }

            for (int i = 0; i < Suggestions.Count; i++)
            {
                TextBlock block = m_suggestions[i];
                block.Text = Suggestions[i];
                block.Visibility = Visibility.Visible;

                block.Background = new SolidColorBrush(Colors.Red);
                block.Foreground = new SolidColorBrush(Colors.White);
            }

            if (Index != -1 && Index < m_suggestions.Count)
            {
                m_suggestions[Index].Background = new SolidColorBrush(Colors.Green);
            }
        }

        private static void OnIndexChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            ConsoleSuggestionBox box = dependencyObject as ConsoleSuggestionBox;
            box.UpdateDisplay();
        }

        private static void OnSuggestionsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            ConsoleSuggestionBox box = dependencyObject as ConsoleSuggestionBox;
            box.UpdateDisplay();
        }

        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register("Index", typeof(int), typeof(ConsoleSuggestionBox), new PropertyMetadata(-1, OnIndexChanged));
        public static readonly DependencyProperty SuggestionsProperty = DependencyProperty.Register("Suggestions", typeof(List<string>), typeof(ConsoleSuggestionBox), new PropertyMetadata(new List<string>(), OnSuggestionsChanged));

        public int Index
        {
            get { return (int)this.GetValue(IndexProperty); }
            set { this.SetValue(IndexProperty, value); }
        }

        public List<string> Suggestions
        {
            get { return (List<string>)this.GetValue(SuggestionsProperty); }
            set { this.SetValue(SuggestionsProperty, value); }
        }


        private List<TextBlock> m_suggestions = new List<TextBlock>();
    }
}

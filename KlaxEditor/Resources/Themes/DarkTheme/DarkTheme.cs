using System;
using Xceed.Wpf.AvalonDock.Themes;

namespace KlaxEditor.Themes
{
    public class DarkTheme : Theme
    {
        public override Uri GetResourceUri()
        {
            return new Uri("pack://application:,,,/Resources/Themes/DarkTheme/Theme.xaml");
        }
    }
}

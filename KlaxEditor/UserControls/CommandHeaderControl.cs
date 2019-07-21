using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KlaxEditor.UserControls
{
	/// <summary>
	/// Header Control that exposes a text and a command property
	/// Default Style includes a TextBlock and a Button 
	/// </summary>
	public class CommandHeaderControl : HeaderedContentControl
	{
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text", typeof(string), typeof(CommandHeaderControl), new PropertyMetadata(default(string)));

		public string Text
		{
			get { return (string) GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
			"Command", typeof(ICommand), typeof(CommandHeaderControl), new PropertyMetadata(default(ICommand)));

		public ICommand Command
		{
			get { return (ICommand) GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(
			"ButtonStyle", typeof(Style), typeof(CommandHeaderControl), new PropertyMetadata(default(Style)));

		public Style ButtonStyle
		{
			get { return (Style) GetValue(ButtonStyleProperty); }
			set { SetValue(ButtonStyleProperty, value); }
		}
	}
}

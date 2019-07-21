using System;
using System.Windows;
using System.Windows.Controls;

namespace KlaxEditor.Views.KlaxScript
{
    class PinControl : UserControl
    {
		public static readonly DependencyProperty ConnectionPointProperty = DependencyProperty.Register(
			"ConnectionPoint", typeof(Point), typeof(PinControl), new PropertyMetadata(default(Point)));

		public Point ConnectionPoint
		{
			get { return (Point) GetValue(ConnectionPointProperty); }
			set { SetValue(ConnectionPointProperty, value); }
		}

		public static readonly DependencyProperty ParentNodeViewProperty = DependencyProperty.Register(
			"ParentNodeView", typeof(ScriptNodeView), typeof(PinControl), new PropertyMetadata(default(ScriptNodeView), ParentNodeChanged));

		private static void ParentNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			PinControl c = (PinControl) d;
			c.UpdateConnectionPoint();
		}

		public ScriptNodeView ParentNodeView
		{
			get { return (ScriptNodeView) GetValue(ParentNodeViewProperty); }
			set { SetValue(ParentNodeViewProperty, value); }
		}

		public PinControl()
		{
			LayoutUpdated += OnLayoutUpdated;
		}		

		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			UpdateConnectionPoint();
		}

		private void UpdateConnectionPoint()
		{
			if (ParentNodeView == null || !ParentNodeView.IsAncestorOf(this))
			{
				return;
			}

			Point centerPoint = new Point(ActualWidth / 2, ActualHeight / 2);
			Point newConnectionPoint = TransformToAncestor(ParentNodeView).Transform(centerPoint);
			if (newConnectionPoint != ConnectionPoint)
			{
				ConnectionPoint = TransformToAncestor(ParentNodeView).Transform(centerPoint);
			}
		}
	}
}

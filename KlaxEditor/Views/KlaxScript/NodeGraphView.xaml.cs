using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KlaxEditor.ViewModels.KlaxScript;
using SharpDX;
using Point = System.Windows.Point;

namespace KlaxEditor.Views.KlaxScript
{
    /// <summary>
    /// Interaction logic for NodeGraphView.xaml
    /// </summary>
    public partial class NodeGraphView : UserControl
    {
        public NodeGraphView()
        {
            InitializeComponent();
        }

		internal bool HitTestConnection(Point testPoint, out CPinViewModel hitPin, out Point hitCanvasCoords)
		{
			hitCanvasCoords = NodeCanvas.PointFromScreen(testPoint);
			Point hitTestPoint = NodeGraphOuter.PointFromScreen(testPoint);
			IInputElement hitElement = NodeGraphOuter.InputHitTest(hitTestPoint);
			if (hitElement is FrameworkElement uiElement)
			{
				if (uiElement.DataContext is CPinViewModel hitPinVM)
				{
					hitPin = hitPinVM;
					return true;
				}
			}

			hitPin = null;
			return false;
		}

		internal Point GetPasteReferenceLocation()
		{
			System.Drawing.Point mouse = System.Windows.Forms.Cursor.Position;
			Point mousePoint = new Point(mouse.X, mouse.Y);
			Point viewPoint = PointFromScreen(mousePoint);

			Point viewMax = new Point(ActualWidth, ActualHeight);
			Point canvasPoint = NodeCanvas.PointFromScreen(mousePoint);
			if (viewPoint.X > 0 && viewPoint.Y > 0 && viewPoint.X < viewMax.X && viewPoint.Y < viewMax.Y)
			{
				return canvasPoint;
			}

			// little hack here as nodes are pasted at their TopLeft Corner we offset the MaxBottomRight location so nodes are not pasted entirely offscreen
			double viewX = Math.Max(Math.Min(viewMax.X - 100, viewPoint.X), 0);
			double viewY = Math.Max(Math.Min(viewMax.Y - 100, viewPoint.Y), 0);

			GeneralTransform toCanvas = NodeGraphOuter.TransformToDescendant(NodeCanvas);
			return toCanvas.Transform(new Point(viewX, viewY));
		}

		internal Point GetMouseEventPosInNodeCanvas(MouseButtonEventArgs args)
		{
			return args.GetPosition(NodeCanvas);
		}
		
		private void NodeGraphView_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			Focus();
			if (e.ChangedButton != MouseButton.Left || e.Handled)
			{
				return;
			}

			m_isDrawinSelectionRect = true;
			m_selectionStartPoint = e.GetPosition(NodeCanvas);

			Canvas.SetLeft(SelectionRect, m_selectionStartPoint.X);
			Canvas.SetTop(SelectionRect, m_selectionStartPoint.Y);
			SelectionRect.Width = 0;
			SelectionRect.Height = 0;
			SelectionRect.Visibility = Visibility.Visible;
			MouseHook.OnMouseUp += OnMouseHookUp;
		}

		private void NodeGraphView_OnMouseMove(object sender, MouseEventArgs e)
		{
			if (m_isDrawinSelectionRect)
			{
				Point mousePos = e.GetPosition(NodeCanvas);
				if (mousePos.X > m_selectionStartPoint.X)
				{
					Canvas.SetLeft(SelectionRect, m_selectionStartPoint.X);
					SelectionRect.Width = mousePos.X - m_selectionStartPoint.X;
				}
				else
				{
					Canvas.SetLeft(SelectionRect, mousePos.X);
					SelectionRect.Width = m_selectionStartPoint.X - mousePos.X;
				}

				if (mousePos.Y > m_selectionStartPoint.Y)
				{
					Canvas.SetTop(SelectionRect, m_selectionStartPoint.Y);
					SelectionRect.Height = mousePos.Y - m_selectionStartPoint.Y;
				}
				else
				{
					Canvas.SetTop(SelectionRect, mousePos.Y);
					SelectionRect.Height = m_selectionStartPoint.Y - mousePos.Y;
				}				

				var rect = new RectangleGeometry(new Rect(m_selectionStartPoint, mousePos));
				var hitTestParams = new GeometryHitTestParameters(rect);				
				HashSet<CScriptNodeViewmodel> containedNodes = new HashSet<CScriptNodeViewmodel>();				
				var resultCallback = new HitTestResultCallback(element =>
				{
					return HitTestResultBehavior.Continue;
				});

				var filterCallback = new HitTestFilterCallback(element =>
				{
					if (element is FrameworkElement uiElement)
					{
						if (uiElement.DataContext is CScriptNodeViewmodel vm)
						{
							containedNodes.Add(vm);
							return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
						}
					}

					return HitTestFilterBehavior.Continue;
				});

				VisualTreeHelper.HitTest(NodeCanvas, filterCallback, resultCallback, hitTestParams);
				((CNodeGraphViewModel)DataContext).SelectNodes(containedNodes);
			}
		}

		private void OnMouseHookUp(object sender, Point p)
		{
			m_isDrawinSelectionRect = false;
			SelectionRect.Visibility = Visibility.Collapsed;
		}
		
		private Point m_selectionStartPoint;
		private bool m_isDrawinSelectionRect;

		private void NodeGraphView_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			Focus();
		}
	}
}

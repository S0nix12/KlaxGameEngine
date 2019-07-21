using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KlaxEditor.Views.KlaxScript
{
	enum LineOrientation
	{
		Horizontal,
		Vertical,
		Auto
	}

    class SmoothLineControl : Shape
    {
		public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(
			"StartPoint", typeof(Point), typeof(SmoothLineControl), new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));

		public Point StartPoint
		{
			get { return (Point) GetValue(StartPointProperty); }
			set { SetValue(StartPointProperty, value); }
		}

		public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
			"EndPoint", typeof(Point), typeof(SmoothLineControl), new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

		public Point EndPoint
		{
			get { return (Point) GetValue(EndPointProperty); }
			set { SetValue(EndPointProperty, value); }
		}

		public static readonly DependencyProperty LineOrientationProperty = DependencyProperty.Register(
			"LineOrientation", typeof(LineOrientation), typeof(SmoothLineControl), new FrameworkPropertyMetadata(default(LineOrientation), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

		public LineOrientation LineOrientation
		{
			get { return (LineOrientation) GetValue(LineOrientationProperty); }
			set { SetValue(LineOrientationProperty, value); }
		}

		private void CreatePath()
		{
			if (StartPoint == EndPoint)
			{
				m_pathGeometry = null;
				return;
			}

			m_pathGeometry = new PathGeometry();
			PathFigure figure = new PathFigure();

			Point startControl;
			Point endControl;

			switch (LineOrientation)
			{
				case LineOrientation.Horizontal:
					double deltaX = EndPoint.X - StartPoint.X;
					startControl = new Point(StartPoint.X + deltaX / 2, StartPoint.Y);
					endControl = new Point(EndPoint.X - deltaX / 2, EndPoint.Y);
					break;
				case LineOrientation.Vertical:
					double deltaY = EndPoint.Y - StartPoint.Y;
					startControl = new Point(StartPoint.X, StartPoint.Y + deltaY / 2);
					endControl = new Point(EndPoint.X, EndPoint.Y - deltaY / 2);
					break;
				case LineOrientation.Auto:
					double delX = EndPoint.X - StartPoint.X;
					double delY = EndPoint.Y - StartPoint.Y;
					if (Math.Abs(delX) > Math.Abs(delY))
					{
						startControl = new Point(StartPoint.X + delX / 2, StartPoint.Y);
						endControl = new Point(EndPoint.X - delX / 2, EndPoint.Y);
					}
					else
					{
						startControl = new Point(StartPoint.X, StartPoint.Y + delY / 2);
						endControl = new Point(EndPoint.X, EndPoint.Y - delY / 2);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			figure.IsClosed = false;
			figure.IsFilled = false;
			figure.StartPoint = StartPoint;

			BezierSegment segment = new BezierSegment(startControl, endControl, EndPoint, true);

			figure.Segments.Add(segment);
			m_pathGeometry.Figures.Add(figure);			
		}

		protected PathGeometry m_pathGeometry = new PathGeometry();
		protected override Geometry DefiningGeometry
		{
			get
			{
				CreatePath();
				if (m_pathGeometry == null)
				{
					return Geometry.Empty;
				}

				return m_pathGeometry;
			}
		}
	}
}

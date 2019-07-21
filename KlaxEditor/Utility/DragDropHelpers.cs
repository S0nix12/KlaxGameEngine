using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KlaxEditor.Utility
{
    public static class DragDropHelpers
    {
		public static bool IsMovementBiggerThreshold(Vector deltaMove)
		{
			return Math.Abs(deltaMove.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(deltaMove.Y) > SystemParameters.MinimumVerticalDragDistance;
		}
    }
}

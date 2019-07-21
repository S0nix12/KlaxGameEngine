using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxShared.Containers
{
	public static class ContainerExtensionMethods
	{
		public static void SetMinSize<T>(this List<T> list, int minSize)
		{
			int spaceNeeded = minSize - list.Count;
			if (spaceNeeded > 0)
			{
				if (list.Capacity < minSize)
				{
					list.Capacity = minSize;
				}

				for (int i = 0; i < spaceNeeded; i++)
				{
					list.Add(default);
				}
			}
		}
	}
}

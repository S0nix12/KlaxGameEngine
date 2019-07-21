using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxShared.Utilities
{
	public static class ContainerUtilities
	{
		public static void RemoveSwapAt<T>(IList<T> container, int index)
		{
			Debug.Assert(index >= 0);
			Debug.Assert(index < container.Count);			

			int lastIndex = container.Count - 1;

			if (index != lastIndex)
			{
				container[index] = container[container.Count - 1];
			}
			container.RemoveAt(lastIndex);
		}

		public static void Swap<T>(IList<T> container, int indexA, int indexB)
		{
			if (indexA == indexB)
				return;

			T temp = container[indexA];
			container[indexA] = container[indexB];
			container[indexB] = temp;
		}

        public static void AddUnique<T>(IList<T> container, T element)
        {
            if (!container.Contains(element))
            {
                container.Add(element);
            }
        }

		public static int CombineHashes(object a, object b)
		{
			int hash = 17;
			hash = hash * 31 + (a == null ? 0 : a.GetHashCode());
			hash = hash * 31 + (a == null ? 0 : b.GetHashCode());
			return hash;
		}
	}
}

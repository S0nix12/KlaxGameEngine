using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxShared
{
    public static class Extensions
    {
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            TValue val;

            if (!dict.TryGetValue(key, out val))
            {
                val = new TValue();
                dict.Add(key, val);
            }

            return val;
        }

		public static object GetDefaultValue(this Type type)
		{
			return type.IsValueType ? Activator.CreateInstance(type) : null;
		}

		public static double GetElapsedAsMilliseconds(this Stopwatch watch)
		{
			double seconds = (double)watch.ElapsedTicks / Stopwatch.Frequency;
			return seconds * 1000.0;
		}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxShared
{
    public class CInitializer
    {
        public void Add<T>(T instance)
        {
            if (m_objects.ContainsKey(typeof(T)))
            {
                throw new ArgumentException(string.Format("Cannot add two instances of type {0} to an engine initializer.", typeof(T).Name));
            }
            else
            {
                m_objects.Add(typeof(T), instance);
            }
        }

        public T Get<T>()
        {
            m_objects.TryGetValue(typeof(T), out object result);
            return (T)result;
        }

        Dictionary<Type, object> m_objects = new Dictionary<Type, object>(16);
    }
}

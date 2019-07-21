using KlaxShared;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace KlaxConfig.Console
{
    public class CCVar : IConsoleInvokable
    {
        public CCVar(SHashedName name, PropertyInfo property, object targetObject, object defaultValue, MethodInfo callback)
        {
            Name = name;
            m_property = property;
            TargetObject = targetObject;
            m_defaultValue = defaultValue;
            Type = property.PropertyType;
            m_callback = callback;

            Set(defaultValue);
        }

        public object Get()
        {
            return m_property.GetValue(TargetObject);
        }

        public T Get<T>()
        {
            return (T)Get();
        }

        public void Set(object newValue)
        {
            object oldValue = Get();

            //String is a special case as it is most likely set from the console
            if (newValue is string stringValue)
            {
                if (CConfigManager.Instance.ConsoleArgConverters[Type].ConvertFromString(stringValue, out object convertedValue))
                {
                    newValue = convertedValue;
                }
                else
                {
                    return;
                }
            }
            else
            {
                newValue = Convert.ChangeType(newValue, Type);
            }

            m_property.SetValue(TargetObject, newValue);
            m_callback?.Invoke(TargetObject, new object[] { oldValue, newValue });
        }

        public string GetSuggestionName()
        {
            return string.Format("{0} [{1}]={2}", Name.GetString(), Type.Name, Get().ToString());
        }


        public Type Type { get; }
        public SHashedName Name { get; private set; }
        public object TargetObject { get; }

        private readonly PropertyInfo m_property;
        private readonly MethodInfo m_callback;
        private readonly object m_defaultValue;
    }
}

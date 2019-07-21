using KlaxShared;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace KlaxConfig.Console
{
    public class CConsoleCommand : IConsoleInvokable
    {
        public CConsoleCommand(SHashedName name, MethodInfo method, object targetObject)
        {
            Name = name;
            m_targetMethod = method;
            TargetObject = targetObject;
        }

        public void Invoke(List<string> arguments)
        {
            if (m_targetMethod != null)
            {
                ParameterInfo[] requiredParams = m_targetMethod.GetParameters();
                List<object> convertedParams = new List<object>();
                Dictionary<Type, CConsoleArgumentConverter> converters = CConfigManager.Instance.ConsoleArgConverters;

                for (int i = 0; i < arguments.Count && i < requiredParams.Length; i++)
                {
                    ParameterInfo info = requiredParams[i];
                    string argument = arguments[i];

                    object convertedArgument = null;
                    if (!converters[info.ParameterType].ConvertFromString(argument, out convertedArgument))
                    {
                        LogUtility.Log("Tried to invoke command {0} with incompatible parameter for argument {1}. Passed in parameter must be {2}.", Name, info.Name, info.ParameterType.Name);
                        return;
                    }
                    convertedParams.Add(convertedArgument);
                }

                //Fill up parameters with default values
                while (convertedParams.Count < requiredParams.Length)
                {
                    convertedParams.Add(null);
                }

                m_targetMethod.Invoke(null, convertedParams.ToArray());
            }
        }

        public string GetSuggestionName()
        {
            string result = Name.GetString();

            foreach (ParameterInfo info in m_targetMethod.GetParameters())
            {
                result += string.Format(" [{0}]", info.ParameterType.Name);
            }

            return result;
        }

        public SHashedName Name { get; }
        public object TargetObject { get; }
        private readonly MethodInfo m_targetMethod;
    }
}

using KlaxConfig.Console;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using TCVarDictionary = System.Collections.Generic.Dictionary<KlaxShared.SHashedName, KlaxConfig.Console.CCVar>;
using TCommandDictionary = System.Collections.Generic.Dictionary<KlaxShared.SHashedName, KlaxConfig.Console.CConsoleCommand>;
using TConsoleArgConverters = System.Collections.Generic.Dictionary<System.Type, KlaxConfig.Console.CConsoleArgumentConverter>;
using KlaxShared.Attributes;
using KlaxShared;
using KlaxShared.Containers;
using System.Threading;

namespace KlaxConfig
{
    public enum EConfigDispatcherPriority
    {
        Update
    }

    public class CConfigManager : IDisposable, IDispatchable<EConfigDispatcherPriority>
    {
        public static CConfigManager Instance { get; private set; }

        public CConfigManager()
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("There can only be one Config Manager at the same time!");
            }

            m_argumentConverters = new TConsoleArgConverters();
            m_variables = new TCVarDictionary();
            m_commands = new TCommandDictionary();
            m_owningThread = Thread.CurrentThread;

            Instance = this;
        }

        public void Dispose()
        {
            Instance = null;

            AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoaded;
        }

        public void Init()
        {
            RegisterArgumentConverters();
            RegisterConsoleVariables();
            m_loader.Init();
        }

        public void Update(float deltaTime)
        {
            m_dispatcherQueue.Execute((int)EConfigDispatcherPriority.Update);
        }

        public void RegisterVariable(string name, Type type, object defaultValue, PropertyInfo property, object targetObject, MethodInfo callback = null)
        {
            void RegisterVariableInternal()
            {
                SHashedName hashedName = new SHashedName(name);
                if (!m_variables.ContainsKey(hashedName))
                {
                    CCVar cvar = new CCVar(hashedName, property, targetObject, defaultValue, callback);
                    m_variables.Add(hashedName, cvar);
                    return;
                }

                throw new ArgumentException("Passed in console variable already exists!");
            }

            if (IsInAuthoritativeThread())
            {
                RegisterVariableInternal();
            }
            else
            {
                Dispatch(EConfigDispatcherPriority.Update, RegisterVariableInternal);
            }
        }

        public void UnregisterVariable(string name)
        {
            m_variables.Remove(new SHashedName(name));
        }

        public void RegisterCommand(string name, MethodInfo method, object targetObject)
        {
            void RegisterCommandInternal()
            {
                if (IsConsoleCommandValid(method))
                {
                    SHashedName hashedName = new SHashedName(name);
                    if (!m_commands.ContainsKey(hashedName))
                    {
                        CConsoleCommand command = new CConsoleCommand(hashedName, method, targetObject);
                        m_commands.Add(hashedName, command);
                    }
                }
            }

            if (IsInAuthoritativeThread())
            {
                RegisterCommandInternal();
            }
            else
            {
                Dispatch(EConfigDispatcherPriority.Update, RegisterCommandInternal);
            }
        }

        public void UnregisterCommand(string name)
        {
            void UnregisterCommandInternal()
            {
                SHashedName hashedName = new SHashedName(name);
                m_commands.Remove(hashedName);
            }

            if (IsInAuthoritativeThread())
            {
                UnregisterCommandInternal();
            }
            else
            {
                Dispatch(EConfigDispatcherPriority.Update, UnregisterCommandInternal);
            }
        }

        public void RegisterInstance(object instance)
        {
            void RegisterInstanceInternal()
            {
                Type instanceType = instance.GetType();

                KlaxAssembly assembly = instanceType.Assembly.GetCustomAttribute<KlaxAssembly>();

                //Register properties
                foreach (var property in instanceType.GetProperties())
                {
                    CVarAttribute attribute = property.GetCustomAttribute<CVarAttribute>();
                    if (attribute != null)
                    {
                        string name = string.IsNullOrEmpty(attribute.NameOverride) ? (assembly.ConsoleVariablePrefix + property.Name) : attribute.NameOverride;
                        RegisterVariable(name, property.PropertyType, attribute.ValueOverride ?? property.GetValue(instance), property, instance, attribute.Callback != null ? instanceType.GetMethod(attribute.Callback, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) : null);
                    }
                }
            }

            if (IsInAuthoritativeThread())
            {
                RegisterInstanceInternal();
            }
            else
            {
                Dispatch(EConfigDispatcherPriority.Update, RegisterInstanceInternal);
            }
        }

        public void UnregisterInstance(object instance)
        {
            void UnregisterInstanceInternal()
            {
                List<SHashedName> variableNames = new List<SHashedName>(4);
                foreach (var entry in m_variables)
                {
                    if (entry.Value.TargetObject == instance)
                    {
                        variableNames.Add(entry.Key);
                    }
                }

                foreach (SHashedName name in variableNames)
                {
                    m_variables.Remove(name);
                }

                variableNames.Clear();
                foreach (var entry in m_commands)
                {
                    if (entry.Value.TargetObject == instance)
                    {
                        variableNames.Add(entry.Key);
                    }
                }

                foreach (SHashedName name in variableNames)
                {
                    m_commands.Remove(name);
                }
            }

            if (IsInAuthoritativeThread())
            {
                UnregisterInstanceInternal();
            }
            else
            {
                Dispatch(EConfigDispatcherPriority.Update, UnregisterInstanceInternal);
            }
        }

        public void InvokeConsoleCommand(string name, List<string> arguments)
        {
            void InvokeConsoleCommandInternal(List<string> args)
            {
                SHashedName hashedName = new SHashedName(name);
                if (m_commands.TryGetValue(hashedName, out CConsoleCommand command))
                {
                    command.Invoke(args);
                }
            }

            if (IsInAuthoritativeThread())
            {
                InvokeConsoleCommandInternal(arguments);
            }
            else
            {
                List<string> copiedArguments = new List<string>(arguments);
                Dispatch(EConfigDispatcherPriority.Update, () => InvokeConsoleCommandInternal(copiedArguments));
            }
        }

        public bool SetVariable(string name, object value)
        {
            SHashedName hashedName = new SHashedName(name);
            if (m_variables.TryGetValue(hashedName, out CCVar cvar))
            {
                cvar.Set(value);
                return true;
            }

            return false;
        }

        public object GetVariable(string name)
        {
            SHashedName hashedName = new SHashedName(name);
            return m_variables.TryGetValue(hashedName, out CCVar cvar) ? cvar.Get() : null;
        }

        public T GetVariable<T>(string name)
        {
            SHashedName hashedName = new SHashedName(name);
            return m_variables.TryGetValue(hashedName, out CCVar cvar) ? cvar.Get<T>() : default;
        }

        public void GetConsoleSuggestionsContainingString(string name, ref List<string> outSuggestions)
        {
            lock (m_lockObject)
            {
                outSuggestions.Clear();
                var commandNames = m_commands.Where(element => element.Key.GetString().Contains(name)).Select(element => element.Value.GetSuggestionName());
                var variableNames = m_variables.Where(element => element.Key.GetString().Contains(name)).Select(element => element.Value.GetSuggestionName());

                outSuggestions.AddRange(commandNames);
                outSuggestions.AddRange(variableNames);
                outSuggestions.Sort();
            }
        }

        public void GetConsoleSuggestions(string input, ref List<string> outSuggestions, ref List<string> outRawSuggestions)
        {
            lock (m_lockObject)
            {
                List<string> names = new List<string>();
                var commandNames = m_commands.Select(element => element.Value.GetSuggestionName());
                var variableNames = m_variables.Select(element => element.Value.GetSuggestionName());

                names.AddRange(commandNames);
                names.AddRange(variableNames);
                names.Sort();

                outSuggestions = names.Where(name => name.StartsWith(input)).ToList();

                outRawSuggestions.Clear();
                names.Clear();
                var rawCommandNames = m_commands.Select(element => element.Key.GetString());
                var rawVariableNames = m_variables.Select(element => element.Key.GetString());

                names.AddRange(rawCommandNames);
                names.AddRange(rawVariableNames);
                names.Sort();

                outRawSuggestions = names.Where(name => name.StartsWith(input)).ToList();
            }
        }

        private void RegisterArgumentConverters()
        {
            m_argumentConverters.Add(typeof(int), new CIntegerArgumentConverter());
            m_argumentConverters.Add(typeof(float), new CFloatArgumentConverter());
            m_argumentConverters.Add(typeof(string), new CStringArgumentConverter());
        }

        private void RegisterConsoleVariables()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                RegisterAssembly(assembly);
            }
			
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;
        }

        private void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            RegisterAssembly(args.LoadedAssembly);
            m_loader.ParseSystemConfiguration();
        }

        private void RegisterAssembly(Assembly assembly)
        {
            KlaxAssembly klaxAssembly = assembly.GetCustomAttribute<KlaxAssembly>();
            if (klaxAssembly == null)
                return;

            foreach (TypeInfo type in assembly.DefinedTypes)
            {
				if (type.Name == "CUpdateScheduler")
				{
					LogUtility.Log("D");
				}
                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    CVarAttribute attribute = property.GetCustomAttribute<CVarAttribute>();
                    if (attribute != null && IsConsoleVariableValid(property))
                    {
                        string name = string.IsNullOrEmpty(attribute.NameOverride) ? (klaxAssembly.ConsoleVariablePrefix + property.Name) : attribute.NameOverride;
                        object defaultValue = attribute.ValueOverride ?? property.GetValue(null);
                        MethodInfo callback = attribute.Callback != null ? type.GetMethod(attribute.Callback, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public) : null;

                        RegisterVariable(name, property.PropertyType, defaultValue, property, null, callback);
                    }
                }
				
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    ConsoleCommandAttribute attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();
                    if (attribute != null)
                    {
                        RegisterCommand(attribute.NameOverride ?? method.Name, method, null);
                    }
                }
            }
        }

        bool IsConsoleVariableValid(PropertyInfo property)
        {
            return m_argumentConverters.ContainsKey(property.PropertyType);
        }

        bool IsConsoleCommandValid(MethodInfo method)
        {
            if (method.ReturnType != typeof(void))
            {
                LogUtility.Log("Tried to register console command for function {0}/{1} with return type {3}! Only void is allowed!", method.DeclaringType.Name, method.Name, method.ReturnType.Name);
                return false;
            }

            foreach (ParameterInfo info in method.GetParameters())
            {
                if (!m_argumentConverters.ContainsKey(info.ParameterType))
                {
                    LogUtility.Log("Tried to register console command for function parameter {0}/{1}/{2} which has unsupported type {3}.", method.DeclaringType.Name, method.Name, info.Name, info.ParameterType.Name);
                    return false;
                }
            }

            return true;
        }

        public void Dispatch(EConfigDispatcherPriority priority, Action action)
        {
            m_dispatcherQueue.Add((int)priority, action);
        }

        public bool IsInAuthoritativeThread()
        {
            return Thread.CurrentThread == m_owningThread;
        }

        public TConsoleArgConverters ConsoleArgConverters { get { return m_argumentConverters; } }

        private SConfigLoader m_loader;
        private TCVarDictionary m_variables;
        private TCommandDictionary m_commands;
        private TConsoleArgConverters m_argumentConverters;

        private object m_lockObject = new object();
        private Thread m_owningThread;
        private DispatcherQueue m_dispatcherQueue = new DispatcherQueue((int)EConfigDispatcherPriority.Update + 1);
    }
}

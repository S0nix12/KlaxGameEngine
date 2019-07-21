using System;

namespace KlaxShared.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class KlaxAssembly : Attribute
    {
        public KlaxAssembly()
        {
        }

        public KlaxAssembly(string variablePrefix)
        {
            ConsoleVariablePrefix = variablePrefix;
        }

        public KlaxAssembly(bool bReflected)
        {
            Reflected = bReflected;
        }

        public KlaxAssembly(string variablePrefix, bool bReflected)
        {
            ConsoleVariablePrefix = variablePrefix;
            Reflected = bReflected;
        }

        public string ConsoleVariablePrefix { get; }
        public bool Reflected { get; }
    }
}

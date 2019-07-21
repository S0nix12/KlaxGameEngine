using System;

namespace KlaxShared.Attributes
{
    /// <summary>
    /// Mark a static property with this attribute to make it editable by config files and the console.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CVarAttribute : Attribute
    {
        public CVarAttribute()
        {}

        public CVarAttribute(string name)
        {
            NameOverride = name;
        }

        public CVarAttribute(object defaultValue)
        {
            ValueOverride = defaultValue;
        }

        public CVarAttribute(string name, object defaultValue)
        {
            NameOverride = name;
            ValueOverride = defaultValue;
        }

        public string NameOverride { get; private set; }
        public object ValueOverride { get; private set; }
        public string Callback { get; set; }
    }
}

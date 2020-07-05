using System;

namespace ScriptingLaunguage
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MetaCommandAttribute : Attribute
    {
        public string Name;
    }
}
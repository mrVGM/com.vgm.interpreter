using System;

namespace ScriptingLanguage.REPL
{
    [AttributeUsage(AttributeTargets.Method)]
    class MetaCommandAttribute : Attribute
    {
        public string Name;
    }
}

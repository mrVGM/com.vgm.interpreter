using System;

namespace ScriptingLanguage.VisualScripting
{
    public class NodeTargetAttribute : Attribute
    {
        public Type Type { get; }
        public NodeTargetAttribute(Type type)
        {
            Type = type;
        }
    }
}
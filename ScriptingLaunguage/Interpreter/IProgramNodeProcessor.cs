using System;
using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    public interface IProgramNodeProcessor
    {
        object ProcessNode(ProgramNode programNode, Scope scope, ref object value);
    }
}

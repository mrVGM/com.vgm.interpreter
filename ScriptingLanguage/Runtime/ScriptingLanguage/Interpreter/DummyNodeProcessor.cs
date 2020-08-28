using System;
using System.Collections.Generic;
using System.Text;
using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    public class DummyNodeProcessor : IProgramNodeProcessor
    {
        public static IProgramNodeProcessor DummyProcessor = new DummyNodeProcessor();
        public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
        {
            throw new NotImplementedException();
        }
    }
}

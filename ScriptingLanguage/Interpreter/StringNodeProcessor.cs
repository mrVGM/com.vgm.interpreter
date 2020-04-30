using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    public class StringNodeProcessor : IProgramNodeProcessor
    {
        public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
        {
            if (programNode.Token.Name == "String")
            {
                value = programNode.Token.Data as string;
                return true;
            }
            return false;
        }
    }
}

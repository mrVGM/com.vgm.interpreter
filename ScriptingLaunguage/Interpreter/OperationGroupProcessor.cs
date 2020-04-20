using System;
using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    class OperationGroupProcessor : IProgramNodeProcessor
    {
        public bool StopOnBreak = false;
        public bool StopOnReturn = false;
        IProgramNodeProcessor OperationProcessor = new OperationProcessor();
        public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
        {
            if (programNode.Token.Name != "OperationGroup") 
            {
                throw new NotSupportedException();
            }

            if (programNode.MatchChildren("Operation")) 
            {
                return OperationProcessor.ProcessNode(programNode.Children[0], scope, ref value);
            }

            if (programNode.MatchChildren("OperationGroup", "Operation"))
            {
                object tmp = null;
                var operation = ProcessNode(programNode.Children[0], scope, ref tmp);
                if (operation is OperationProcessor.BreakOperation)
                {
                    return operation;
                }
                if (operation is OperationProcessor.ReturnOperation) 
                {
                    value = tmp;
                    return operation;
                }
                return OperationProcessor.ProcessNode(programNode.Children[1], scope, ref value);
            }

            throw new NotImplementedException();
        }
    }
}

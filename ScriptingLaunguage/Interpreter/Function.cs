using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    class Function : IFunction
    {
        public ProgramNode Block;
        public Scope Scope { get; set; }
        public string[] ParameterNames { get; set; }
        IProgramNodeProcessor BlockProcessor = new BlockProcessor(new OperationGroupProcessor { StopOnReturn = true });
        public object Result { get; set; }

        public void Execute()
        {
            Result = null;
            object functionResult = null;
            var scope = new Scope { ParentScope = Scope };
            var res = BlockProcessor.ProcessNode(Block, scope, ref functionResult);
            if (res is OperationProcessor.ReturnOperation) 
            {
                Result = functionResult;
            }
        }
    }
}

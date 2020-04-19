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
            object tmp = null;
            var scope = new Scope { ParentScope = Scope };
            var res = BlockProcessor.ProcessNode(Block, scope, ref tmp);
            if (res is OperationProcessor.ReturnOperation) 
            {
                Result = res;
            }
        }
    }
}

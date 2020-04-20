using System.IO;
using ScriptingLaunguage.Interpreter;

namespace ScriptingLaunguage.BaseFunctions
{
    class RequireFunction : IFunction
    {
        Scope scope;
        public Scope Scope
        {
            get
            {
                if (scope == null) 
                {
                    scope = new Scope { ParentScope = Interpreter.Interpreter.GlobalScope };
                    scope.AddVariable("filename", null);
                    scope.AddVariable("exports", new GenericObject());
                }

                var localScope = new Scope { ParentScope = scope };
                return localScope;
            }
        }
        public string[] ParameterNames { get; private set; } = new string[] { "filename" };
        public object Result { get; set; }

        string workingDir;

        public RequireFunction(string dir) 
        {
            workingDir = dir;
        }

        public void Execute()
        {
            var fullPath = workingDir + Scope.GetVariable("filename");
            if (!File.Exists(fullPath)) 
            {
                throw new FileNotFoundException($"Cannot find script: {fullPath}!");
            }

            var interpreter = new Interpreter.Interpreter(Scope.GetVariable("filename") as string, Scope);
            interpreter.Run();
            Result = Scope.GetVariable("exports");
        }
    }
}

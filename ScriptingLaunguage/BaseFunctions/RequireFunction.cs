using System.IO;
using System.Runtime.InteropServices;
using ScriptingLaunguage.Interpreter;

namespace ScriptingLaunguage.BaseFunctions
{
    class RequireFunction : IFunction
    {
        public Scope ScopeTemplate
        {
            get
            {
                var scope = new Scope { ParentScope = Interpreter.Interpreter.GlobalScope };                
                scope.AddVariable("filename", null);
                scope.AddVariable("exports", new GenericObject());

                var localScope = new Scope { ParentScope = scope };
                return localScope;
            }
        }
        public string[] ParameterNames { get; private set; } = new string[] { "filename" };
        
        string workingDir;

        public RequireFunction(string dir) 
        {
            workingDir = dir;
        }

        public object Execute(Scope scope)
        {
            var fullPath = workingDir + scope.GetVariable("filename");
            if (!File.Exists(fullPath)) 
            {
                throw new FileNotFoundException($"Cannot find script: {fullPath}!");
            }

            var interpreter = new Interpreter.Interpreter(scope.GetVariable("filename") as string, scope);
            interpreter.Run();
            return scope.GetVariable("exports");
        }
    }
}

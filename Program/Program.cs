using System;
using ScriptingLaunguage.Interpreter;

namespace Program
{
    class Program
    {
        const string WorkingDir = "C:\\Users\\Vasil\\Desktop\\Scripts\\";
        const string MainScript = "main.txt"; 
        static void Main(string[] args)
        {
            var testClass = new TestClass();

            var globalScope = new Scope();
            globalScope.AddVariable("getDelegate", new GetEventType());
            globalScope.AddVariable("createDelegate", new CreateDelegateFunction());
            globalScope.AddVariable("subscribeToEvent", new SubscribeToEvent());
            globalScope.AddVariable("testClass", testClass);

            Interpreter.GlobalScope.ParentScope = globalScope;
            Interpreter.workingDir = WorkingDir;
            var scope = new Scope() { ParentScope = Interpreter.GlobalScope };
            var interpreter = new Interpreter(MainScript, scope);
            interpreter.Run();

            testClass.CallAsd();
        }
    }
}

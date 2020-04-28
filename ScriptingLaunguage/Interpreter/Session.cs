using ScriptingLaunguage.BaseFunctions;

namespace ScriptingLaunguage.Interpreter
{
    public class Session
    {
        public string WorkingDir { get; private set; }
        public Scope SessionScope { get; private set; }

        private Scope interpteterScope;

        public Scope GetClearScope()
        {
            var clearScope = new Scope {ParentScope = interpteterScope};
            return clearScope;
        }

        public Session(string workingDir)
        {
            WorkingDir = workingDir;
            SessionScope = new Scope();
            interpteterScope = Interpreter.GetStaticScope();
            interpteterScope.ParentScope = SessionScope;
            SessionScope.AddVariable("require", new RequireFunction(this, interpteterScope));
        }
    }
}

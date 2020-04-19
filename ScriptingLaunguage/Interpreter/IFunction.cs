namespace ScriptingLaunguage.Interpreter
{
    public interface IFunction
    {
        Scope Scope { get; }
        string[] ParameterNames { get; }
        object Result { get; }
        void Execute();
    }
}

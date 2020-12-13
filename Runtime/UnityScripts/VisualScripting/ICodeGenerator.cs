namespace ScriptingLanguage.VisualScripting
{
    public interface ICodeGenerator
    {
        string GenerateCode(Knob knob);
    }
}

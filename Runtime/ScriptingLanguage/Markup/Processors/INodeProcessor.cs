namespace ScriptingLanguage.Markup.Processors
{
    public interface INodeProcessor
    {
        public void ProcessNode(UIElement node, object context);
    }
}

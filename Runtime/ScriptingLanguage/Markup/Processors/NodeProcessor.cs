namespace ScriptingLanguage.Markup.Processors
{
    public static class NodeProcessor
    {
        public static void ProcessNode(INodeProcessor processor, UIElement element, object context)
        {
            processor.ProcessNode(element, context);
        }
    }
}

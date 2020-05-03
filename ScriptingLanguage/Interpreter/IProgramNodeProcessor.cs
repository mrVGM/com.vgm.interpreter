using System;
using ScriptingLaunguage.Parser;
using ScriptingLaunguage.Tokenizer;
using static ScriptingLaunguage.Utils;

namespace ScriptingLaunguage.Interpreter
{
    public class BasicLanguageException : LanguageException
    {
        public BasicLanguageException(string message, ProgramNode exceptionNode) : base(message, exceptionNode.GetScriptSource(), exceptionNode.GetCodeIndex())
        {
        }

        public override string GetErrorMessage(bool printLineNumbers)
        {
            return GetCodeSample(CodeIndex, ScriptId.Script, true);
        }
    }
    public class NodeProcessor
    {
        private static NodeProcessor Instance = new NodeProcessor();
        
        public static object ExecuteProgramNodeProcessor(IProgramNodeProcessor processor, ProgramNode programNode, Scope scope, ref object value)
        {
            try
            {
                return processor.ProcessNode(programNode, scope, ref value);
            }
            catch (Exception e) 
            {
                LanguageException languageException = e as LanguageException;
                if (languageException == null) {
                    languageException = new BasicLanguageException(e.Message, programNode);
                }
                throw languageException;
            }
        }
        private NodeProcessor() { }
    }

    public interface IProgramNodeProcessor
    {
        object ProcessNode(ProgramNode programNode, Scope scope, ref object value);
    }
}

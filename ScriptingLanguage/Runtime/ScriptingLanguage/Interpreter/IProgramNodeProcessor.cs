using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using ScriptingLaunguage.Parser;
using ScriptingLaunguage.Tokenizer;
using static ScriptingLaunguage.Utils;

namespace ScriptingLaunguage.Interpreter
{
    public class BasicLanguageException : LanguageException
    {
        private string callStack = "";
        public BasicLanguageException(string message, ProgramNode exceptionNode, Stack<ProgramNode> programStack) : base(message, exceptionNode.GetScriptSource(), exceptionNode.GetCodeIndex())
        {
            int lastLine = -1;
            ScriptId lastScript = null;
            foreach (var node in programStack) 
            {
                var scriptSource = node.GetScriptSource();
                if (string.IsNullOrEmpty(scriptSource.Filename))
                {
                    break;
                }

                string script = scriptSource.Script;
                int line = Utils.GetLineNumber(CodeIndex, script);
                if (line == lastLine && scriptSource == lastScript) 
                {
                    continue;
                }

                int index = Utils.GetLineOffset(CodeIndex, script);
                lastLine = line;
                lastScript = scriptSource;

                callStack += $"{scriptSource.Filename} {line + 1}:{index + 1}{Environment.NewLine}";
            }
        }

        public override string GetErrorMessage(bool printLineNumbers)
        {
            return $"{GetCodeSample(CodeIndex, ScriptId.Script, true)}{Environment.NewLine}{callStack}";
        }
    }
    public class NodeProcessor
    {
        private static Stack<ProgramNode> programStack = new Stack<ProgramNode>();
        
        public static object ExecuteProgramNodeProcessor(IProgramNodeProcessor processor, ProgramNode programNode, Scope scope, ref object value)
        {
            try
            {
                programStack.Push(programNode);
                var res = processor.ProcessNode(programNode, scope, ref value);
                programStack.Pop();
                return res;
            }
            catch (Exception e) 
            {
                LanguageException languageException = e as LanguageException;
                if (languageException == null) {
                    languageException = new BasicLanguageException($"{e.Message} {e.InnerException?.Message}", programNode, programStack);
                }
                programStack.Clear();
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

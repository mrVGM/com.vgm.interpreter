using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    class FunctionCallProcessor : IProgramNodeProcessor
    {
        public class FunctionCallSettings 
        {
            public string FunctionName;
            public string TemplateParamName;
            public IEnumerable<object> Arguments = new List<object>();
        }

        public class TemplateProcessor : IProgramNodeProcessor
        {
            public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
            {
                if (programNode.Token.Name != "Template") 
                {
                    throw new NotSupportedException();
                }

                if (programNode.MatchChildren("|", "String", "|")) 
                {
                    value = programNode.Children[1].Token.Data as string;
                    return null;
                }
                throw new NotImplementedException();
            }
        }

        public class ArgumentsProcessor : IProgramNodeProcessor
        {
            ExpressionNodeProcessor expressionProcessor = new ExpressionNodeProcessor();
            public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
            {
                if (programNode.Token.Name != "Arguments") 
                {
                    throw new NotSupportedException();
                }

                IEnumerable<object> extractArgs(ProgramNode node)
                {
                    if (node.MatchChildren("Expression")) 
                    {
                        object tmp = null;
                        NodeProcessor.ExecuteProgramNodeProcessor(expressionProcessor, node.Children[0], scope, ref tmp);
                        yield return tmp;
                        yield break;
                    }

                    if (node.MatchChildren("Arguments", ",", "Expression")) 
                    {
                        var arguments = extractArgs(node.Children[0]);
                        foreach (var arg in arguments) 
                        {
                            yield return arg;
                        }

                        object tmp = null;
                        NodeProcessor.ExecuteProgramNodeProcessor(expressionProcessor, node.Children[2], scope, ref tmp);
                        yield return tmp;
                    }
                }

                value = extractArgs(programNode);
                return null;
            }
        }

        IProgramNodeProcessor Template = new TemplateProcessor();
        IProgramNodeProcessor Arguments = new ArgumentsProcessor();

        public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
        {
            if (programNode.Token.Name != "FunctionCall") 
            {
                throw new NotSupportedException();
            }

            if (programNode.MatchChildren("Name", "(", ")")) 
            {
                var name = programNode.Children[0].Token.Data as string;
                value = new FunctionCallSettings { FunctionName = name };
                return null;
            }

            if (programNode.MatchChildren("Name", "Template", "(", ")")) 
            {
                var name = programNode.Children[0].Token.Data as string;

                object template = null;
                NodeProcessor.ExecuteProgramNodeProcessor(Template, programNode.Children[1], scope, ref template);
                value = new FunctionCallSettings { FunctionName = name, TemplateParamName = template as string };
                return null;
            }

            if (programNode.MatchChildren("Name", "(", "Arguments", ")"))
            {
                var name = programNode.Children[0].Token.Data as string;

                object args = null;
                NodeProcessor.ExecuteProgramNodeProcessor(Arguments, programNode.Children[2], scope, ref args);
                var arguments = args as IEnumerable<object>;
                value = new FunctionCallSettings { FunctionName = name, Arguments = arguments };
                return null;
            }

            if (programNode.MatchChildren("Name", "Template", "(", "Arguments", ")"))
            {
                var name = programNode.Children[0].Token.Data as string;

                object template = null;
                NodeProcessor.ExecuteProgramNodeProcessor(Template, programNode.Children[1], scope, ref template);

                object args = null;
                NodeProcessor.ExecuteProgramNodeProcessor(Arguments, programNode.Children[3], scope, ref args);
                var arguments = args as IEnumerable<object>;
                value = new FunctionCallSettings { FunctionName = name, Arguments = arguments, TemplateParamName = template as string };
                return null;
            }

            throw new NotImplementedException();
        }
    }
}

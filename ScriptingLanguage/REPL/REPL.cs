using ScriptingLaunguage.Interpreter;
using ScriptingLaunguage.Parser;
using ScriptingLaunguage.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ScriptingLaunguage.Parser.Parser;
using static ScriptingLaunguage.Utils;

namespace ScriptingLanguage.REPL
{
    public class REPL
    {
        public class PrintBuffer : IFunction
        {
            const string paramName = "arg";
            public Scope ScopeTemplate => new Scope();

            public string[] ParameterNames => new[] { paramName };

            public List<string> Buffer { get; } = new List<string>();

            public void Clear() 
            {
                Buffer.Clear();
            }

            public object Execute(Scope scope)
            {
                Buffer.Add($"{scope.GetVariable(paramName)}");
                return null;
            }
        }

        private Session session;
        private Interpreter interpreter;
        private string buffer = "";
        private readonly PrintBuffer printBuffer = new PrintBuffer();

        public REPL(ParserTable pt)
        {
            var parser = new Parser { ParserTable = pt };
            session = new Session(null, parser, new Session.SessionFunc { Name = "print", Function = printBuffer });
            interpreter = new Interpreter(session);
        }

        MethodInfo[] metaCommandMethods;
        IEnumerable<MethodInfo> MetaCommands
        {
            get 
            {
                IEnumerable<MethodInfo> findMetaCommands()
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

                    foreach (var assembly in assemblies)
                    {
                        var types = assembly.GetTypes();
                        foreach (var type in types) 
                        {
                            var methods = type.GetMethods(bindingFlags);
                            foreach (var method in methods) 
                            {
                                var attribute = method.GetCustomAttribute<MetaCommandAttribute>();
                                if (attribute != null) 
                                {
                                    yield return method;
                                }
                            }
                        }
                    }
                }

                if (metaCommandMethods == null) 
                {
                    metaCommandMethods = findMetaCommands().ToArray();
                }

                return metaCommandMethods;
            }
        }

        bool HandleMetaCommand(string command, out IEnumerable<string> output)
        {
            var trimmed = command.Trim();
            IEnumerable<string> createEnumerable(params string[] args) 
            {
                foreach (var arg in args)
                    yield return arg;
            }

            if (!trimmed.StartsWith(":") || trimmed.Length == 1 || trimmed[1] == ' ')
            {
                output = createEnumerable();
                return false;
            }



            trimmed = trimmed.Substring(1);
            var words = trimmed.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));
            var commandName = words.First();
            var commandArgs = words.Skip(1);

            var method = MetaCommands.FirstOrDefault(x => x.GetCustomAttribute<MetaCommandAttribute>().Name == command);
            if (method == null) 
            {
                output = createEnumerable($"Command {command} not found!");
                return true;
            }

            output = method.Invoke(null, commandArgs.ToArray()) as IEnumerable<string>;
            return true;
        }

        IEnumerable<string> HandleCommand(string command) 
        {
            IEnumerable<string> output;
            bool metaCommand = HandleMetaCommand(command, out output);
            if (metaCommand) 
            {
                foreach (var str in output) 
                {
                    yield return str;
                }
                yield break;
            }

            buffer += command;
            var scriptId = new ScriptId { Script = buffer };
            printBuffer.Clear();

            LanguageException exception = null;
            object res = null;
            try
            {
                res = interpreter.RunScript(buffer, session.GetWorkingScope(), scriptId);
            }
            catch (LanguageException e) 
            {
                exception = e;
            }

            foreach (var str in printBuffer.Buffer) 
            {
                yield return str;
            }
          
            if (exception is ExpectsSymbolException
                && exception.ScriptId == scriptId
                && exception.CodeIndex == buffer.Length) 
            {
                yield return "...";
                yield break;
            }

            buffer = "";
            if (exception != null)
            {
                yield return exception.Message;
                throw exception;
            }
            yield return $"{(res == null ? "null" : res)}";
        }
    }
}

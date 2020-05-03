using System;
using ScriptingLaunguage.Interpreter;
using ScriptingLaunguage.Tokenizer;
using static ScriptingLaunguage.Parser.Parser;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            var session = new Session("Scripts\\");
            var interpreter = new Interpreter(session);
            string buffer = "";
            while (true) 
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line.StartsWith("#") && line.Contains("exit")) 
                {
                    break;
                }
                if (string.IsNullOrWhiteSpace(line)) 
                {
                    buffer = "";
                    continue;
                }

                buffer += line + Environment.NewLine;
                object res = null;
                var scriptId = new ScriptId { Script = buffer };
                try
                {
                    res = interpreter.RunScript(buffer, session.GetWorkingScope(), scriptId);
                }
                catch (ExpectsSymbolException expectSymbolException)
                {
                    var exceptionSource = expectSymbolException.ScriptId;
                    if (scriptId == exceptionSource && expectSymbolException.CodeIndex == buffer.Length)
                    {
                        Console.WriteLine(" ...");
                        continue;
                    }
                    bool printLineNumbers = exceptionSource != scriptId;
                    Console.WriteLine(expectSymbolException.GetErrorMessage(printLineNumbers));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }

                Console.WriteLine(res == null ? "null" : res);
                buffer = "";
            }
        }
    }
}

using System;
using ScriptingLaunguage.Interpreter;
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
                try
                {
                    res = interpreter.RunScript(buffer, session.GetWorkingScope(), new ProgramSource { Interpreter = interpreter, SourceCode = buffer });
                }
                catch (ExpectsSymbolException expectSymbolException)
                {
                    var exceptionSource = expectSymbolException.ExceptionSource;
                    if (exceptionSource.Interpreter == interpreter && expectSymbolException.CodeIndex == buffer.Length)
                    {
                        Console.WriteLine(" ...");
                        continue;
                    }

                    Console.WriteLine(expectSymbolException.GetErrorMessage());
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

using System;
using System.IO;
using ScriptingLaunguage.BaseFunctions;
using ScriptingLaunguage.Parser;
using ScriptingLaunguage.Tokenizer;

namespace ScriptingLaunguage.Interpreter
{
    public class Interpreter
    {
        const string grammarDefinition = "grammar.txt";
        const string parserTableData = "parserTable.dat";

        public static string workingDir;
        string entryPoint;

        private static Scope globalScope;
        public static Scope GlobalScope
        {
            get 
            {
                if (globalScope == null) 
                {
                    globalScope = new Scope();
                    globalScope.AddVariable("require", new RequireFunction(workingDir));
                    globalScope.AddVariable("print", new PrintFunction());
                    globalScope.AddVariable("len", new LengthFunction());
                    globalScope.AddVariable("new", new CreateObjectFunction());
                    globalScope.AddVariable("getDelegate", new GetEventType());
                    globalScope.AddVariable("createDelegate", new CreateDelegateFunction());
                    globalScope.AddVariable("subscribeToEvent", new SubscribeToEventFunction());
                    globalScope.AddVariable("unsubscribeFromEvent", new UnsubscribeFromEventFunction());
                }
                return globalScope;
            }
        }

        public Scope Scope = GlobalScope;

        public IProgramNodeProcessor OperationGroupProcessor = new OperationGroupProcessor();

        ITokenizer tokenizer = Tokenizer.CombinedTokenizer.DefaultTokenizer;
        Parser.Parser parser;
        public Interpreter(string mainScript, Scope scope = null)
        {
            entryPoint = mainScript;
            if (scope != null) 
            {
                Scope = scope;
            }

            if (!File.Exists(workingDir + parserTableData)) 
            {
                var grammar = Grammar.ReadGrammarFromFile(workingDir + grammarDefinition);
                var parserTable = new ParserTable(grammar);
                bool validTable =  parserTable.Validate();
                parserTable.Serialize(workingDir + parserTableData);
                parser = new Parser.Parser { ParserTable = parserTable };
            }
            else 
            {
                var parserTable = ParserTable.Deserialize(workingDir + parserTableData);
                parser = new Parser.Parser { ParserTable = parserTable };
            }
        }

        public void Run() 
        {
            var entryScript = File.ReadAllText(workingDir + entryPoint);
            var tokenized = tokenizer.Tokenize(Utils.TokenizeText(entryScript, new Token { Name = "Terminal" }));
            var programTree = parser.ParseProgram(tokenized);

            EvaluateProgramNode(programTree);
        }

        public void EvaluateProgramNode(ProgramNode programNode) 
        {
            var curNode = programNode;
            if (curNode.Token.Name != "Root") 
            {
                throw new Exception("Invalid Program");
            }
            curNode = curNode.Children[0];
            object tmp = null;
            OperationGroupProcessor.ProcessNode(curNode, Scope, ref tmp);
        }
    }
}

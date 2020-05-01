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

        public static Scope GetStaticScope()
        {
            return StaticScope;
        }

        private static Scope StaticScope
        {
            get
            {
                var staticScope = new Scope();
                staticScope.AddVariable("print", new PrintFunction());
                staticScope.AddVariable("len", new LengthFunction());
                staticScope.AddVariable("new", new CreateObjectFunction());
                staticScope.AddVariable("getDelegate", new GetEventType());
                staticScope.AddVariable("createDelegate", new CreateDelegateFunction());
                staticScope.AddVariable("subscribeToEvent", new SubscribeToEventFunction());
                staticScope.AddVariable("unsubscribeFromEvent", new UnsubscribeFromEventFunction());
                staticScope.AddVariable("getTypeMembers", new GetTypeMembersFunction());
                staticScope.AddVariable("createCoroutine", new CreateCoroutineFunction());
                return staticScope;
            }
        }

        public IProgramNodeProcessor OperationGroupProcessor = new OperationGroupProcessor();

        ITokenizer tokenizer = Tokenizer.CombinedTokenizer.DefaultTokenizer;
        Parser.Parser parser;
        public Interpreter(Session session)
        {
            if (!File.Exists(session.WorkingDir + parserTableData)) 
            {
                var grammar = Grammar.ReadGrammarFromFile(session.WorkingDir + grammarDefinition);
                var parserTable = new ParserTable(grammar);
                bool validTable =  parserTable.Validate();
                parserTable.Serialize(session.WorkingDir + parserTableData);
                parser = new Parser.Parser { ParserTable = parserTable };
            }
            else 
            {
                var parserTable = ParserTable.Deserialize(session.WorkingDir + parserTableData);
                parser = new Parser.Parser { ParserTable = parserTable };
            }
        }

        public object RunScriptFile(string scriptToRunFullPath, Scope scope = null)
        {
            var entryScript = File.ReadAllText(scriptToRunFullPath);
            return RunScript(entryScript, scope);
        }
        
        public object RunScript(string script, Scope scope)
        {
            var tokenized = tokenizer.Tokenize(Utils.TokenizeText(script, new SimpleToken { Name = "Terminal" }));
            var programTree = parser.ParseProgram(tokenized);
            
            return EvaluateProgramNode(programTree, scope);
        }

        public object EvaluateProgramNode(ProgramNode programNode, Scope scope) 
        {
            var curNode = programNode;
            if (curNode.Token.Name != "Root") 
            {
                throw new Exception("Invalid Program");
            }
            curNode = curNode.Children[0];
            object res = null;
            OperationGroupProcessor.ProcessNode(curNode, scope, ref res);
            return res;
        }
    }
}

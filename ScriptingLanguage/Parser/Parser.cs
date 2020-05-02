using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using ScriptingLaunguage.Tokenizer;

namespace ScriptingLaunguage.Parser
{
    public class Parser
    {
        public class ProgramSource
        {
            public string Filename;
            public string SourceCode;
            public Interpreter.Interpreter Interpreter;
        }

        public abstract class ParseException : Exception
        {
            public int CodeIndex;
            public ProgramSource ExceptionSource;
            public ParseException(ProgramSource exceptionSource, int codeIndex)
            {
                ExceptionSource = exceptionSource;
                CodeIndex = codeIndex;
            }
            public string GetCodeSample(int index, string source)
            {
                int errorLineNumber = Utils.GetLineNumber(index, source);
                string errorLine = Utils.GetLine(errorLineNumber, source);
                int lineOffset = Utils.GetLineOffset(index, source);
                string pointerLine = Utils.PointSymbol(lineOffset, errorLine);
                string previousLine = "";
                if (errorLineNumber > 0) 
                {
                    previousLine = Utils.GetLine(errorLineNumber - 1, source);
                }
                string nextLine = Utils.GetLine(errorLineNumber + 1, source);

                string newLine = Environment.NewLine;
                if (!string.IsNullOrEmpty(previousLine)) 
                {
                    previousLine = $"{previousLine}{newLine}";
                }
                pointerLine = $"{pointerLine}{newLine}";
                nextLine = $"{nextLine}{newLine}";

                return $"{previousLine}{errorLine}{pointerLine}{nextLine}";
            }

            public abstract string GetErrorMessage();
        }

        public class ExpectsSymbolException : ParseException 
        {
            public IEnumerable<string> ExpectedSymbols;
            public ExpectsSymbolException(ProgramSource exceptionSource, int codeIndex, IEnumerable<string> expectedSymbols) : base(exceptionSource, codeIndex)
            {
                ExpectedSymbols = expectedSymbols;
            }

            public override string GetErrorMessage()
            {
                string expecting = "";
                foreach (var symbol in ExpectedSymbols) 
                {
                    expecting += $", {symbol}";
                }
                expecting = expecting.Substring(2);
                return $"Expection one of: {expecting}{Environment.NewLine}{ExceptionSource.Filename}{Environment.NewLine}{GetCodeSample(CodeIndex, ExceptionSource.SourceCode)}";
            }
        }

        public class CantProceedParsingException : ParseException 
        {
            public CantProceedParsingException(ProgramSource exceptionSource, int codeIndex) : base(exceptionSource, codeIndex) { }

            public override string GetErrorMessage()
            {
                return $"Syntax error{Environment.NewLine}{ExceptionSource.Filename}{Environment.NewLine}{GetCodeSample(CodeIndex, ExceptionSource.SourceCode)}";
            }
        }

        public ParserTable ParserTable;
        public ProgramNode ParseProgram(IEnumerable<IToken> program, ProgramSource programSource)
        {
            IEnumerator<IToken> script = program.GetEnumerator();
            script.MoveNext();

            Stack<int> stateStack = new Stack<int>();
            stateStack.Push(ParserTable.InitialState);

            Stack<ProgramNode> treeStack = new Stack<ProgramNode>();

            bool endOfProgram = false;
            IToken lastRead = null;

            while (stateStack.Peek() != ParserTable.FinalState) 
            {
                if (endOfProgram)
                {
                    var nextSymbols = ParserTable.ParserActions.Where(x => x.CurrentState == stateStack.Peek()).Select(x => x.NextSymbol);
                    int index = (lastRead as IIndexed).Index;
                    if (!nextSymbols.Any())
                    {
                        throw new CantProceedParsingException(programSource, index);
                    }
                    else 
                    {
                        throw new ExpectsSymbolException(programSource, index, nextSymbols);
                    }
                }
                var curToken = script.Current;
                lastRead = curToken;
                var action = ParserTable.ParserActions.FirstOrDefault(x => x.CurrentState == stateStack.Peek() && x.NextSymbol == curToken.Name);
                if (action == null) 
                {
                    int index = (curToken as IIndexed).Index;
                    var nextSymbols = ParserTable.ParserActions.Where(x => x.CurrentState == stateStack.Peek()).Select(x => x.NextSymbol);
                    if (!nextSymbols.Any())
                    {
                        throw new CantProceedParsingException(programSource, index);
                    }
                    else
                    {
                        throw new ExpectsSymbolException(programSource, index, nextSymbols);
                    }
                }

                if (action.ActionType == ActionType.Shift) 
                {
                    endOfProgram = !script.MoveNext();
                    stateStack.Push(action.NextState);
                    treeStack.Push(new ProgramNode { Token = curToken, RuleId = -1 });
                    continue;
                }

                var tmpStateStack = new Stack<int>();
                var tmpTreeStack = new Stack<ProgramNode>();
                for (int i = 0; i < action.ReduceSymbols; ++i) 
                {
                    tmpStateStack.Push(stateStack.Pop());
                    tmpTreeStack.Push(treeStack.Pop());
                }
                var reduceSymbol = action.ReduceSymbol;
                var reduceNode = new ProgramNode { RuleId = action.RuleId, Token = new SimpleToken { Name = reduceSymbol } };
                while (tmpTreeStack.Any()) 
                {
                    reduceNode.Children.Add(tmpTreeStack.Pop());
                }
                var shiftAfterReduceAction = ParserTable.ParserActions.FirstOrDefault(x => x.CurrentState == stateStack.Peek() && x.NextSymbol == reduceSymbol);
                if (shiftAfterReduceAction == null) 
                {
                    var nextSymbols = ParserTable.ParserActions.Where(x => x.CurrentState == stateStack.Peek()).Select(x => x.NextSymbol);
                    if (!nextSymbols.Any())
                    {
                        throw new CantProceedParsingException(programSource, reduceNode.GetCodeIndex());
                    }
                    else
                    {
                        throw new ExpectsSymbolException(programSource, reduceNode.GetCodeIndex(), nextSymbols);
                    }
                }
                treeStack.Push(reduceNode);
                stateStack.Push(shiftAfterReduceAction.NextState);
            }

            return treeStack.Last();
        }
    }
}

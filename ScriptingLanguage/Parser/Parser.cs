using System;
using System.Collections.Generic;
using System.Linq;
using ScriptingLaunguage.Tokenizer;

namespace ScriptingLaunguage.Parser
{
    public class Parser
    {
        public abstract class ParseException : Exception
        {
            public int CodeIndex;
            public ScriptId ScriptId;
            public ParseException(ScriptId scriptId, int codeIndex)
            {
                ScriptId = scriptId;
                CodeIndex = codeIndex;
            }
            public string GetCodeSample(int index, string source)
            {
                string lineNumber(int number, int stringLength) 
                {
                    string res = (number + 1).ToString();
                    while (res.Length < stringLength) 
                    {
                        res = $" {res}";
                    }
                    return $"{res}| ";
                }

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

                int lineNumberLength = (errorLineNumber + 2).ToString().Length;

                if (!string.IsNullOrEmpty(previousLine))
                {
                    previousLine = $"{lineNumber(errorLineNumber - 1, lineNumberLength)}{previousLine}";
                }
                errorLine = $"{lineNumber(errorLineNumber, lineNumberLength)}{errorLine}{newLine}";
                if (!string.IsNullOrEmpty(nextLine))
                {
                    nextLine = $"{lineNumber(errorLineNumber + 1, lineNumberLength)}{nextLine}";
                }

                return $"{previousLine}{errorLine}{pointerLine}{nextLine}";
            }

            public abstract string GetErrorMessage();
        }

        public class ExpectsSymbolException : ParseException 
        {
            public IEnumerable<string> ExpectedSymbols;
            public ExpectsSymbolException(ScriptId scriptId, int codeIndex, IEnumerable<string> expectedSymbols) : base(scriptId, codeIndex)
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
                return $"Expecting one of: {expecting}{Environment.NewLine}{ScriptId.Filename}{Environment.NewLine}{GetCodeSample(CodeIndex, ScriptId.Script)}";
            }
        }

        public class CantProceedParsingException : ParseException 
        {
            public CantProceedParsingException(ScriptId scriptId, int codeIndex) : base(scriptId, codeIndex) { }

            public override string GetErrorMessage()
            {
                return $"Syntax error{Environment.NewLine}{ScriptId.Filename}{Environment.NewLine}{GetCodeSample(CodeIndex, ScriptId.Script)}";
            }
        }

        public ParserTable ParserTable;
        public ProgramNode ParseProgram(IEnumerable<IToken> program)
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
                    var scriptSource = (lastRead as IScriptSourceHolder).ScriptSource;
                    if (!nextSymbols.Any())
                    {
                        throw new CantProceedParsingException(scriptSource, index);
                    }
                    else 
                    {
                        throw new ExpectsSymbolException(scriptSource, index, nextSymbols);
                    }
                }
                var curToken = script.Current;
                lastRead = curToken;
                var action = ParserTable.ParserActions.FirstOrDefault(x => x.CurrentState == stateStack.Peek() && x.NextSymbol == curToken.Name);
                if (action == null) 
                {
                    int index = (curToken as IIndexed).Index;
                    var scriptId = (curToken as IScriptSourceHolder).ScriptSource;
                    var nextSymbols = ParserTable.ParserActions.Where(x => x.CurrentState == stateStack.Peek()).Select(x => x.NextSymbol);
                    if (!nextSymbols.Any())
                    {
                        throw new CantProceedParsingException(scriptId, index);
                    }
                    else
                    {
                        throw new ExpectsSymbolException(scriptId, index, nextSymbols);
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
                        throw new CantProceedParsingException(reduceNode.GetScriptSource(), reduceNode.GetCodeIndex());
                    }
                    else
                    {
                        throw new ExpectsSymbolException(reduceNode.GetScriptSource(), reduceNode.GetCodeIndex(), nextSymbols);
                    }
                }
                treeStack.Push(reduceNode);
                stateStack.Push(shiftAfterReduceAction.NextState);
            }

            return treeStack.Last();
        }
    }
}

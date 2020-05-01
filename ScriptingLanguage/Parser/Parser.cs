using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ScriptingLaunguage.Tokenizer;

namespace ScriptingLaunguage.Parser
{
    public class Parser
    {
        public abstract class ParseException : Exception
        {
            public ParseException(string message) : base(message)
            {
            }
        }

        public class ExpectsSymbolException : ParseException 
        {
            public ExpectsSymbolException() : base("Expects Symbol") { }
        }

        public class CantProceedParsingException : ParseException 
        {
            public CantProceedParsingException() : base("Syntax error") { }
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
            while (stateStack.Peek() != ParserTable.FinalState) 
            {
                if (endOfProgram)
                {
                    var nextSymbols = ParserTable.ParserActions.Where(x => x.CurrentState == stateStack.Peek()).Select(x => x.NextSymbol);
                    if (!nextSymbols.Any())
                    {
                        throw new CantProceedParsingException();
                    }
                    else 
                    {
                        throw new ExpectsSymbolException();
                    }
                }
                var curToken = script.Current;
                var action = ParserTable.ParserActions.FirstOrDefault(x => x.CurrentState == stateStack.Peek() && x.NextSymbol == curToken.Name);
                if (action == null) 
                {
                    var nextSymbols = ParserTable.ParserActions.Where(x => x.CurrentState == stateStack.Peek()).Select(x => x.NextSymbol);
                    if (!nextSymbols.Any())
                    {
                        throw new CantProceedParsingException();
                    }
                    else
                    {
                        throw new ExpectsSymbolException();
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
                        throw new CantProceedParsingException();
                    }
                    else
                    {
                        throw new ExpectsSymbolException();
                    }
                }
                treeStack.Push(reduceNode);
                stateStack.Push(shiftAfterReduceAction.NextState);
            }

            return treeStack.Last();
        }
    }
}

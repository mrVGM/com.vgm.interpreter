using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptingLaunguage.Tokenizer
{
    public class NewLineTokenizer : ITokenizer
    {
        IEnumerable<Token> TwoSymbolNewLine(IEnumerable<Token> script) 
        {
            bool oneSymbolRead = false;
            string firstSymbol = Environment.NewLine[0].ToString();
            string secondSymbol = Environment.NewLine[1].ToString();
            foreach (var symbol in script)
            {
                if (!oneSymbolRead && symbol.Name == firstSymbol)
                {
                    oneSymbolRead = true;
                    continue;
                }
                if (oneSymbolRead) 
                {
                    if (symbol.Name == secondSymbol) 
                    {
                        yield return new Token { Name = Environment.NewLine };
                    }
                    else 
                    {
                        yield return new Token { Name = firstSymbol };
                    }
                    oneSymbolRead = false;
                    continue;
                }

                yield return symbol;
            }
        }
        public IEnumerable<Token> Tokenize(IEnumerable<Token> script)
        {
            if (Environment.NewLine.Length == 2) 
            {
                return TwoSymbolNewLine(script);
            }

            return script;
        }
    }
}

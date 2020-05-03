using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptingLaunguage.Tokenizer
{
    public class NewLineTokenizer : ITokenizer
    {
        IEnumerable<IndexedToken> TwoSymbolNewLine(IEnumerable<IndexedToken> script) 
        {
            IndexedToken oneSymbolRead = null;
            string firstSymbol = Environment.NewLine[0].ToString();
            string secondSymbol = Environment.NewLine[1].ToString();
            foreach (var symbol in script)
            {
                if (oneSymbolRead == null && symbol.Name == firstSymbol)
                {
                    oneSymbolRead = symbol;
                    continue;
                }
                if (oneSymbolRead != null) 
                {
                    if (symbol.Name == secondSymbol) 
                    {
                        yield return new IndexedToken(oneSymbolRead.Index, symbol.ScriptSource) { Name = Environment.NewLine };
                    }
                    else 
                    {
                        yield return oneSymbolRead;
                        yield return symbol;
                    }
                    oneSymbolRead = null;
                    continue;
                }

                yield return symbol;
            }
        }
        public IEnumerable<IndexedToken> Tokenize(IEnumerable<IndexedToken> script)
        {
            if (Environment.NewLine.Length == 2) 
            {
                return TwoSymbolNewLine(script);
            }

            return script;
        }
    }
}

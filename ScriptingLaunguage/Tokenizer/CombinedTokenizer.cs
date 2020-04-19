using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptingLaunguage.Tokenizer
{
    public class CombinedTokenizer : ITokenizer
    {
        static CombinedTokenizer defaultTokenizer;
        public static CombinedTokenizer DefaultTokenizer
        {
            get 
            {
                if (defaultTokenizer == null) 
                {
                    defaultTokenizer = new CombinedTokenizer(
                        new StringTokenizer(),
                        new NewLineTokenizer(),
                        new KeywordTokenizer("let", "if", "while", "return", "function", "break", "null"),
                        new NameTokenizer(),
                        new NumberTokenizer(),
                        new WordTokenizer("==", "!=", "&&", "||", "<=", ">="),
                        new BlankSpaceSkipTokenizer());
                }
                return defaultTokenizer;
            }
        }
        
        IEnumerable<ITokenizer> Tokenizers;
        public CombinedTokenizer(params ITokenizer[] tokenizers) 
        {
            Tokenizers = tokenizers;
        }
        public IEnumerable<Token> Tokenize(IEnumerable<Token> script)
        {
            var res = script.ToList();
            foreach (var tokenizer in Tokenizers) 
            {
                res = tokenizer.Tokenize(res).ToList();
            }
            return res;
        }
    }
}

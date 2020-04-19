using System.Collections.Generic;
using System.Linq;

namespace ScriptingLaunguage.Tokenizer
{
    public class KeywordTokenizer : ITokenizer
    {
        string[] Keywords;
        public KeywordTokenizer(params string[] keywords)
        {
            Keywords = keywords;
        }

        bool ReadKeyword(string keyword, IEnumerable<Token> script) 
        {
            var str = "";
            var beginning = script.Take(keyword.Length);
            var next = script.Skip(keyword.Length);
            foreach (var token in beginning) 
            {
                if (!Utils.IsNameSymbol(token))
                {
                    return false;
                }
                str += token.Name;
            }
            if (str != keyword) 
            {
                return false;
            }

            var nextToken = next.FirstOrDefault();
            if (nextToken != null && Utils.IsNameSymbol(nextToken))
            {
                return false;
            }

            return true;
        }

        IEnumerable<Token> TryReadSingleKeyword(IEnumerable<Token> script, out Token processedToken) 
        {
            foreach (var keyword in Keywords) 
            {
                if (ReadKeyword(keyword, script)) 
                {
                    processedToken = new Token { Name = keyword };
                    return script.Skip(keyword.Length);
                }
            }

            processedToken = script.First();
            return script.Skip(1); 
        }
        public IEnumerable<Token> Tokenize(IEnumerable<Token> script)
        {
            var left = script;
            while (left.Any()) 
            {
                Token token = null;
                left = TryReadSingleKeyword(left, out token);
                yield return token;
            }
        }
    }
}

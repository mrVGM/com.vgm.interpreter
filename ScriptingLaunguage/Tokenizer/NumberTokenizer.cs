using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptingLaunguage.Tokenizer
{
    public class NumberTokenizer : ITokenizer
    {
        public IEnumerable<Token> TokenizeInt(IEnumerable<Token> script)
        {
            List<Token> buffer = new List<Token>();

            foreach (var token in script)
            {
                if (Utils.IsDigit(token))
                {
                    buffer.Add(token);
                }
                else if (buffer.Any())
                {
                    string str = "";
                    foreach (var t in buffer)
                    {
                        str += t.Name;
                    }
                    yield return new Token { Name = "Number", Data = str };
                    yield return token;
                    buffer.Clear();
                }
                else 
                {
                    yield return token;
                }
            }

            if (buffer.Any()) 
            {
                string str = "";
                foreach (var t in buffer)
                {
                    str += t.Name;
                }
                yield return new Token { Name = "Number", Data = str };
            }
        }

        int TryReadNumber(IEnumerable<Token> script, out Token processed) 
        {
            var tmp = script.Take(3).ToArray();
            if (tmp.Length == 3)
            {
                if (tmp[0].Name == "Number" && tmp[1].Name == "." && tmp[2].Name == "Number")
                {
                    processed = new Token { Name = "Number", Data = $"{tmp[0].Data}.{tmp[2].Data}" };
                    return 3;
                }
            }

            processed = tmp[0];
            return 1;
        }
        public IEnumerable<Token> Tokenize(IEnumerable<Token> script)
        {
            var left = TokenizeInt(script);
            while (left.Any())
            {
                Token token = null;
                int toSkip = TryReadNumber(left, out token);
                yield return token;
                left = left.Skip(toSkip);
            }
        }
    }
}

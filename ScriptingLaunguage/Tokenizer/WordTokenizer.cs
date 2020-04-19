using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptingLaunguage.Tokenizer
{
    public class WordTokenizer : ITokenizer
    {
        IEnumerable<string> Words;
        public WordTokenizer(params string[] words)
        {
            Words = words.OrderByDescending(x => x.Length);
        }

        bool TryReadWord(string word, IEnumerable<Token> script)
        {
            var beginning = script.Take(word.Length);
            if (beginning.Count() < word.Length || beginning.Any(x => x.Name.Length > 1))
            {
                return false;
            }

            string str = "";
            foreach (var symbol in beginning) 
            {
                str += symbol.Name;
            }

            if (str != word) 
            {
                return false;
            }

            return true;
        }

        int ReadWord(IEnumerable<Token> script, out Token token)
        {
            foreach (var word in Words)
            {
                if (TryReadWord(word, script)) 
                {
                    token = new Token { Name = word };
                    return word.Length;
                }
            }

            token = script.First();
            return 1;
        }
        public IEnumerable<Token> Tokenize(IEnumerable<Token> script)
        {
            var left = script;
            while (left.Any()) 
            {
                Token token = null;
                int read = ReadWord(left, out token);
                yield return token;
                left = left.Skip(read);
            }
        }
    }
}

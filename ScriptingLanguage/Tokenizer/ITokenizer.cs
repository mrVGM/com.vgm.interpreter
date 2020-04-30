using System.Collections.Generic;

namespace ScriptingLaunguage.Tokenizer
{
    public interface ITokenizer
    {
        IEnumerable<Token> Tokenize(IEnumerable<Token> script);
    }
}

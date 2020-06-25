using System.Collections.Generic;

namespace ScriptingLaunguage.Tokenizer
{
    public interface ITokenizer
    {
        IEnumerable<IndexedToken> Tokenize(IEnumerable<IndexedToken> script);
    }
}

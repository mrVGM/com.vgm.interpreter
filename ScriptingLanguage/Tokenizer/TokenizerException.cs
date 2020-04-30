using System;

namespace ScriptingLaunguage.Tokenizer
{
    public class TokenizerException : Exception
    {
        public TokenizerException()
        {
        }
        public TokenizerException(string info) : base(info) 
        {
        }
    }
}

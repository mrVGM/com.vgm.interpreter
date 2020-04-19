using System;
using System.Collections.Generic;

namespace ScriptingLaunguage.Tokenizer
{
    public class StringTokenizer : ITokenizer
    {
        enum State 
        {
            Default,
            InString,
            EscapingToken
        };
        public IEnumerable<Token> Tokenize(IEnumerable<Token> script)
        {
            var state = State.Default;
            List<Token> buffer = new List<Token>();

            foreach (var token in script)
            {
                if (token.Name == Environment.NewLine)
                {
                    if (state != State.Default)
                    {
                        throw new TokenizerException("Open Quotes!");
                    }

                    yield return token;
                    continue;
                }

                if (token.Name == "\\") 
                {
                    if (state == State.InString) 
                    {
                        state = State.EscapingToken;
                        continue;
                    }
                }

                if (token.Name == "\"") 
                {
                    if (state == State.Default) 
                    {
                        buffer.Clear();
                        state = State.InString;
                        continue;
                    }

                    if (state == State.InString) 
                    {
                        var dataStr = "";
                        foreach (var symbol in buffer) 
                        {
                            dataStr += symbol.Name;
                        }

                        var stringToken = new Token {
                            Name = "String",
                            Data = dataStr
                        };
                        yield return stringToken;
                        state = State.Default;
                        continue;
                    }
                }

                if (state != State.Default)
                {
                    buffer.Add(token);
                    if (state == State.EscapingToken)
                    {
                        state = State.InString;
                    }
                    continue;
                }

                yield return token;
            }

            if (state != State.Default) 
            {
                throw new TokenizerException("Open Quotes!");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using ScriptingLaunguage.Tokenizer;

namespace ScriptingLaunguage
{
    public static class Utils
    {
        public static IEnumerable<Token> TokenizeText(string text, Token endOfText = null)
        {
            foreach (var symbol in text)
            {
                yield return new Token {
                    Name = $"{symbol}"
                };
            }
            if (endOfText != null) 
            {
                yield return endOfText;
            }
        }

        public static bool IsNameSymbol(Token token) 
        {
            if (token.Name.Length > 1) 
            {
                return false;
            }
            char c = token.Name[0];
            if (c == '_') 
            {
                return true;
            }
            if ('0' <= c && c <= '9') 
            {
                return true;
            }
            if ('a' <= c && c <= 'z') 
            {
                return true;
            }
            if ('A' <= c && c <= 'Z')
            {
                return true;
            }

            return false;
        }

        public static bool IsNameStartSymbol(Token token) 
        {
            if (!IsNameSymbol(token)) 
            {
                return false;
            }
            char c = token.Name[0];
            if ('0' <= c && c <= '9') 
            {
                return false;
            }
            return true;
        }

        public static bool IsDigit(Token token)
        {
            if (token.Name.Length > 1)
            {
                return false;
            }
            char c = token.Name[0];
            return '0' <= c && c <= '9';
        }
    }
}

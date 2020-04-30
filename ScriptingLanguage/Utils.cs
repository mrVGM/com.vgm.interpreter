using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ScriptingLaunguage.Interpreter;
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

        public static object GetProperty(object obj, string propertyName)
        {
            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var type = obj.GetType();
            var propertyInfo = type.GetProperty(propertyName, flags);
            if (propertyInfo != null) {
                return propertyInfo.GetValue(obj);
            }

            var fieldInfo = type.GetField(propertyName, flags);
            if (fieldInfo != null) {
                return fieldInfo.GetValue(obj);
            }

            return null;
        }

        public static void SetProperty(object obj, string propertyName, object value)
        {
            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var type = obj.GetType();
            var propertyInfo = type.GetProperty(propertyName, flags);
            if (propertyInfo != null) {
                propertyInfo.SetValue(obj, value);
                return;
            }

            var fieldInfo = type.GetField(propertyName, flags);
            if (fieldInfo != null) {
                fieldInfo.SetValue(obj, value);
                return;
            }
        }

        public static Type GetTypeAcrossAssemblies(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        public static bool IsNumber(object obj)
        {
            if (obj == null) {
                return false;
            }
            return Number.SupportedTypes.Contains(obj.GetType());
        }

        public static Number ToNumber(object obj)
        {
            if (IsNumber(obj)) 
            {
                return new Number(Convert.ToDouble(obj));
            }
            return null;
        }

        public static bool GetArgumentFor(Type paramType, object argument, out object realParam)
        {
            if (argument == null)
            {
                realParam = null;
                return true;
            }

            if (argument is Number)
            {
                var requiredType = Number.SupportedTypes.FirstOrDefault(x => paramType.IsAssignableFrom(x));
                if (requiredType != null)
                {
                    realParam = (argument as Number).GetNumber(requiredType);
                    return true;
                }
                else
                {
                    realParam = argument;
                    return false;
                }
            }

            realParam = argument;
            if (paramType.IsAssignableFrom(argument.GetType()))
            {
                return true;
            }
            return false;
        }
    }
}

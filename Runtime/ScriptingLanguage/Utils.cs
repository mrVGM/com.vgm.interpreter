﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ScriptingLanguage.Tokenizer;

namespace ScriptingLanguage
{
    public static class Utils
    {
        public abstract class LanguageException : Exception
        {
            const int SurroundingLines = 2;

            public IIndexed CodeIndex;
            public ScriptId ScriptId;

            public override string Message => $"{base.Message}\n{GetErrorMessage(true)}";
            public LanguageException(string message, ScriptId scriptId, IIndexed codeIndex) : base(message)
            {
                ScriptId = scriptId;
                CodeIndex = codeIndex;
            }

            public IEnumerable<IndexedToken> GetSampleOfLines(int lineOfInterest, int numberOfSurroundingLines, ScriptId scriptId)
            {
                var lines = scriptId.Lines;
                foreach (var line in lines)
                {
                    if (Math.Abs(line.Index - lineOfInterest) <= numberOfSurroundingLines)
                    {
                        yield return line;
                    }
                }
            }

            public string GetCodeSample(IIndexed index, ScriptId scriptId, bool printLineNumbers)
            {
                int lineOfInterest = scriptId.GetLineNumber(index);
                var sample = GetSampleOfLines(lineOfInterest, SurroundingLines, scriptId);

                var errorLine = sample.FirstOrDefault(x => x.Index == lineOfInterest);
                int errorLineOffset = scriptId.GetLineOffset(index);

                string pointerLine = Utils.PointSymbol(errorLineOffset, scriptId.GetStringOfLine(errorLine));

                string lineNumberSuffix = "| ";
                int longestPrefixLength = (sample.Last().Index + 1).ToString().Length + lineNumberSuffix.Length;
                string blankPrefix = "";
                while (blankPrefix.Length < longestPrefixLength)
                {
                    blankPrefix += " ";
                }

                string getPrefix(int lineNumber)
                {
                    if (!printLineNumbers)
                    {
                        return "";
                    }
                    string lineNumberStr = $"{lineNumber}{lineNumberSuffix}";
                    while (lineNumberStr.Length < longestPrefixLength)
                    {
                        lineNumberStr = $" {lineNumberStr}";
                    }
                    return lineNumberStr;
                }

                if (printLineNumbers)
                {
                    pointerLine = $"{blankPrefix}{pointerLine}";
                }
                string res = "";
                foreach (var line in sample)
                {
                    var lineString = scriptId.GetStringOfLine(line);
                    res += $"{getPrefix(line.Index)}{lineString}";
                    if (line.Index == lineOfInterest)
                    {
                        if (!lineString.EndsWith(NewLineTokenizer.LF) && !lineString.EndsWith(NewLineTokenizer.CRLF)) {
                            res += "\n";
                        }
                        res += $"{pointerLine}\n";
                    }
                }

                if (string.IsNullOrEmpty(ScriptId.Filename))
                {
                    return res;
                }

                return $"{ScriptId.Filename}\n{res}";
            }
            public abstract string GetErrorMessage(bool printLineNumbers);
        }

        public static string PointSymbol(int index, string line) 
        {
            var replaced = line.Select(x => {
                if (x == '\t') {
                    return x;
                }
                return ' ';
            }).ToList();

            while (replaced.Count() <= index) 
            {
                replaced.Add(' ');
            }
            replaced[index] = '^';

            return new string(replaced.ToArray());
        }

        public static IEnumerable<IndexedToken> TokenizeText(ScriptId scriptId, IToken endOfText = null)
        {
            var text = scriptId.Script;
            int index = 0;
            foreach (var symbol in text)
            {
                yield return new IndexedToken(index++, scriptId) {
                    Name = $"{symbol}"
                };
            }
            if (endOfText != null) 
            {
                yield return new IndexedToken(index, scriptId) { Name = endOfText.Name, Data = endOfText.Data };
            }
        }

        public static bool IsNameSymbol(IToken token) 
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

        public static bool IsNameStartSymbol(IToken token) 
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

        public static bool IsDigit(IToken token)
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
            var type = obj.GetType();
            return GetProperty(obj, type, propertyName);
        }

        public static object GetProperty(object obj, Type type, string propertyName)
        {
            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var propertyInfo = type.GetProperty(propertyName, flags);
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(obj);
            }

            var fieldInfo = type.GetField(propertyName, flags);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(obj);
            }

            return null;
        }

        public static void SetProperty(object obj, string propertyName, object value) 
        {
            var type = obj.GetType();
            SetProperty(obj, type, propertyName, value);
        }
        public static void SetProperty(object obj, Type type, string propertyName, object value)
        {
            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
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
            return IsNumber(obj.GetType());
        }
    }
}

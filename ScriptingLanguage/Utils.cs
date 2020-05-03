using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ScriptingLaunguage.Interpreter;
using ScriptingLaunguage.Tokenizer;

namespace ScriptingLaunguage
{
    public static class Utils
    {
        public class NumberedLine 
        {
            public int LineIndex;
            public string Line;
        }

        public abstract class LanguageException : Exception
        {
            const int SurroundingLines = 2;

            public int CodeIndex;
            public ScriptId ScriptId;

            public override string Message => $"{base.Message}{Environment.NewLine}{GetErrorMessage(true)}";
            public LanguageException(string message, ScriptId scriptId, int codeIndex) : base(message)
            {
                ScriptId = scriptId;
                CodeIndex = codeIndex;
            }

            public IEnumerable<NumberedLine> GetSampleOfLines(int lineOfInterest, int numberOfSurroundingLines, string script)
            {
                var lines = Utils.GetNumberedLines(script);
                foreach (var line in lines)
                {
                    if (Math.Abs(line.LineIndex - lineOfInterest) <= numberOfSurroundingLines)
                    {
                        yield return line;
                    }
                }
            }

            public string GetCodeSample(int index, string script, bool printLineNumbers)
            {
                int lineOfInterest = Utils.GetLineNumber(index, script);
                var sample = GetSampleOfLines(lineOfInterest, SurroundingLines, script);

                var errorLine = sample.FirstOrDefault(x => x.LineIndex == lineOfInterest);
                int errorLineOffset = Utils.GetLineOffset(index, script);

                string pointerLine = Utils.PointSymbol(errorLineOffset, errorLine.Line);

                string lineNumberSuffix = "| ";
                int longestPrefixLength = (sample.Last().LineIndex + 1).ToString().Length + lineNumberSuffix.Length;
                string blankPrefix = "";
                while (blankPrefix.Length < longestPrefixLength)
                {
                    blankPrefix += " ";
                }

                string getPrefix(NumberedLine line)
                {
                    if (!printLineNumbers)
                    {
                        return "";
                    }
                    string lineNumber = $"{line.LineIndex + 1}{lineNumberSuffix}";
                    while (lineNumber.Length < longestPrefixLength)
                    {
                        lineNumber = $" {lineNumber}";
                    }
                    return lineNumber;
                }

                if (printLineNumbers)
                {
                    pointerLine = $"{blankPrefix}{pointerLine}";
                }
                string res = "";
                foreach (var line in sample)
                {
                    res += $"{getPrefix(line)}{line.Line}{Environment.NewLine}";
                    if (line.LineIndex == lineOfInterest)
                    {
                        res += $"{pointerLine}{Environment.NewLine}";
                    }
                }

                if (string.IsNullOrEmpty(ScriptId.Filename))
                {
                    return res;
                }

                return $"{ScriptId.Filename}{Environment.NewLine}{res}";
            }
            public abstract string GetErrorMessage(bool printLineNumbers);
        }

        public static IEnumerable<int> GetNewLineIndeces(string source)
        {
            int curIndex = 0;
            while (true) 
            {
                int foundIndex = source.IndexOf(Environment.NewLine, curIndex);
                if (foundIndex < 0) 
                {
                    break;
                }
                curIndex = foundIndex + Environment.NewLine.Length;
                yield return foundIndex;
            }
        }

        public static IEnumerable<NumberedLine> GetNumberedLines(string text)
        {
            var newLineIndeces = GetNewLineIndeces(text);
            var newLinesCount = newLineIndeces.Count();

            for (int i = 0; i < newLinesCount; ++i) 
            {
                yield return new NumberedLine { LineIndex = i, Line = GetLine(i, text) };
            }
            string lastLine = GetLine(newLinesCount, text);
            if (!string.IsNullOrEmpty(lastLine)) 
            {
                yield return new NumberedLine { LineIndex = newLinesCount, Line = lastLine };
            }
        }
        
        public static int GetLineNumber(int index, string source)
        {
            var newLineIndeces = GetNewLineIndeces(source);
            return newLineIndeces.Count(x => x <= index);
        }

        public static string GetLine(int index, string source) 
        {
            var newLineIndeces = GetNewLineIndeces(source).ToList();
            
            if (index > newLineIndeces.Count)
            {
                return "";
            }
            if (index == 0) 
            {
                return source.Substring(0, newLineIndeces[0]);
            }
            int startIndex = newLineIndeces[index - 1] + Environment.NewLine.Length;
            int endIndex = source.Length;
            if (index < newLineIndeces.Count) 
            {
                endIndex = newLineIndeces[index];
            }
            return source.Substring(startIndex, endIndex - startIndex);
        }

        public static int GetLineOffset(int index, string source) 
        {
            var newLines = GetNewLineIndeces(source);
            int startLine = 0;
            if (newLines.Any(x => x < index)) 
            {
                startLine = newLines.Last(x => x < index) + Environment.NewLine.Length;
            }

            return index - startLine;
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

        public static IEnumerable<IndexedToken> TokenizeText(string text, ScriptId scriptId, IToken endOfText = null)
        {
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

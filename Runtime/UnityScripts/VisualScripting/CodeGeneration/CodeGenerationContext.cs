using ScriptingLanguage.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ScriptingLanguage.VisualScripting.CodeGeneration
{
    public class CodeGenerationContext
    {
        private Stack<HashSet<string>> _scopes = new Stack<HashSet<string>>();
        public object CustomContext { get; private set; }
        private int counter = 0;

        public interface ITemporaryScope : IDisposable { }
        public interface ITemporaryCustomContext : IDisposable { }

        private class TemporaryScope : ITemporaryScope
        {
            CodeGenerationContext _context;
            public TemporaryScope(CodeGenerationContext context, IEnumerable<string> varNames)
            {
                _context = context;
                _context.PushScope(varNames);
            }
            public void Dispose()
            {
                _context.PopScope();
            }
        }

        private class TemporaryCustomContext : ITemporaryCustomContext
        {
            CodeGenerationContext _context;
            object previousContext;
            public TemporaryCustomContext(CodeGenerationContext context, object customContext)
            {
                _context = context;
                previousContext = _context.CustomContext;
                _context.CustomContext = customContext;
            }
            public void Dispose()
            {
                _context.CustomContext = previousContext;
            }
        }

        private void PushScope(IEnumerable<string> varNames)
        {
            _scopes.Push(new HashSet<string>(varNames));
        }
        private void PopScope()
        {
            _scopes.Pop();
        }

        public ITemporaryScope CreateTemporaryScope(params string[] varNames)
        {
            return new TemporaryScope(this, varNames);
        }

        public ITemporaryCustomContext CreateTemporaryCustomContext(object customContext)
        {
            return new TemporaryCustomContext(this, customContext);
        }

        public bool IsVariableDeclared(string varName) 
        {
            foreach (var scope in _scopes) {
                if (scope.Contains(varName)) {
                    return true;
                }
            }
            return false;
        }

        public void DeclaredVar(string varName)
        {
            var curScope = _scopes.Peek();
            curScope.Add(varName);
        }

        public string GenerateVarName()
        {
            return $"var_{counter++}";
        }
    }
}
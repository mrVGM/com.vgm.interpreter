using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ScriptingLaunguage.Interpreter;

namespace ScriptingLaunguage.BaseFunctions
{
    class LengthFunction : IFunction
    {
        const string argName = "collectionToCount";
        Scope scope;
        public Scope ScopeTemplate
        {
            get 
            {
                if (scope == null) 
                {
                    scope = new Scope { ParentScope = Interpreter.Interpreter.GlobalScope };
                    scope.AddVariable(argName, null);
                }
                return scope;
            }
        }

        public string[] ParameterNames { get; private set; } = { argName };

        public object Execute(Scope scope)
        {
            var col = scope.GetVariable(argName, InCurrentScope: true) as IEnumerable;
            if (col == null) 
            {
                return 0;
            }
            int res = 0;
            foreach (var obj in col) 
            {
                ++res;
            }
            return res;
        }
    }
}

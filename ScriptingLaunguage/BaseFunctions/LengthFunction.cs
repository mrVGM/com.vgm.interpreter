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
        public Scope Scope 
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

        public object Result { get; private set; }

        public void Execute()
        {
            Result = 0;
            var col = Scope.GetVariable(argName, InCurrentScope: true) as IEnumerable;
            if (col == null) 
            {
                return;
            }
            int res = 0;
            foreach (var obj in col) 
            {
                ++res;
            }
            Result = res;
        }
    }
}

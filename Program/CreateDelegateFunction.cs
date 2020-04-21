using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using ScriptingLaunguage.Interpreter;

namespace Program
{
    class CreateDelegateFunction : IFunction
    {
        const string delegateType = "delegate_type";
        const string returnType = "return_value_type";
        const string argument_types = "argument_types";
        const string functionToExecute = "function_to_execute";
        const string functionToExecuteId = "function_to_execute_id";

        string[] parameters = { delegateType, returnType, argument_types, functionToExecute, functionToExecuteId };
        public Scope ScopeTemplate
        {
            get
            {
                var scope = new Scope();
                scope.AddVariable(delegateType, null);
                scope.AddVariable(returnType, null);
                scope.AddVariable(argument_types, null);
                scope.AddVariable(functionToExecute, null);
                scope.AddVariable(functionToExecuteId, null);
                return scope;
            }
        }

        public string[] ParameterNames => parameters;

        public static void Handler(object arg1, object arg2, object arg3, object arg4, int id)
        {

            var func = delegateIds[id];
            var scope = func.ScopeTemplate;
            object[] argsToPass = { arg1, arg2, arg3, arg4 };
            for (int i = 0; i < Math.Min(func.ParameterNames.Length, argsToPass.Count()); ++i)
            {
                scope.AddVariable(func.ParameterNames[i], argsToPass[i]);
            }

            func.Execute(scope);
        }

        static Dictionary<int, IFunction> delegateIds = new Dictionary<int, IFunction>();
       
        public object Execute(Scope scope)
        {
            var argsTypeNames = scope.GetVariable(argument_types) as IEnumerable<object>;
            var delegateArgs = argsTypeNames.Select(x => Type.GetType(x as string)).ToArray();
            var retType = Type.GetType(scope.GetVariable(returnType) as string);

            int argsCount = delegateArgs.Length;

            DynamicMethod hello = new DynamicMethod("asd",
                retType,
                delegateArgs,
                typeof(CreateDelegateFunction).Module);

            var method = typeof(CreateDelegateFunction).GetMethod("Handler");

            int functionId = Convert.ToInt32(scope.GetVariable(functionToExecuteId));

            ILGenerator il = hello.GetILGenerator(256);
            if (argsCount >= 1)
                il.Emit(OpCodes.Ldarg_0);
            if (argsCount >= 2)
                il.Emit(OpCodes.Ldarg_1);
            if (argsCount >= 3)
                il.Emit(OpCodes.Ldarg_2);
            if (argsCount >= 4)
                il.Emit(OpCodes.Ldarg_3);

            for (int i = 0; i < 4 - argsCount; ++i) 
            {
                il.Emit(OpCodes.Ldnull);
            }
            il.Emit(OpCodes.Ldc_I4, functionId);

            il.EmitCall(OpCodes.Call, method, null);
            if (retType != typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            il.Emit(OpCodes.Ret);

            var typeOfDelegate = scope.GetVariable(delegateType) as Type;
            var function = hello.CreateDelegate(typeOfDelegate);

            delegateIds[functionId] = scope.GetVariable(functionToExecute) as IFunction;

            return function;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using ScriptingLaunguage.Interpreter;

namespace ScriptingLaunguage.BaseFunctions
{
    class CreateDelegateFunction : IFunction
    {
        const string delegateType = "delegate_type";
        const string functionToExecute = "function_to_execute";
        const string functionToExecuteId = "function_to_execute_id";

        string[] parameters = { delegateType, functionToExecute, functionToExecuteId };
        public Scope ScopeTemplate
        {
            get
            {
                var scope = new Scope();
                scope.AddVariable(delegateType, null);
                scope.AddVariable(functionToExecute, null);
                scope.AddVariable(functionToExecuteId, null);
                return scope;
            }
        }

        public string[] ParameterNames => parameters;

        public static void Handler<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, int id)
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
            var typeOfDelegate = scope.GetVariable(delegateType) as Type;
            var delegateMethod = typeOfDelegate.GetMethod("Invoke");

            var delegateArgs = delegateMethod.GetParameters().Select(x => x.ParameterType).ToArray();
            var retType = delegateMethod.ReturnType;

            int argsCount = delegateArgs.Length;

            DynamicMethod dynamicMethod = new DynamicMethod("",
                retType,
                delegateArgs,
                typeof(CreateDelegateFunction).Module);

            List<Type> handlerTemplateArgs = new List<Type>();
            handlerTemplateArgs.AddRange(delegateArgs);
            while (handlerTemplateArgs.Count() < 4) 
            {
                handlerTemplateArgs.Add(typeof(object));
            }

            var method = typeof(CreateDelegateFunction).GetMethod("Handler");
            method = method.MakeGenericMethod(handlerTemplateArgs.ToArray());

            int functionId = (int) (scope.GetVariable(functionToExecuteId) as Number).GetNumber(typeof(int));

            ILGenerator il = dynamicMethod.GetILGenerator(256);
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

            var function = dynamicMethod.CreateDelegate(typeOfDelegate);

            delegateIds[functionId] = scope.GetVariable(functionToExecute) as IFunction;

            return function;
        }
    }
}

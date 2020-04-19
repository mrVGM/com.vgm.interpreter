using System;
using ScriptingLaunguage.Interpreter;

namespace ScriptingLaunguage.BaseFunctions
{
	class PrintFunction : IFunction
	{
		Scope scope;
		public Scope Scope
		{
			get
			{
				if (scope == null)
				{
					scope = new Scope();
					scope.AddVariable("str", null);
				}
				return scope;
			}
		}

		public string[] ParameterNames { get; set; } = new string[] { "str" };

		public object Result => null;

		public void Execute()
		{
			Console.WriteLine(Scope.GetVariable("str") as string);
		}
	}
}

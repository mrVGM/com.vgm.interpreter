using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    class ComparisonProcessor : IProgramNodeProcessor
    {
        IProgramNodeProcessor ExpressionProcessor = new ExpressionNodeProcessor();
        public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
        {
            if (programNode.Token.Name != "Comparison") 
            {
                throw new NotSupportedException();
            }

            var expr1 = programNode.Children[0];
            var expr2 = programNode.Children[2];

            if (expr1.Token.Name != "Expression" || expr2.Token.Name != "Expression") 
            {
                throw new NotSupportedException();
            }

            object e1 = null;
            object e2 = null;
            ExpressionProcessor.ProcessNode(expr1, scope, ref e1);
            ExpressionProcessor.ProcessNode(expr2, scope, ref e2);

            if (programNode.MatchChildren("Expression", "==", "Expression")) 
            {
                if (e1 is float || e1 is int) 
                {
                    value = Convert.ToSingle(e1) == Convert.ToSingle(e2);
                    return true;
                }

                var method = e1.GetType().GetMethod("op_Equality");
                value = method.Invoke(null, new object[] { e1, e2 });
                return true;
            }

            if (programNode.MatchChildren("Expression", "!=", "Expression"))
            {
                if (e1 is float || e1 is int)
                {
                    value = Convert.ToSingle(e1) != Convert.ToSingle(e2);
                    return true;
                }

                var method = e1.GetType().GetMethod("op_Inequality");
                value = method.Invoke(null, new object[] { e1, e2 });
                return true;
            }

            if (programNode.MatchChildren("Expression", "<", "Expression"))
            {
                if (e1 is float || e1 is int) {
                    value = Convert.ToSingle(e1) < Convert.ToSingle(e2);;
                    return true;
                }

                var method = e1.GetType().GetMethod("op_LessThan");
                value = method.Invoke(null, new object[] { e1, e2 });
                return true;
            }

            if (programNode.MatchChildren("Expression", ">", "Expression"))
            {
                if (e1 is float || e1 is int)
                {
                    value = Convert.ToSingle(e1) > Convert.ToSingle(e2);
                    return true;
                }

                var method = e1.GetType().GetMethod("op_GreaterThan");
                value = method.Invoke(null, new object[] { e1, e2 });
                return true;
            }

            if (programNode.MatchChildren("Expression", "<=", "Expression"))
            {
                if (e1 is float || e1 is int)
                {
                    value = Convert.ToSingle(e1) <=  Convert.ToSingle(e2);
                    return true;
                }

                var method = e1.GetType().GetMethod("op_LessThanOrEqual");
                value = method.Invoke(null, new object[] { e1, e2 });
                return true;
            }

            if (programNode.MatchChildren("Expression", ">=", "Expression"))
            {
                if (e1 is float || e1 is int)
                {
                    value = Convert.ToSingle(e1) >= Convert.ToSingle(e2);
                    return true;
                }

                var method = e1.GetType().GetMethod("op_GreaterThanOrEqual");
                value = method.Invoke(null, new object[] { e1, e2 });
                return true;
            }

            throw new NotImplementedException();
        }
    }
}

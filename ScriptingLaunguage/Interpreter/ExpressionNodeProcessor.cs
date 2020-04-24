using System;
using System.Linq;
using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    public class ExpressionNodeProcessor : IProgramNodeProcessor
    {
        static IProgramNodeProcessor ValueProcessor = new ValueNodeProcessor();
        static IProgramNodeProcessor ArithmeticExpr = new ArithmeticExpression();
        static IProgramNodeProcessor ProductExpr = new ProductExpression();
        static IProgramNodeProcessor ExpressionProcessor = new ExpressionNodeProcessor();

        class ArithmeticExpression : IProgramNodeProcessor
        {
            public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
            {
                if (programNode.Token.Name != "ArithmethicExpression") 
                {
                    throw new Exception("Unsupported Node Type!");
                }

                if (programNode.MatchChildren("Prod")) 
                {
                    return ProductExpr.ProcessNode(programNode.Children[0], scope, ref value);
                }
                if (programNode.MatchChildren("ArithmethicExpression", "+", "Prod")) 
                {
                    object val1 = null;
                    ProcessNode(programNode.Children[0], scope, ref val1);
                    
                    object val2 = null;
                    ProductExpr.ProcessNode(programNode.Children[2], scope, ref val2);

                    if (val1 is Single || val1 is int) 
                    {
                        value = (Single)val1 + (Single)val2;
                        return true;
                    }

                    var type = val1.GetType();
                    var method = type.GetMethod("op_Addition");
                    object[] par = { val1, val2 };
                    value = method.Invoke(null, par);
                    return true;
                }
                if (programNode.MatchChildren("ArithmethicExpression", "-", "Prod"))
                {
                    object val1 = null;
                    ProcessNode(programNode.Children[0], scope, ref val1);

                    object val2 = null;
                    ProductExpr.ProcessNode(programNode.Children[2], scope, ref val2);

                    if (val1 is Single || val1 is int)
                    {
                        value = (Single)val1 - (Single)val2;
                        return true;
                    }

                    var type = val1.GetType();
                    var method = type.GetMethod("op_Subtraction");
                    object[] par = { val1, val2 };
                    value = method.Invoke(null, par);
                    return true;
                }

                throw new NotImplementedException();
            }
        }

        class ProductExpression : IProgramNodeProcessor
        {
            public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
            {
                if (programNode.Token.Name != "Prod")
                {
                    throw new Exception("Unsupported Node Type!");
                }

                if (programNode.MatchChildren("SingleValue"))
                {
                    return ExpressionProcessor.ProcessNode(programNode.Children[0], scope, ref value);
                }
                if (programNode.MatchChildren("Prod", "*", "SingleValue"))
                {
                    object val1 = null;
                    ProcessNode(programNode.Children[0], scope, ref val1);

                    object val2 = null;
                    ProductExpr.ProcessNode(programNode.Children[2], scope, ref val2);

                    if (val1 is Single || val1 is int)
                    {
                        value = (Single)val1 * (Single)val2;
                        return true;
                    }

                    var type = val1.GetType();
                    var method = type.GetMethod("op_Multiply");
                    object[] par = { val1, val2 };
                    value = method.Invoke(null, par);
                    return true;
                }
                if (programNode.MatchChildren("Prod", "/", "SingleValue"))
                {
                    object val1 = null;
                    ProcessNode(programNode.Children[0], scope, ref val1);

                    object val2 = null;
                    ProductExpr.ProcessNode(programNode.Children[2], scope, ref val2);

                    if (val1 is Single || val1 is int)
                    {
                        value = (Single)val1 / (Single)val2;
                        return true;
                    }

                    var type = val1.GetType();
                    var method = type.GetMethod("op_Division");
                    object[] par = { val1, val2 };
                    value = method.Invoke(null, par);
                    return true;
                }

                throw new NotImplementedException();
            }
        }

        public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
        {
            var token = programNode.Token;
            var childrenNames = programNode.Children.Select(x => x.Token.Name).ToArray();
            if (token.Name == "Expression")
            {
                return ProcessNode(programNode.Children[0], scope, ref value);
            }

            if (token.Name == "ArithmethicExpression") 
            {
                return ArithmeticExpr.ProcessNode(programNode, scope, ref value);
            }

            if (token.Name == "SingleValue") 
            {
                if (programNode.MatchChildren("Value")) 
                {
                    var res = ValueProcessor.ProcessNode(programNode.Children[0], scope, ref value);
                    var objectContainer = value as GenericObject.ObjectContainer;
                    if (objectContainer != null) {
                        value = objectContainer.ObjectValue;
                    }

                    return res;
                }
                if (programNode.MatchChildren("(", "ArithmethicExpression", ")"))
                {
                    return ArithmeticExpr.ProcessNode(programNode.Children[1], scope, ref value);
                }

                throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }
}

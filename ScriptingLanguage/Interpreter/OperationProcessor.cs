using System;
using System.Linq;
using System.Reflection;
using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    public class OperationProcessor : IProgramNodeProcessor
    {
        public class BreakOperation { }
        public class ReturnOperation 
        {
            public bool ReturnExpression = true;
        }

        static IProgramNodeProcessor ValueProcessor = new ValueNodeProcessor();
        static IProgramNodeProcessor ExpressionProcessor = new ExpressionNodeProcessor();
        static IProgramNodeProcessor BooleanExpressionProcessor = new BooleanExpressionProcessor();
        static IProgramNodeProcessor NameProcessor = new NameProcessor();
        static IProgramNodeProcessor AssignProcessor = new AssignmentProcessor();
        static IProgramNodeProcessor DeclarationProcessor = new DeclarationProcessor();
        static IProgramNodeProcessor IfProcessor = new IfProcessor();
        static IProgramNodeProcessor WhileProcessor = new WhileProcessor();

        class SetValueProcessor : IProgramNodeProcessor
        {
            object ValueToSet;
            public SetValueProcessor(object valueToSet) 
            {
                ValueToSet = valueToSet;
            }
            public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
            {
                if (programNode.Token.Name != "Value") 
                {
                    throw new NotSupportedException("Wrong node supported");
                }

                if (programNode.MatchChildren("Name")) 
                {
                    var child = programNode.Children[0];
                    scope.SetVariable(child.Token.Data as string, ValueToSet);
                    return null;
                }

                if (programNode.MatchChildren("Value", ".", "Name")) 
                {
                    object val = null;
                    ValueProcessor.ProcessNode(programNode.Children[0], scope, ref val);
                    
                    string propertyName = programNode.Children[2].Token.Data as string;
                    
                    var genericObject = val as GenericObject;
                    if (genericObject != null)
                    {
                        var objectContainer = genericObject.GetPropoerty(propertyName);
                        objectContainer.ObjectValue = ValueToSet;
                        return true;
                    }
                    
                    Utils.SetProperty(val, propertyName, ValueToSet);
                    return true;
                }

                if (programNode.MatchChildren("Value", "[", "Expression", "]")) 
                {
                    object val = null;
                    ValueProcessor.ProcessNode(programNode.Children[0], scope, ref val);

                    object expr = null;
                    ExpressionProcessor.ProcessNode(programNode.Children[2], scope, ref expr);

                    var type = val.GetType();
                    var genericArguments = type.GetGenericArguments();

                    if (genericArguments.Count() == 0)
                    {
                        var method = type.GetMethod("Set");
                        method.Invoke(val, new object[] { (int)(float)expr, ValueToSet });
                        return true;
                    }

                    if (genericArguments.Count() == 1) 
                    {
                        var index = (int)((float)expr);
                        var method = type.GetMethod("set_Item");
                        method.Invoke(val, new object[] { (int)(float)expr, ValueToSet });
                        return true;
                    }

                    if (genericArguments.Count() == 2)
                    {
                        var key = expr;
                        if (typeof(int).IsAssignableFrom(genericArguments[0]) && expr is Single)
                        {
                            key = (int)((float)expr);
                        }

                        var method = type.GetMethod("set_Item");
                        method.Invoke(val, new object[] { key, ValueToSet });
                        return true;
                    }

                    throw new NotImplementedException();
                }
                throw new NotImplementedException();
            }
        }

        class AssignmentProcessor : IProgramNodeProcessor
        {
            public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
            {
                if (programNode.Token.Name != "Assignment")
                {
                    throw new Exception("Wrong node supported");
                }

                if (programNode.MatchChildren("Value", "=", "Expression", ";")) 
                {
                    object val1 = null;
                    ValueProcessor.ProcessNode(programNode.Children[0], scope, ref val1);

                    object val2 = null;
                    ExpressionProcessor.ProcessNode(programNode.Children[2], scope, ref val2);
                    if (val1 is GenericObject.ObjectContainer)
                    {
                        var objectContainer = val1 as GenericObject.ObjectContainer;
                        objectContainer.ObjectValue = val2;
                        return true;
                    }

                    var setValueProcessor = new SetValueProcessor(val2);
                    setValueProcessor.ProcessNode(programNode.Children[0], scope, ref value);
                    return true;
                }

                if (programNode.MatchChildren("Value", "=", "BooleanExpression", ";"))
                {
                    object val1 = null;
                    ValueProcessor.ProcessNode(programNode.Children[0], scope, ref val1);

                    object val2 = null;
                    BooleanExpressionProcessor.ProcessNode(programNode.Children[2], scope, ref val2);
                    if (val1 is GenericObject.ObjectContainer)
                    {
                        var objectContainer = val1 as GenericObject.ObjectContainer;
                        objectContainer.ObjectValue = val2;
                        return true;
                    }

                    var setValueProcessor = new SetValueProcessor(val2);
                    setValueProcessor.ProcessNode(programNode.Children[0], scope, ref value);
                    return true;
                }

                throw new NotImplementedException();
            }
        }
        public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
        {
            if (programNode.Token.Name != "Operation")
            {
                throw new Exception("Wrong node supported!");
            }
            if (programNode.MatchChildren("Value", ";")) 
            {
                return ValueProcessor.ProcessNode(programNode.Children[0], scope, ref value);
            }

            if (programNode.MatchChildren("Assignment"))
            {
                return AssignProcessor.ProcessNode(programNode.Children[0], scope, ref value);
            }

            if (programNode.MatchChildren("Declaration")) 
            {
                return DeclarationProcessor.ProcessNode(programNode.Children[0], scope, ref value);
            }

            if (programNode.MatchChildren("break", ";")) 
            {
                return new BreakOperation();
            }

            if (programNode.MatchChildren("IfStatement")) 
            {
                return IfProcessor.ProcessNode(programNode.Children[0], scope, ref value);
            }
            if (programNode.MatchChildren("WhileStatement"))
            {
                return WhileProcessor.ProcessNode(programNode.Children[0], scope, ref value);
            }
            if (programNode.MatchChildren("return", ";"))
            {
                value = null;
                return new ReturnOperation { ReturnExpression = false };
            }
            if (programNode.MatchChildren("return", "Expression", ";"))
            {
                ExpressionProcessor.ProcessNode(programNode.Children[1], scope, ref value);
                return new ReturnOperation();
            }
            if (programNode.MatchChildren("return", "BooleanExpression", ";"))
            {
                BooleanExpressionProcessor.ProcessNode(programNode.Children[1], scope, ref value);
                return new ReturnOperation();
            }

            throw new NotImplementedException();
        }
    }
}

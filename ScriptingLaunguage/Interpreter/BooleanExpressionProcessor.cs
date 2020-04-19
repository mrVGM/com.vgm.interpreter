using System;
using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    class BooleanExpressionProcessor : IProgramNodeProcessor
    {
        static IProgramNodeProcessor Conjunction = new ConjunctionProcessor();
        static IProgramNodeProcessor Negation = new NegationProcessor();
        static IProgramNodeProcessor BooleanExpression = new BooleanExpressionProcessor();
        static IProgramNodeProcessor Comparison = new ComparisonProcessor();

        class NegationProcessor : IProgramNodeProcessor
        {
            public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
            {
                if (programNode.Token.Name != "Negation") 
                {
                    throw new NotSupportedException();
                }

                if (programNode.MatchChildren("SingleBooleanExpression")) 
                {
                    BooleanExpression.ProcessNode(programNode.Children[0], scope, ref value);
                    return true;
                }
                if (programNode.MatchChildren("!", "SingleBooleanExpression"))
                {
                    object tmp = null;
                    BooleanExpression.ProcessNode(programNode.Children[0], scope, ref tmp);
                    value = !(bool)tmp;
                    return true;
                }
                throw new NotImplementedException();
            }
        }
        class ConjunctionProcessor : IProgramNodeProcessor
        {
            public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
            {
                if (programNode.Token.Name != "Conjunction")
                {
                    throw new NotSupportedException("Wrong node Supported!");
                }

                if (programNode.MatchChildren("Negation")) 
                {
                    Negation.ProcessNode(programNode.Children[0], scope, ref value);
                    return true;
                }

                if (programNode.MatchChildren("Conjunction", "&&", "Negation")) 
                {
                    object expr = null;
                    ProcessNode(programNode.Children[0], scope, ref expr);

                    if (!(bool)expr) 
                    {
                        value = false;
                        return true;
                    }

                    Negation.ProcessNode(programNode.Children[2], scope, ref value);
                    return true;
                }

                throw new NotImplementedException();
            }
        }

        public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
        {
            if (programNode.Token.Name != "BooleanExpression" && programNode.Token.Name != "SingleBooleanExpression") 
            {
                throw new NotSupportedException("Wrong node supported");
            }

            if (programNode.Token.Name == "BooleanExpression")
            {
                if (programNode.MatchChildren("Conjunction"))
                {
                    Conjunction.ProcessNode(programNode.Children[0], scope, ref value);
                    return true;
                }
                if (programNode.MatchChildren("BooleanExpression", "||", "Conjunction"))
                {
                    object expr = null;
                    ProcessNode(programNode.Children[0], scope, ref expr);

                    if ((bool)expr)
                    {
                        value = true;
                        return true;
                    }

                    object conj = null;
                    Conjunction.ProcessNode(programNode.Children[2], scope, ref conj);

                    value = (bool)conj;
                    return true;
                }
            }
            if (programNode.Token.Name == "SingleBooleanExpression") 
            {
                if (programNode.MatchChildren("(", "BooleanExpression", ")")) 
                {
                    ProcessNode(programNode.Children[0], scope, ref value);
                    return true;
                }
                if (programNode.MatchChildren("Comparison"))
                {
                    Comparison.ProcessNode(programNode.Children[0], scope, ref value);
                    return true;
                }
            }
            throw new NotImplementedException();
        }
    }
}

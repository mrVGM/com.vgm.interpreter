﻿using System;
using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    class WhileProcessor : IProgramNodeProcessor
    {
        IProgramNodeProcessor BlockProcessor = new BlockProcessor(new OperationGroupProcessor { StopOnBreak = true });
        IProgramNodeProcessor BooleanExpressionProcessor = new BooleanExpressionProcessor();
        public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
        {
            if (programNode.Token.Name != "WhileStatement") 
            {
                throw new NotSupportedException();
            }

            if (programNode.MatchChildren("while", "(", "BooleanExpression", ")", "Block")) 
            {
                while (true)
                {
                    object expr = null;
                    BooleanExpressionProcessor.ProcessNode(programNode.Children[2], scope, ref expr);
                    if ((bool)expr)
                    {
                        var whileScope = new Scope();
                        whileScope.ParentScope = scope;
                        var block = BlockProcessor.ProcessNode(programNode.Children[4], whileScope, ref value);
                        if (block is OperationProcessor.BreakOperation) 
                        {
                            break;
                        }

                        if (block is OperationProcessor.ReturnOperation)
                        {
                            return block;
                        }
                    }
                    else 
                    {
                        break;
                    }
                }
                return true;
            }

            throw new NotImplementedException();
        }
    }
}
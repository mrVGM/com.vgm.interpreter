﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    class ValueNodeProcessor : IProgramNodeProcessor
    {
        public IProgramNodeProcessor NumberProcessor = new NumberNodeProcessor();
        public IProgramNodeProcessor StringProcessor = new StringNodeProcessor();
        public IProgramNodeProcessor ExpressionProcessor = new ExpressionNodeProcessor();
        public IProgramNodeProcessor KeywordProcessor = DummyNodeProcessor.DummyProcessor;
        public IProgramNodeProcessor NameProcessor = new NameProcessor();
        public IProgramNodeProcessor FunctionCall = new FunctionCallProcessor();
        public IProgramNodeProcessor FunctionDeclaration = new FunctionDeclarationProcessor();

        public object ProcessNode(ProgramNode programNode, Scope scope, ref object value)
        {
            var token = programNode.Token;
            if (token.Name != "Value")
            {
                return false;
            }

            var childNode = programNode.Children.First();
            var nodeNames = programNode.Children.Select(x => x.Token.Name).ToArray();
            if (programNode.Children.Count == 1)
            {
                switch (childNode.Token.Name)
                {
                    case "Number":
                        return NumberProcessor.ProcessNode(childNode, scope, ref value);
                    case "String":
                        return StringProcessor.ProcessNode(childNode, scope, ref value);
                    case "Name":
                        return NameProcessor.ProcessNode(childNode, scope, ref value);
                    case "null":
                        return KeywordProcessor.ProcessNode(childNode, scope, ref value);
                    case "FunctionCall":
                        {
                            object tmp = null;
                            FunctionCall.ProcessNode(childNode, scope, ref tmp);
                            var funcSettings = tmp as FunctionCallProcessor.FunctionCallSettings;

                            var func = scope.GetVariable(funcSettings.FunctionName);
                            var iFunc = func as IFunction;
                            if (iFunc != null)
                            {
                                foreach (var param in iFunc.ParameterNames)
                                {
                                    iFunc.Scope.SetVariable(param, null);
                                }

                                if (iFunc.ParameterNames.Length > 0)
                                {
                                    int index = 0;
                                    foreach (var arg in funcSettings.Arguments)
                                    {
                                        iFunc.Scope.SetVariable(iFunc.ParameterNames[index++], arg);

                                        if (index == iFunc.ParameterNames.Length)
                                        {
                                            break;
                                        }
                                    }
                                }
                                iFunc.Execute();
                                value = iFunc.Result;
                                return null;
                            }
                            throw new NotImplementedException();
                        }
                    case "FunctionDeclaration":
                        {
                            object tmp = null;
                            FunctionDeclaration.ProcessNode(childNode, scope, ref tmp);
                            var funcScopeAndBlock = tmp as FunctionDeclarationProcessor.FunctionScopeAndBlock;
                            var functionScope = new Scope { ParentScope = scope };
                            foreach (var param in funcScopeAndBlock.ScopeVariables) 
                            {
                                functionScope.AddVariable(param, null);
                            }

                            var func = new Function
                            {
                                Block = funcScopeAndBlock.Block,
                                ParameterNames = funcScopeAndBlock.ScopeVariables.ToArray(),
                                Scope = functionScope,
                            };

                            value = func;
                            return null;
                        }
                    default:
                        return DummyNodeProcessor.DummyProcessor.ProcessNode(childNode, scope, ref value);
                }
            }

            if (programNode.MatchChildren("Value", ".", "Name"))
            {
                object val1 = null;
                ProcessNode(programNode.Children[0], scope, ref val1);
                string propertyName = programNode.Children[2].Token.Data as string;

                var genericObject = val1 as GenericObject;
                if (genericObject != null)
                {
                    value = genericObject.GetPropoerty(propertyName);
                    return true;
                }

                var type = val1.GetType();
                value = type.GetProperty(propertyName).GetValue(val1);
                return true;
            }

            if (programNode.MatchChildren("{", "}")) 
            {
                value = new GenericObject();
                return true;
            }

            if (programNode.MatchChildren("[", "]"))
            {
                value = new List<object>();
                return true;
            }

            if (programNode.MatchChildren("Value", "[", "Expression", "]"))
            {
                object val1 = null;
                ProcessNode(programNode.Children[0], scope, ref val1);
                object val2 = null;
                ExpressionProcessor.ProcessNode(programNode.Children[2], scope, ref val2);

                var type = val1.GetType();
                var genericArguments = type.GetGenericArguments();
                    
                if (val1 is IEnumerable) 
                {
                    if (genericArguments.Length == 0) 
                    {
                        var method = type.GetMethod("Get");
                        value = method.Invoke(val1, new object[] { (int)(float)val2 });
                        return true;
                    }
                    if (genericArguments.Length == 1) 
                    {
                        var method = type.GetMethod("GetEnumerator");
                        var enumerator = method.Invoke(val1, null);
                        var moveNext = enumerator.GetType().GetMethod("MoveNext");
                        for (int i = 0; i <= (int)(float)val2; ++i) 
                        {
                            moveNext.Invoke(enumerator, null);
                        }
                        var current = enumerator.GetType().GetProperty("Current");
                        value = current.GetValue(enumerator);
                        return true;
                    }
                    if (genericArguments.Length == 2) 
                    {
                        object key = val2;
                        if (val2 is Single && typeof(int).IsAssignableFrom(genericArguments[0])) 
                        {
                            key = (int)((float)val2);
                        }
                        var method = type.GetMethod("TryGetValue");
                        object res = null;
                        object[] par = { key, res };
                        method.Invoke(val1, par);
                        value = par[1];
                        return true;
                    }
                }
            }

            if (programNode.MatchChildren("Value", ".", "FunctionCall")) 
            {
                object obj = null;
                ProcessNode(programNode.Children[0], scope, ref obj);

                object tmp = null;
                FunctionCall.ProcessNode(programNode.Children[2], scope, ref tmp);
                var settings = tmp as FunctionCallProcessor.FunctionCallSettings;

                var genericObject = obj as GenericObject;
                if (genericObject != null)
                {
                    var property = genericObject.GetPropoerty(settings.FunctionName);
                    var iFunc = property.ObjectValue as IFunction;
                    if (iFunc != null)
                    {
                        foreach (var param in iFunc.ParameterNames)
                        {
                            iFunc.Scope.SetVariable(param, null);
                        }

                        if (iFunc.ParameterNames.Length > 0)
                        {
                            int index = 0;
                            foreach (var arg in settings.Arguments)
                            {
                                iFunc.Scope.SetVariable(iFunc.ParameterNames[index++], arg);

                                if (index == iFunc.ParameterNames.Length)
                                {
                                    break;
                                }
                            }
                        }
                        iFunc.Execute();
                        value = iFunc.Result;
                        return null;
                    }
                }
                else 
                {
                    var method = obj.GetType().GetMethod(settings.FunctionName);
                    var args = settings.Arguments.ToArray();
                    var types = method.GetParameters().Select(x => x.ParameterType).ToArray();

                    for (int i = 0; i < Math.Min(args.Length, types.Length); ++i) 
                    {
                        if (typeof(int).IsAssignableFrom(types[i])) 
                        {
                            args[i] = (int)(float)args[i];
                        }
                    }
                    if (!string.IsNullOrEmpty(settings.TemplateParamName)) 
                    {
                        var type = Type.GetType(settings.TemplateParamName);
                        method = method.MakeGenericMethod(type);
                    }
                    value = method.Invoke(obj, args);
                    return null;
                }
            }

            throw new NotImplementedException();
        }
    }
}
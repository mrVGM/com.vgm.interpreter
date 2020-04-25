using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Xml;
using ScriptingLaunguage.Parser;

namespace ScriptingLaunguage.Interpreter
{
    public class StaticMethodPath
    {
        public string Path = "";
    }
    class ValueNodeProcessor : IProgramNodeProcessor
    {
        public IProgramNodeProcessor NumberProcessor = new NumberNodeProcessor();
        public IProgramNodeProcessor StringProcessor = new StringNodeProcessor();
        public IProgramNodeProcessor ExpressionProcessor = new ExpressionNodeProcessor();
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
                        {
                            var name = childNode.Token.Data as string;
                            if (!scope.HasVariable(name))
                            {
                                value = new StaticMethodPath { Path = name };
                                return null;
                            }
                            return NameProcessor.ProcessNode(childNode, scope, ref value);
                        }
                    case "null":
                        value = null;
                        return null;
                    case "true":
                        value = true;
                        return null;
                    case "false":
                        value = false;
                        return null;
                    case "FunctionCall":
                        {
                            object tmp = null;
                            FunctionCall.ProcessNode(childNode, scope, ref tmp);
                            var funcSettings = tmp as FunctionCallProcessor.FunctionCallSettings;

                            var func = scope.GetVariable(funcSettings.FunctionName);
                            var iFunc = func as IFunction;
                            if (iFunc != null)
                            {
                                var functionScope = iFunc.ScopeTemplate;
                                foreach (var param in iFunc.ParameterNames)
                                {
                                    functionScope.SetVariable(param, null);
                                }

                                if (iFunc.ParameterNames.Length > 0)
                                {
                                    int index = 0;
                                    foreach (var arg in funcSettings.Arguments)
                                    {
                                        functionScope.SetVariable(iFunc.ParameterNames[index++], arg);

                                        if (index == iFunc.ParameterNames.Length)
                                        {
                                            break;
                                        }
                                    }
                                }
                                value = iFunc.Execute(functionScope);
                                return null;
                            }
                            throw new NotImplementedException();
                        }
                    case "FunctionDeclaration":
                        {
                            object tmp = null;
                            FunctionDeclaration.ProcessNode(childNode, scope, ref tmp);
                            var funcScopeAndBlock = tmp as FunctionDeclarationProcessor.FunctionScopeAndBlock;

                            var func = new Function(scope);
                            func.Block = funcScopeAndBlock.Block;
                            func.ParameterNames = funcScopeAndBlock.ScopeVariables.ToArray();

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

                var staticMethodPath = val1 as StaticMethodPath;
                if (staticMethodPath != null) 
                {
                    staticMethodPath.Path += $".{propertyName}";
                    value = staticMethodPath;
                    return null;
                }

                var genericObject = val1 as GenericObject;
                if (genericObject != null)
                {
                    value = genericObject.GetPropoerty(propertyName);
                    return true;
                }

                value = Utils.GetProperty(val1, propertyName);
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
                    if (genericArguments.Length < 2)
                    {
                        var tmpList = new List<object>();
                        foreach (var obj in val1 as IEnumerable) 
                        {
                            tmpList.Add(obj);
                        }
                        value = tmpList[(int)(float)val2];
                        return null;
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
                        var functionScope = iFunc.ScopeTemplate;
                        foreach (var param in iFunc.ParameterNames)
                        {
                            functionScope.SetVariable(param, null);
                        }

                        if (iFunc.ParameterNames.Length > 0)
                        {
                            int index = 0;
                            foreach (var arg in settings.Arguments)
                            {
                                functionScope.SetVariable(iFunc.ParameterNames[index++], arg);

                                if (index == iFunc.ParameterNames.Length)
                                {
                                    break;
                                }
                            }
                        }
                        value = iFunc.Execute(functionScope);
                        return null;
                    }
                    throw new NotImplementedException();
                }

                var staticMethodPath = obj as StaticMethodPath;
                Type typeForSearchMethodIn = obj.GetType();
                object objectToCallMethodOn = obj;
                if (staticMethodPath != null) 
                {
                    typeForSearchMethodIn = Utils.GetTypeAcrossAssemblies(staticMethodPath.Path);
                    objectToCallMethodOn = null;
                }
                
                var method = GetMethod(typeForSearchMethodIn, settings);
                var args = settings.Arguments.ToArray();
                var paramTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();

                for (int i = 0; i < args.Length; ++i) 
                {
                    Utils.GetArgumentFor(paramTypes[i], args[i], out args[i]);
                }

                value = method.Invoke(objectToCallMethodOn, args);
                return null;
                
            }

            throw new NotImplementedException();
        }

        MethodInfo GetMethod(Type type, FunctionCallProcessor.FunctionCallSettings settings) 
        {
            var methods = new List<MethodInfo>();

            var curType = type;
            while (curType != null) 
            {
                methods.AddRange(curType.GetMethods().Where(x => x.Name == settings.FunctionName));
                curType = curType.BaseType;
            }

            if (!string.IsNullOrEmpty(settings.TemplateParamName)) 
            {
                var genType = Utils.GetTypeAcrossAssemblies(settings.TemplateParamName);
                methods = methods.Where(x => x.GetGenericArguments().Length == 1)
                                 .Select(x => x.MakeGenericMethod(genType)).ToList();
            }

            if (!methods.Any()) 
            {
                return null;
            }

            var args = settings.Arguments.ToArray();
            settings.Arguments = args;

            bool methodPredicate(MethodInfo method)
            {
                var methodParameters = method.GetParameters();
                if (methodParameters.Length != settings.Arguments.Count())
                {
                    return false;
                }

                for (int i = 0; i < methodParameters.Length; ++i) 
                {
                    var arg = args[i];
                    var paramType = methodParameters[i].ParameterType;

                    object tmp = null;
                    if (!Utils.GetArgumentFor(paramType, arg, out tmp)) 
                    {
                        return false;
                    }
                }
                return true;
            }

            return methods.FirstOrDefault(methodPredicate);
        }
    }
}

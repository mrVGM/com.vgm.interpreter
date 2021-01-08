using ScriptingLanguage.Interpreter;
using ScriptingLanguage.Tokenizer;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace ScriptingLanguage.VisualScripting
{
    public class VisualScriptingSession : MonoBehaviour
    {
        public TextAsset ParserTable;

        private Parser.Parser _parser;

        private Session _session;
        private Interpreter.Interpreter _interpreter;

        Session Session
        {
            get
            {
                if (_session == null) {
                    InitSession();
                }
                return _session;
            }
        }

        Interpreter.Interpreter Interpreter
        {
            get
            {
                if (_interpreter == null)
                {
                    InitSession();
                }
                return _interpreter;
            }
        }

        Scope _rootScope = null;
        Scope RootScope 
        {
            get
            {
                if (_rootScope == null) {
                    var sessionScope = Session.GetWorkingScope();
                    sessionScope.AddVariable("requireBin", new RequireBin(this));
                    _rootScope = new Scope { ParentScope = sessionScope };
                }
                return _rootScope;
            }
        }

        class RequireBin : IFunction
        {
            const string filename = "filename";
            const string exports = "exports";

            public VisualScriptingSession Session { get; }
            
            public RequireBin(VisualScriptingSession session) 
            {
                Session = session;
            }

            public Scope ScopeTemplate
            {
                get
                {
                    var scope = new Scope { ParentScope = Session.RootScope };
                    scope.AddVariable(filename, null);
                    scope.AddVariable(exports, new GenericObject());

                    var localScope = new Scope { ParentScope = scope };
                    return localScope;
                }
            }

            public string[] ParameterNames { get; private set; } = { filename };

            public object Execute(Scope scope)
            {
                string scriptName = scope.GetVariable(filename) as string;
                var fullPath = Session.GetWorkingDir() + scriptName;
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"Cannot find script: {fullPath}!");
                }

                NodesDB nodesDB = null;
                using (FileStream fs = new FileStream(scriptName, FileMode.Open)) {
                    var bf = new BinaryFormatter();
                    nodesDB = bf.Deserialize(fs) as NodesDB;
                }

                var allNodes = nodesDB.NodesWithPositions.Select(x => x.Node);
                var allEndpoints = allNodes.SelectMany(x => x.Endpoints);

                foreach (var endpoint in allEndpoints) {
                    endpoint.RestoreLinkedEndpoints(allEndpoints);
                }
                foreach (var node in allNodes) {
                    nodesDB.AddNode(node);
                }

                var startingNode = allNodes.OfType<ExecutionStartNodeComponent.ExecutionStartNode>().FirstOrDefault();
                string code = startingNode.GenerateCode(startingNode.RightEndpoint, nodesDB, null);

                var scriptId = new ScriptId { Filename = filename, Script = code };
                var interpreter = new Interpreter.Interpreter(Session.Session);
                interpreter.RunScript(scope, scriptId);
                return scope.GetVariable("exports");
            }
        }

        private Parser.Parser Parser
        {
            get
            {
                if (_parser == null) {
                    var pt = ScriptingLanguage.Parser.ParserTable.Deserialize(ParserTable.bytes);
                    _parser = new Parser.Parser { ParserTable = pt };
                }
                return _parser;
            }
        }

        public void RunScript(string script)
        {
            var scriptID = new ScriptId { Script = script };
            Interpreter.RunScript(GetRootScope(), scriptID);
        }

        private void InitSession()
        {
            _session = new Session("", Parser);
            _interpreter = new Interpreter.Interpreter(_session);
        }

        public void ResetSession(string workingDir)
        {
            Session.Reset(workingDir);
            _rootScope = null;
        }

        public Scope GetRootScope() 
        {
            return RootScope;
        }

        public string GetWorkingDir() {
            return Session.WorkingDir;
        }
    }
}
using ScriptingLanguage.Interpreter;
using ScriptingLanguage.Tokenizer;
using UnityEngine;

namespace ScriptingLanguage.VisualScripting
{
    public class VisualScriptingSession : MonoBehaviour
    {
        public TextAsset ParserTable;

        private Parser.Parser _parser;

        private Session _session;
        private Interpreter.Interpreter _interpreter;

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
            if (_interpreter == null) {
                InitSession();
            }
            var scriptID = new ScriptId { Script = script };
            _interpreter.RunScript(_session.GetWorkingScope(), scriptID);
        }

        private void InitSession()
        {
            _session = new Session("", Parser);
            _interpreter = new Interpreter.Interpreter(_session);
        }

        public void ResetSession(string workingDir)
        {
            if (_session == null) {
                InitSession();
            }
            _session.Reset(workingDir);
        }
    }
}
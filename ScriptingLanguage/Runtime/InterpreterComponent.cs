using ScriptingLaunguage.Interpreter;
using UnityEngine;

namespace ScriptingLaunguage
{
    public class InterpreterComponent : MonoBehaviour
    {
        public string ScriptsFolder = "";
        [TextArea(30, 50)]
        public string Output;
        public string Command;
        
        public Session Session;
        public Interpreter.Interpreter Interpreter;

    }
}

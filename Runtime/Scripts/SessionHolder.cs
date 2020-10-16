using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage
{
    public class SessionHolder : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        private TextAsset ParserTable;

        REPL.REPL repl;
        REPL.REPL REPL
        {
            get
            {
                if (repl == null) {
                    var parserData = Resources.FindObjectsOfTypeAll<ParserData>().FirstOrDefault();
                    var pt = Parser.ParserTable.Deserialize(parserData.ParserTable.bytes);
                    repl = new REPL.REPL(pt);
                }
                return repl;
            }
        }

        public IEnumerable<string> RunCommand(string command)
        {
            if (string.IsNullOrEmpty(command)) {
                yield return command;
            }

            var res = REPL.HandleCommand(command);
            foreach (var str in res)
            {
                yield return str;
            }
        }
    }
}

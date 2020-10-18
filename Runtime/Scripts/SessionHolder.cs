using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage
{
    public class SessionHolder : MonoBehaviour
    {
        [SerializeField]
        private ParserData ParserData;

        REPL.REPL repl;
        REPL.REPL REPL
        {
            get
            {
                if (repl == null) {
                    var pt = ParserData.GetParserTable();
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

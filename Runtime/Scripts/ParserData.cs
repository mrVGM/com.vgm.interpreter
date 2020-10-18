using ScriptingLanguage.Parser;
using UnityEngine;

namespace ScriptingLanguage
{
    public class ParserData : ScriptableObject
    {
        [SerializeField]
        private TextAsset ParserTable;

        public ParserTable GetParserTable()
        {
            return Parser.ParserTable.Deserialize(ParserTable.bytes);
        }
    }
}

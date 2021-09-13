using ScriptingLanguage;
using ScriptingLanguage.Parser;
using ScriptingLanguage.Tokenizer;
using UnityEngine;

public class Test : MonoBehaviour
{
    public TextAsset ParserTable;
    ParserTable _parserTable;

    public TextAsset Markup;
    public ParserTable PT 
    {
        get
        {
            if (_parserTable == null) {
                _parserTable = ScriptingLanguage.Parser.ParserTable.Deserialize(ParserTable.bytes);
            }
            return _parserTable;
        }
    }
    Parser _parser = null;
    public Parser Parser
    {
        get
        {
            if (_parser == null) {

                _parser = new Parser { ParserTable = PT };
            }
            return _parser;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        var scriptId = new ScriptId { Filename = Markup.name, Script = Markup.text };
        var tokens = Utils.TokenizeText(scriptId, new SimpleToken { Name = "Terminal" });
        tokens = CombinedTokenizer.DefaultTokenizer.Tokenize(tokens);
        var tree = Parser.ParseProgram(tokens);
        bool t = true;
    }
}

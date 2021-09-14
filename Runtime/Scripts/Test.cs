using ScriptingLanguage;
using ScriptingLanguage.Markup;
using ScriptingLanguage.Markup.Layout;
using ScriptingLanguage.Parser;
using ScriptingLanguage.Tokenizer;
using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour
{
    public TextAsset ParserTable;
    ParserTable _parserTable;
    public UIBuildingContext BuildingContext;

    public TextAsset Markup;
    public TextAsset Markup2;
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

    public void OnCreated(UIElement element)
    {
        var scriptId = new ScriptId { Filename = Markup2.name, Script = Markup2.text };
        var tokens = Utils.TokenizeText(scriptId, new SimpleToken { Name = "Terminal" });
        tokens = CombinedTokenizer.DefaultTokenizer.Tokenize(tokens);
        var tree = Parser.ParseProgram(tokens);
        MarkupInterpreter.ValidateParserTree(tree);

        var interpreter = new MarkupInterpreter();
        var elements = interpreter.SetupUnityElements(tree, BuildingContext, element.UnityElement);
        var topLevelElements = elements.Where(x => x.ElementLevel == 0).ToList();
        foreach (var elem in topLevelElements) {
            elem.Parent = element;
        }
    }
    public void BuildUI()
    {
        var scriptId = new ScriptId { Filename = Markup.name, Script = Markup.text };
        var tokens = Utils.TokenizeText(scriptId, new SimpleToken { Name = "Terminal" });
        tokens = CombinedTokenizer.DefaultTokenizer.Tokenize(tokens);
        var tree = Parser.ParseProgram(tokens);
        MarkupInterpreter.ValidateParserTree(tree);

        var interpreter = new MarkupInterpreter();
        var elements = interpreter.SetupUnityElements(tree, BuildingContext, GetComponent<RectTransform>());

        var layouthandler = new LayoutSizeHandler();
        var grouphandler = new GroupHandler();
        layouthandler.HandleLayout(elements);
        grouphandler.HandleLayout(elements);
    }
}

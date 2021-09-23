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

    public void OnCreated(UIElement element, UIBuildingContext buildingContext)
    {
        var scriptId = new ScriptId { Filename = Markup2.name, Script = Markup2.text };
        var tokens = Utils.TokenizeText(scriptId, new SimpleToken { Name = "Terminal" });
        tokens = CombinedTokenizer.DefaultTokenizer.Tokenize(tokens);
        var tree = Parser.ParseProgram(tokens);
        MarkupInterpreter.ValidateParserTree(tree);

        var interpreter = new MarkupInterpreter();
        var elements = interpreter.SetupUnityElements(tree, buildingContext, element.UnityElement);
        var topLevelElements = elements.Where(x => x.ElementLevel == 0).ToList();
        foreach (var elem in topLevelElements) {
            elem.Parent = element;
        }
    }

    public ProgramNode ParseLayout(TextAsset markup)
    {
        var scriptId = new ScriptId { Filename = markup.name, Script = markup.text };
        var tokens = Utils.TokenizeText(scriptId, new SimpleToken { Name = "Terminal" });
        tokens = CombinedTokenizer.DefaultTokenizer.Tokenize(tokens);
        var tree = Parser.ParseProgram(tokens);
        MarkupInterpreter.ValidateParserTree(tree);
        return tree;
    }

    public void BuildUI(TextAsset markup, RectTransform root, UIBuildingContext buildingContext)
    {
        var scriptId = new ScriptId { Filename = markup.name, Script = markup.text };
        var tokens = Utils.TokenizeText(scriptId, new SimpleToken { Name = "Terminal" });
        tokens = CombinedTokenizer.DefaultTokenizer.Tokenize(tokens);
        var tree = Parser.ParseProgram(tokens);
        MarkupInterpreter.ValidateParserTree(tree);

        var interpreter = new MarkupInterpreter();
        var elements = interpreter.SetupUnityElements(tree, buildingContext, root);

        var layoutSizeHandler = new LayoutSizeHandler();
        var grouphandler = new GroupHandler();
        var texthandler = new TextHandler();
        var imagehandler = new ImageHandler();

        texthandler.HandleLayout(elements);
        imagehandler.HandleLayout(elements);
        layoutSizeHandler.HandleLayout(elements);
        grouphandler.HandleLayout(elements);
    }
}

using ScriptingLanguage.Parser;
using System;
using System.Collections.Generic;

namespace ScriptingLanguage.Markup
{
    public static class MarkupUtils
    {
        public static IEnumerable<KeyValuePair<string, string>> GetTagParameters(UIElement element)
        {
            var programNode = element.ProgramNode;
            if (programNode.Token.Name != "Tag") {
                throw new InvalidOperationException();
            }
            var openingTag = programNode.Children[0]; 
            if (programNode.MatchChildren("<", "Name", ">")) {
                yield break;
            }
            var tagParams = openingTag.Children[2];
            IEnumerable<ProgramNode> getTagParams(ProgramNode parameters)
            {
                if (parameters.MatchChildren("TagParameter")) {
                    yield return parameters.Children[0];
                    yield break;
                }
                if (parameters.MatchChildren("TagParameters", "TagParameter")) {
                    var nodes = getTagParams(parameters.Children[0]);
                    foreach (var node in nodes) {
                        yield return node;
                    }
                    yield return parameters.Children[1];
                }
            }

            var tagParamNodes = getTagParams(tagParams);
            foreach (var p in tagParamNodes) {
                yield return new KeyValuePair<string, string>(p.Children[0].Token.Data as string, p.Children[2].Token.Data as string);
            }
        }

        public static string GetTagName(UIElement element)
        {
            if (element.ProgramNode.Token.Name != "Tag") {
                throw new InvalidOperationException();
            }
            var openingTag = element.ProgramNode.Children[0];
            return openingTag.Children[1].Token.Data as string;
        }
    }
}

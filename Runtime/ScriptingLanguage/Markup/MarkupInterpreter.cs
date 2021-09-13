using ScriptingLanguage.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage.Markup
{
    public class MarkupInterpreter
    {
        public static void ValidateParserTree(ProgramNode node)
        {
            if (node.Token.Name != "Tag")
            {
                foreach (var child in node.Children)
                {
                    ValidateParserTree(child);
                }
                return;
            }

            void validateOpeningClosingTags(ProgramNode openingTag, ProgramNode closingTag)
            {
                string opening = openingTag.Children[1].Token.Data as string;
                string closing = closingTag.Children[2].Token.Data as string;
                if (opening != closing) {
                    throw new Exception("Tags don't match");
                }
            }

            if (node.MatchChildren("OpeningTag", "ClosingTag")) {
                validateOpeningClosingTags(node.Children[0], node.Children[1]);
                return;
            }

            if (node.MatchChildren("OpeningTag", "TagGroup", "ClosingTag")) {
                validateOpeningClosingTags(node.Children[0], node.Children[2]);
                ValidateParserTree(node.Children[1]);
                return;
            }
        }

        public IEnumerable<UIElement> SetupUnityElements(ProgramNode node, RectTransform root)
        {
            var elementList = new List<UIElement>();
            UIElement convertNode(ProgramNode node, RectTransform transform)
            {
                var uiElement = new UIElement { ProgramNode = node };
                if (node.Token.Name == "Tag") {
                    var go = new GameObject($"{node.Children[0].Children[1].Token.Data}");
                    uiElement.UnityElement = go.AddComponent<RectTransform>();
                    uiElement.UnityElement.SetParent(transform);
                }
                foreach (var child in uiElement.ProgramNode.Children) {
                    var tmp = convertNode(child, uiElement.UnityElement != null ? uiElement.UnityElement : transform);
                    tmp.Parent = uiElement;
                }
                elementList.Add(uiElement);
                return uiElement;
            }

            int nodeLevel(UIElement element)
            {
                int level = 0;
                while (element.Parent != null)
                {
                    element = element.Parent;
                    if (element.ProgramNode.Token.Name == "Tag") {
                        ++level;
                    }
                }
                return level;
            }

            convertNode(node, root);

            var tags = elementList.OrderByDescending(nodeLevel);
            return tags;
        }
    }
}

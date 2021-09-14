using ScriptingLanguage.Markup.Layout;
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

        private void OnElementCreated(UIElement element, UIBuildingContext buildingContext)
        {
            var allParams = MarkupUtils.GetTagParameters(element);
            foreach (var tagParam in allParams) {
                if (tagParam.Key == "onCreated") {
                    var funcName = tagParam.Value;
                    var func = buildingContext.NamedProperties.FirstOrDefault(x => x.Name == funcName);
                    if (func.CallBack != null) {
                        func.CallBack.Invoke(element);
                        break;
                    }
                }
            }
        }

        public IEnumerable<UIElement> SetupUnityElements(ProgramNode node, UIBuildingContext buildingContext, RectTransform root)
        {
            UIElement convertNode(ProgramNode node, RectTransform transform, UIElementFactory factory)
            {
                var uiElement = factory.CreateElement(node);
                if (node.Token.Name == "Tag") {
                    var go = new GameObject($"{node.Children[0].Children[1].Token.Data}");
                    uiElement.UnityElement = go.AddComponent<RectTransform>();
                    uiElement.UnityElement.SetParent(transform);
                    OnElementCreated(uiElement, buildingContext);
                }
                foreach (var child in uiElement.ProgramNode.Children) {
                    var tmp = convertNode(child, uiElement.UnityElement != null ? uiElement.UnityElement : transform, factory);
                    tmp.Parent = uiElement;
                }
                return uiElement;
            }
            List<UIElement> elementList = new List<UIElement>();
            using (var factory = new UIElementFactory()) {
                convertNode(node, root, factory);
                elementList = factory.ElementsCreated.Where(x => x.ProgramNode.Token.Name == "Tag").ToList(); 
            }
            foreach (var tagElement in elementList) {
                var parent = tagElement.Parent;
                while (parent != null && !elementList.Contains(parent)) {
                    parent = parent.Parent;
                }
                tagElement.Parent = parent;
            }

            return elementList;
        }
    }
}

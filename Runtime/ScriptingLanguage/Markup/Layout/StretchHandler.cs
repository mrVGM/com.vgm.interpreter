using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage.Markup.Layout
{
    public class StretchHandler : ILayoutHandler
    {
        public void HandleLayout(IEnumerable<UIElement> elements)
        {
            var ordered = elements.OrderByDescending(x => x.ElementLevel);
            foreach (var element in ordered) {
                var stretch = MarkupUtils.GetTagParameters(element).FirstOrDefault(x => x.Key == "stretch");
                if (stretch.Equals(default(KeyValuePair<string, string>))) {
                    continue;
                }

                var rect = element.UnityElement;
                if (stretch.Value == "true") {
                    var parent = element.Parent;
                    string tagName = MarkupUtils.GetTagName(parent);
                    if (tagName == "div") {
                        float height = rect.rect.height;
                        rect.anchorMin = Vector2.up;
                        rect.anchorMax = Vector2.one;
                        rect.offsetMin = height * Vector2.down;
                        rect.offsetMax = Vector2.zero;
                    }
                    else if (tagName == "span") {
                        float width = rect.rect.width;
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.up;
                        rect.offsetMin = Vector2.zero;
                        rect.offsetMax = width * Vector2.right;
                    }
                    else {
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.offsetMin = Vector2.zero;
                        rect.offsetMax = Vector2.zero;
                    }
                }
                else {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                }
            }
        }
    }
}

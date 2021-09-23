using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage.Markup.Layout
{
    public class AlignHandler : ILayoutHandler
    {
        public void HandleLayout(IEnumerable<UIElement> elements)
        {
            string getAlignment(UIElement element)
            {
                var parameters = MarkupUtils.GetTagParameters(element);
                foreach (var param in parameters) {
                    if (param.Key == "align") {
                        return param.Value;
                    }
                }

                return null;
            }
            
            var aligned = elements.Select(x => new KeyValuePair<UIElement, string>(
                    x,
                    getAlignment(x)
            )).Where(x => x.Value != null)
                .OrderByDescending(x => x.Key.ElementLevel);

            foreach (var elem in aligned) {
                var parent = elem.Key.Parent;
                var parentTag = MarkupUtils.GetTagName(parent);
                if (parentTag == "div" || parentTag == "span") {
                    continue;
                }

                string alignment = elem.Value;
                var rect = elem.Key.UnityElement;
                if (alignment == "left") {
                    rect.anchorMin = 0.5f * Vector2.up;
                    rect.anchorMax = 0.5f * Vector2.up;
                    rect.pivot = 0.5f * Vector2.up;
                    rect.anchoredPosition = Vector2.zero;
                }
                if (alignment == "right") {
                    rect.anchorMin = 0.5f * Vector2.up + Vector2.right;
                    rect.anchorMax = 0.5f * Vector2.up + Vector2.right;
                    rect.pivot = 0.5f * Vector2.up + Vector2.right;
                    rect.anchoredPosition = Vector2.zero;
                }
                if (alignment == "up") {
                    rect.anchorMin = Vector2.up + 0.5f * Vector2.right;
                    rect.anchorMax = Vector2.up + 0.5f * Vector2.right;
                    rect.pivot = Vector2.up + 0.5f * Vector2.right;
                    rect.anchoredPosition = Vector2.zero;
                }
                if (alignment == "down") {
                    rect.anchorMin = 0.5f * Vector2.right;
                    rect.anchorMax = 0.5f * Vector2.right;
                    rect.pivot = 0.5f * Vector2.right;
                    rect.anchoredPosition = Vector2.zero;
                }
                if (alignment == "center") {
                    rect.anchorMin = 0.5f * (Vector2.up + Vector2.right);
                    rect.anchorMax = 0.5f * (Vector2.up + Vector2.right);
                    rect.pivot = 0.5f * (Vector2.up + Vector2.right);
                    rect.anchoredPosition = Vector2.zero;
                }
            }
        }
    }
}

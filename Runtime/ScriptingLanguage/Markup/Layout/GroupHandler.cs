using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage.Markup.Layout
{
    public class GroupHandler
    {
        public void HandleLayout(IEnumerable<UIElement> elements)
        {
            var groupTags = elements.Where(x => MarkupUtils.GetTagName(x) == "div" || MarkupUtils.GetTagName(x) == "span");
            Dictionary<UIElement, List<UIElement>> childrenMap = new Dictionary<UIElement, List<UIElement>>();
            foreach (var groupTag in groupTags) {
                childrenMap[groupTag] = new List<UIElement>();
            }

            foreach (var elem in elements) {
                if (elem.Parent == null) {
                    continue;
                }
                List<UIElement> childrenList = null;
                if (childrenMap.TryGetValue(elem.Parent, out childrenList)) {
                    childrenList.Add(elem);
                }
            }

            groupTags = groupTags.OrderByDescending(x => x.ElementLevel);
            foreach (var groupTag in groupTags) {
                if (MarkupUtils.GetTagName(groupTag) == "div") {
                    var anchorPos = groupTag.UnityElement.rect.max;
                    var curSize = Vector2.zero;

                    foreach (var child in childrenMap[groupTag]) {
                        child.UnityElement.anchorMin = Vector2.up;
                        child.UnityElement.anchorMax = Vector2.up;
                        child.UnityElement.pivot = Vector2.up;
                        child.UnityElement.anchoredPosition = curSize.y * Vector2.down;
                        curSize += child.UnityElement.sizeDelta.y * Vector2.up;
                        curSize.x = Mathf.Max(curSize.x, child.UnityElement.sizeDelta.x);
                        groupTag.UnityElement.sizeDelta = curSize;
                    }
                }
                if (MarkupUtils.GetTagName(groupTag) == "span")
                {
                    var anchorPos = groupTag.UnityElement.rect.max;
                    var curSize = Vector2.zero;

                    foreach (var child in childrenMap[groupTag])
                    {
                        child.UnityElement.anchorMin = Vector2.up;
                        child.UnityElement.anchorMax = Vector2.up;
                        child.UnityElement.pivot = Vector2.up;
                        child.UnityElement.anchoredPosition = curSize.x * Vector2.right;
                        curSize += child.UnityElement.sizeDelta.x * Vector2.right;
                        curSize.y = Mathf.Max(curSize.y, child.UnityElement.sizeDelta.y);
                        groupTag.UnityElement.sizeDelta = curSize;
                    }
                }
            }
        }
    }
}

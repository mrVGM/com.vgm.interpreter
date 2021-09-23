using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage.Markup.Layout
{
    public class GroupHandler : ILayoutHandler
    {
        public void HandleLayout(IEnumerable<UIElement> elements)
        {
            IEnumerable<UIElement> getChildrenOf(UIElement element)
            {
                return elements.Where(x => x.Parent == element);
            }

            bool isGroupTag(UIElement element)
            {
                string tagName = MarkupUtils.GetTagName(element);
                return tagName == "div" || tagName == "span";
            }

            var groupTags = elements.Where(isGroupTag).OrderByDescending(x => x.ElementLevel);

            foreach (var groupTag in groupTags) {
                Vector2 curSize = Vector2.zero;
                var children = getChildrenOf(groupTag);
                string tagName = MarkupUtils.GetTagName(groupTag);
                if (tagName == "div") {
                    foreach (var child in children) {
                        child.UnityElement.pivot = Vector2.up;
                        child.UnityElement.anchoredPosition = curSize.y * Vector2.down;
                        curSize.y += child.UnityElement.rect.height;
                        curSize.x = Mathf.Max(curSize.x, child.UnityElement.rect.width);
                    }
                }
                if (tagName == "span") {
                    foreach (var child in children) {
                        child.UnityElement.pivot = Vector2.up;
                        child.UnityElement.anchoredPosition = curSize.x * Vector2.right;
                        curSize.x += child.UnityElement.rect.width;
                        curSize.y = Mathf.Max(curSize.y, child.UnityElement.rect.height);
                    }
                }

                groupTag.UnityElement.sizeDelta = curSize;
            }
        }

        public void HandleLayout1(IEnumerable<UIElement> elements)
        {
            var groupTags = elements;
            Dictionary<UIElement, List<UIElement>> childrenMap = new Dictionary<UIElement, List<UIElement>>();
            foreach (var element in elements) {
                childrenMap[element] = new List<UIElement>();
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

            bool isStretched(UIElement uiElement)
            {
                var parameters = MarkupUtils.GetTagParameters(uiElement);
                var stretch = parameters.FirstOrDefault(x => x.Key == "stretch");
                return stretch.Value == "true";
            }

            groupTags = groupTags.OrderByDescending(x => x.ElementLevel);
            foreach (var groupTag in groupTags) {
                if (MarkupUtils.GetTagName(groupTag) == "div") {
                    var curSize = Vector2.zero;

                    foreach (var child in childrenMap[groupTag]) {
                        if (isStretched(child)) {
                            child.UnityElement.anchorMin = Vector2.zero;
                            child.UnityElement.anchorMax = Vector2.right;
                            float height = child.UnityElement.sizeDelta.y;
                            child.UnityElement.offsetMin = height * Vector2.down;
                            child.UnityElement.offsetMax = Vector2.zero;
                        }
                        else {
                            child.UnityElement.anchorMin = Vector2.up;
                            child.UnityElement.anchorMax = Vector2.up;
                        }

                        child.UnityElement.pivot = Vector2.up;
                        child.UnityElement.anchoredPosition = curSize.y * Vector2.down;
                        curSize += child.UnityElement.sizeDelta.y * Vector2.up;
                        curSize.x = Mathf.Max(curSize.x, child.UnityElement.sizeDelta.x);
                        groupTag.UnityElement.sizeDelta = curSize;
                    }
                }
                if (MarkupUtils.GetTagName(groupTag) == "span")
                {
                    var curSize = Vector2.zero;

                    foreach (var child in childrenMap[groupTag]) {
                        if (isStretched(child)) {
                            child.UnityElement.anchorMin = Vector2.zero;
                            child.UnityElement.anchorMax = Vector2.up;
                            float width = child.UnityElement.sizeDelta.x;
                            child.UnityElement.offsetMin = Vector2.zero;
                            child.UnityElement.offsetMax = width * Vector2.right;
                        }
                        else {
                            child.UnityElement.anchorMin = Vector2.up;
                            child.UnityElement.anchorMax = Vector2.up;
                        }

                        child.UnityElement.pivot = Vector2.up;
                        child.UnityElement.anchoredPosition = curSize.x * Vector2.right;
                        curSize += child.UnityElement.sizeDelta.x * Vector2.right;
                        curSize.y = Mathf.Max(curSize.y, child.UnityElement.sizeDelta.y);
                        groupTag.UnityElement.sizeDelta = curSize;
                    }
                }

                var parameters = MarkupUtils.GetTagParameters(groupTag);
                var wrap = parameters.FirstOrDefault(x => x.Key == "wrap");
                if (!wrap.Equals(default(KeyValuePair<string, string>)) && wrap.Value == "true") {
                    var children = childrenMap[groupTag].Where(x => !isStretched(x));
                    float minX = children.Min(x => (x.UnityElement.anchorMin + x.UnityElement.offsetMin).x);
                    float maxX = children.Max(x => (x.UnityElement.anchorMax + x.UnityElement.offsetMax).x);
                    
                    float minY = children.Min(x => (x.UnityElement.anchorMin + x.UnityElement.offsetMin).y);
                    float maxY = children.Max(x => (x.UnityElement.anchorMax + x.UnityElement.offsetMax).y);

                    groupTag.UnityElement.sizeDelta = new Vector2(maxX - minX, maxY - minY);
                }

                if (isStretched(groupTag)) {
                    groupTag.UnityElement.anchorMin = Vector2.zero;
                    groupTag.UnityElement.anchorMax = Vector2.one;
                    groupTag.UnityElement.offsetMin = Vector2.zero;
                    groupTag.UnityElement.offsetMax = Vector2.zero;
                }
            }
        }
    }
}

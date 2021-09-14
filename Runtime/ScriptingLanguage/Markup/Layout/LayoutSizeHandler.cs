using System.Collections.Generic;
using System.Linq;

namespace ScriptingLanguage.Markup.Layout
{
    public class LayoutSizeHandler
    {
        public void HandleLayout(IEnumerable<UIElement> elements)
        {
            var ordered = elements.OrderByDescending(x => x.ElementLevel);
            foreach (var elem in ordered) 
            {
                var elementParams = MarkupUtils.GetTagParameters(elem);
                var width = elementParams.FirstOrDefault(x => x.Key == "width");
                var height = elementParams.FirstOrDefault(x => x.Key == "height");
                if (!width.Equals(default(KeyValuePair<string, string>))) {
                    int w = int.Parse(width.Value);
                    var sizeDelta = elem.UnityElement.sizeDelta;
                    sizeDelta.x = w;
                    elem.UnityElement.sizeDelta = sizeDelta;
                }
                if (!height.Equals(default(KeyValuePair<string, string>)))
                {
                    int h = int.Parse(height.Value);
                    var sizeDelta = elem.UnityElement.sizeDelta;
                    sizeDelta.y = h;
                    elem.UnityElement.sizeDelta = sizeDelta;
                }
            }
        }
    }
}

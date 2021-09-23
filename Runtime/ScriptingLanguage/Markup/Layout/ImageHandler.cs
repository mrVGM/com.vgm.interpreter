using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using static ScriptingLanguage.Markup.UIBuildingContext;

namespace ScriptingLanguage.Markup.Layout
{
    public class ImageHandler : ILayoutHandler
    {
        public void HandleLayout(IEnumerable<UIElement> elements)
        {
            var imageTags = elements.Where(x => MarkupUtils.GetTagName(x) == "img");
            foreach (var imageTag in imageTags) {
                var context = imageTag.Context;
                var imageComponent = imageTag.UnityElement.gameObject.AddComponent<Image>();
                var parameters = MarkupUtils.GetTagParameters(imageTag);
                var srcImage = parameters.FirstOrDefault(x => x.Key == "src");
                var colorParam = parameters.FirstOrDefault(x => x.Key == "color");
                if (!srcImage.Equals(default(KeyValuePair<string, string>)))
                {
                    var image = context.NamedProperties.FirstOrDefault(x => x.Name == srcImage.Value);
                    if (!image.Equals(default(NamedProperty)))
                    {
                        imageComponent.sprite = image.SpriteValue;
                    }
                }
                if (!colorParam.Equals(default(KeyValuePair<string, string>)))
                {
                    var color = context.NamedProperties.FirstOrDefault(x => x.Name == colorParam.Value);
                    if (!color.Equals(default(NamedProperty)))
                    {
                        imageComponent.color = color.Color;
                    }
                }
            }
        }
    }
}

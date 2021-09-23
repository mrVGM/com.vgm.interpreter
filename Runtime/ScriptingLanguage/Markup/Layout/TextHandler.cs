using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ScriptingLanguage.Markup.UIBuildingContext;

namespace ScriptingLanguage.Markup.Layout
{
    public class TextHandler : ILayoutHandler
    {
        public void HandleLayout(IEnumerable<UIElement> elements)
        {
            var textTags = elements.Where(x => MarkupUtils.GetTagName(x) == "txt");
            foreach (var textTag in textTags) {
                var context = textTag.Context;
                var textComponent = textTag.UnityElement.gameObject.AddComponent<Text>();
                
                var parameters = MarkupUtils.GetTagParameters(textTag);

                var fontParam = parameters.FirstOrDefault(x => x.Key == "font");
                if (!fontParam.Equals(default(KeyValuePair<string, string>))) {
                    var font = context.NamedProperties.FirstOrDefault(x => x.Name == fontParam.Value);
                    if (!font.Equals(default(NamedProperty))) {
                        textComponent.font = font.Font;
                    }
                }

                var fixedText = parameters.FirstOrDefault(x => x.Key == "fixed");
                var srcText = parameters.FirstOrDefault(x => x.Key == "src");
                if (!fixedText.Equals(default(KeyValuePair<string, string>))) {
                    textComponent.text = fixedText.Value;
                }
                if (!srcText.Equals(default(KeyValuePair<string, string>))) {
                    var text = context.NamedProperties.FirstOrDefault(x => x.Name == srcText.Value);
                    if (!text.Equals(default(NamedProperty))) {
                        textComponent.text = text.StringValue;
                    }
                }

                textTag.UnityElement.sizeDelta = new Vector2(textComponent.preferredWidth, textComponent.preferredHeight);
            }
        }
    }
}

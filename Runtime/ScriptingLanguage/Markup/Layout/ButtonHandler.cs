using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using static ScriptingLanguage.Markup.UIBuildingContext;

namespace ScriptingLanguage.Markup.Layout
{
    public class ButtonHandler : ILayoutHandler
    {
        public void HandleLayout(IEnumerable<UIElement> elements)
        {
            var buttonTags = elements.Where(x => MarkupUtils.GetTagName(x) == "btn");
            foreach (var button in buttonTags) {
                var context = button.Context;
                var buttonComponent = button.UnityElement.gameObject.AddComponent<Button>();
                var parameters = MarkupUtils.GetTagParameters(button);
                var onClickParam = parameters.FirstOrDefault(x => x.Key == "onClick");
                
                if (!onClickParam.Equals(default(KeyValuePair<string, string>))) {
                    var onClick = context.NamedProperties.FirstOrDefault(x => x.Name == onClickParam.Value);
                    if (!onClick.Equals(default(NamedProperty))) {
                        buttonComponent.onClick.RemoveAllListeners();
                        buttonComponent.onClick.AddListener(() => onClick.CallBack.Invoke(button));
                    }
                }
            }
        }
    }
}

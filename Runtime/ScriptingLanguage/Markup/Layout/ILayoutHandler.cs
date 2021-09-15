using System.Collections.Generic;

namespace ScriptingLanguage.Markup.Layout
{
    public interface ILayoutHandler
    {
        void HandleLayout(IEnumerable<UIElement> elements, UIBuildingContext context);
    }
}

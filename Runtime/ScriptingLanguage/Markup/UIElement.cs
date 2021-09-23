using ScriptingLanguage.Parser;
using UnityEngine;

namespace ScriptingLanguage.Markup
{
    public interface UIElement
    {
        UIElement Parent { get; set; }
        ProgramNode ProgramNode { get; }
        RectTransform UnityElement { get; set; }
        UIBuildingContext Context { get; set; }
        int ElementLevel { get; }
    }
}

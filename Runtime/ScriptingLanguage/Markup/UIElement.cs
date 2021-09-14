using ScriptingLanguage.Parser;
using UnityEngine;

namespace ScriptingLanguage.Markup
{
    public interface UIElement
    {
        UIElement Parent { get; set; }
        ProgramNode ProgramNode { get; }
        RectTransform UnityElement { get; set; }
        int ElementLevel { get; }
    }
}

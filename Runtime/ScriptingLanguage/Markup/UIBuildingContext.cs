using System;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptingLanguage.Markup
{
    [Serializable]
    public struct UIBuildingContext
    {
        [Serializable]
        public struct NamedProperty
        {
            public string Name;
            public int IntValue;
            public string StringValue;
            public Sprite SpriteValue;
            public UIElementCallback CallBack;
        }
        [Serializable]
        public class UIElementCallback : UnityEvent<UIElement>
        {
        }

        public NamedProperty[] NamedProperties;
    }
}

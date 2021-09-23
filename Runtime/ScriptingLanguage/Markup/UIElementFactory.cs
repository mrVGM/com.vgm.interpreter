using ScriptingLanguage.Parser;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptingLanguage.Markup
{
    public class UIElementFactory : IDisposable
    {
        private class UIElement : Markup.UIElement
        {
            public Markup.UIElement Parent { get; set; }
            public ProgramNode ProgramNode { get; set; }
            public RectTransform UnityElement { get; set; }
            public UIBuildingContext Context { get; set; }

            public int ElementLevel
            {
                get
                {
                    int level = 0;
                    var cur = Parent;
                    while (cur != null) {
                        if (cur.ProgramNode.Token.Name == "Tag") {
                            ++level;
                        }
                        cur = cur.Parent;
                    }
                    return level;
                }
            }
        }

        private static Stack<UIElementFactory> _factories = new Stack<UIElementFactory>();
        private List<Markup.UIElement> _elementsCreated = new List<Markup.UIElement>();

        public IEnumerable<Markup.UIElement> ElementsCreated => _elementsCreated;

        public UIElementFactory() 
        {
            _factories.Push(this);
        }

        public void Dispose()
        {
            if (_factories.Peek() != this) {
                throw new InvalidOperationException();
            }

            _factories.Pop();
            if (_factories.Count > 0) {
                var next = _factories.Peek();
                if (next != null) {
                    next._elementsCreated.AddRange(_elementsCreated);
                }
                _elementsCreated.Clear();
            }
        }

        public Markup.UIElement CreateElement(ProgramNode node)
        {
            var elem = new UIElement { ProgramNode = node};
            _elementsCreated.Add(elem);
            return elem;
        }
    }
}

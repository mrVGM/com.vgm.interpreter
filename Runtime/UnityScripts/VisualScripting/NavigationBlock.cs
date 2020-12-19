using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class NavigationBlock : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private Frame _frame => GetComponentInParent<Frame>();
        private Vector2 _pos;
        private Vector2 _nodesAnchorPosition;
        public void OnBeginDrag(PointerEventData eventData)
        {
            _pos = eventData.position;
            _nodesAnchorPosition = _frame.NodesAnchor.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _frame.NodesAnchor.anchoredPosition = _nodesAnchorPosition + eventData.position - _pos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IEnumerable<Transform> childTransforms() {
                int childCount = _frame.NodesAnchor.childCount;
                for (int i = 0; i < childCount; ++i) {
                    var curChild = _frame.NodesAnchor.GetChild(i);
                    yield return curChild;
                }
            }

            var positions = childTransforms().Select(x => x.position).ToArray();

            _frame.NodesAnchor.anchoredPosition = Vector2.zero;
            Enumerable.Zip(childTransforms(), positions, (tr, p) => {
                tr.position = p;
                return p;
            }).ToArray();
        }
    }
}

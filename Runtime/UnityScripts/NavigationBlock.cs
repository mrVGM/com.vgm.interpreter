using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class NavigationBlock : MonoBehaviour, IDragHandler, IBeginDragHandler
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
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class NavigationElement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private Frame _frame => GetComponentInParent<Frame>();
        private Vector2 _startingPosition;
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Middle) {
                return;
            }
            _startingPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Middle)
            {
                return;
            }
            Vector2 offset = eventData.position - _startingPosition;
            _frame.NodesContainer.anchoredPosition = offset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Middle)
            {
                return;
            }
            Dictionary<Transform, Vector3> children = new Dictionary<Transform, Vector3>();
            for (int i = 0; i < _frame.NodesContainer.transform.childCount; ++i) {
                var cur = _frame.NodesContainer.transform.GetChild(i);
                children[cur] = cur.position;
            }

            _frame.NodesContainer.anchoredPosition = Vector2.zero;
            foreach (var item in children) {
                item.Key.transform.position = item.Value;
            }
        }
    }
}
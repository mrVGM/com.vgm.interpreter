using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class MoveHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private Vector2 _startingPosition;
        private Vector3 _transformInitialPosition;
        public NodeComponent NodeComponent => GetComponentInParent<NodeComponent>();
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) {
                return;
            }
            _startingPosition = eventData.position;
            _transformInitialPosition = NodeComponent.transform.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) {
                return;
            }
            Vector2 offset = eventData.position - _startingPosition;
            NodeComponent.transform.position = _transformInitialPosition + new Vector3(offset.x, offset.y, 0.0f);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) {
                return;
            }
            if (eventData.pointerEnter == null) {
                return;
            }
            var trashCan = eventData.pointerEnter.GetComponent<TrashCan>();
            if (trashCan == null) {
                return;
            }
            NodeComponent.DeInit();
            Destroy(NodeComponent.gameObject);
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class MoveHandle : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        public RectTransform TransformToMove;

        public void OnDrag(PointerEventData eventData)
        {
            TransformToMove.anchoredPosition = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var knobOwner = TransformToMove.GetComponent<IKnobOwner>();
            if (knobOwner == null) {
                return;
            }

            var hoveredGO = eventData.pointerEnter;
            if (hoveredGO == null) {
                return;
            }

            var trashCan = hoveredGO.GetComponent<TrashCan>();
            if (trashCan == null) {
                return;
            }

            NodeUtils.Destroy(knobOwner);
        }
    }
}

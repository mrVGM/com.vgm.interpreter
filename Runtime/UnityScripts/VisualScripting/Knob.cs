using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class Knob : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        public List<LinkEndpoint> LinkEndpoints;

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerEnter == null) {
                return;
            }
            var destinationKnob = eventData.pointerEnter.GetComponent<Knob>();
            if (destinationKnob == null) {
                return;
            }

            var frame = GetComponentInParent<Frame>();
            var link = Instantiate(frame.LinkPrefab, frame.transform);
            link.LinkKnobs(this, destinationKnob);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
        }
    }
}

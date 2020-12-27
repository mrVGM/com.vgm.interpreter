using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class Link : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        public RectTransform Endpoint1;
        public RectTransform Endpoint2;

        private EndpointComponent _endpointComponent1 => Endpoint1.GetComponentInParent<EndpointComponent>();
        private EndpointComponent _endpointComponent2 => Endpoint2.GetComponentInParent<EndpointComponent>();

        public void OnBeginDrag(PointerEventData eventData)
        {
            EndpointComponent.UnLink(_endpointComponent1, _endpointComponent2);
            EndpointComponent.VisuallyUnlink(_endpointComponent1, _endpointComponent2);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void Update()
        {
            if (Endpoint1 == null || Endpoint2 == null) {
                return;
            }

            Vector3 offset = Endpoint2.position - Endpoint1.position;
            transform.position = Endpoint1.position;
            var angle = Vector3.SignedAngle(Vector3.right, offset, Vector3.forward);
            transform.rotation = Quaternion.Euler(0, 0, angle);
            
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(offset.magnitude, rectTransform.sizeDelta.y);
        }
    }
}
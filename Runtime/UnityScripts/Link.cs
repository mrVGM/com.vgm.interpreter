using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class Link : MonoBehaviour, IDragHandler
    {
        public LinkEndpoint LinkEndpoint1;
        public LinkEndpoint LinkEndpoint2;

        public void LinkKnobs(Knob knob1, Knob knob2)
        {
            knob1.LinkEndpoints.Add(LinkEndpoint1);
            LinkEndpoint1.Knob = knob1;
            knob2.LinkEndpoints.Add(LinkEndpoint2);
            LinkEndpoint2.Knob = knob2;
            UpdateTransform();
        }
        
        public void UnLinkKnobs()
        {
            LinkEndpoint1.Knob.LinkEndpoints.Remove(LinkEndpoint1);
            LinkEndpoint2.Knob.LinkEndpoints.Remove(LinkEndpoint2);
        }

        private void UpdateTransform()
        {
            Vector3 knob1Pos = LinkEndpoint1.Knob.transform.position;
            Vector3 offset = LinkEndpoint2.Knob.transform.position - knob1Pos;
            
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.transform.position = knob1Pos;
            float angle = Vector3.SignedAngle(Vector3.right, offset, Vector3.forward);
            rectTransform.rotation = Quaternion.Euler(0, 0, angle);
            rectTransform.sizeDelta = new Vector2(offset.magnitude, rectTransform.sizeDelta.y);
        }

        private void Update()
        {
            UpdateTransform();
        }

        public void DestroyLink()
        {
            UnLinkKnobs();
            Destroy(gameObject);
        }

        public void OnDrag(PointerEventData eventData)
        {
            DestroyLink();
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class NodeTemplate : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        public GameObject Template;
        
        public void OnEndDrag(PointerEventData eventData)
        {
            var frame = GetComponentInParent<Frame>();
            
            var node = Instantiate(Template, frame.transform);
            var rectTransform = node.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = eventData.position;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
        }
    }
}

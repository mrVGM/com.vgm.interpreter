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
            
            var node = Instantiate(Template, frame.NodesAnchor.transform);
            var rectTransform = node.GetComponent<RectTransform>();
            rectTransform.position = eventData.position;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
        }
    }
}

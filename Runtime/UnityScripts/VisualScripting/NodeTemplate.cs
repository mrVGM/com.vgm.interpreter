using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class NodeTemplate : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        private Frame _frame => GetComponentInParent<Frame>();
        public NodeComponent NodePrefab;

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) {
                return;
            }

            var node = Instantiate(NodePrefab, eventData.position, Quaternion.identity, _frame.NodesContainer.transform);
            node.Init(node.NodeTemplate);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }
    }
}
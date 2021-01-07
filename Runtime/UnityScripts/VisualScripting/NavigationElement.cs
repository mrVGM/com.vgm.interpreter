using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class NavigationElement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private Frame _frame => GetComponentInParent<Frame>();
        private Vector2 _startingPosition;

        private bool _startedDragging = false;

        public RectTransform SelectionRactanglePrefab;
        private RectTransform _selectionRactangle = null;

        public RectTransform SelectionRectangleParent;

        public RectTransform SelectionRectangle
        {
            get
            {
                if (_selectionRactangle == null) {
                    _selectionRactangle = Instantiate(SelectionRactanglePrefab, SelectionRectangleParent);
                }
                _selectionRactangle.transform.SetAsLastSibling();
                return _selectionRactangle;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startedDragging = true;
            _startingPosition = eventData.position;
            if (eventData.button == PointerEventData.InputButton.Left) {
                SelectionRectangle.gameObject.SetActive(true);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Middle) {
                Vector2 offset = eventData.position - _startingPosition;
                _frame.NodesContainer.anchoredPosition = offset;
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left) {
                Vector2 offset = _startingPosition - eventData.position;
                Vector2 middle = 0.5f * (_startingPosition + eventData.position) - _frame.NodesContainer.GetComponent<RectTransform>().anchoredPosition;

                SelectionRectangle.anchoredPosition = middle - new Vector2(_frame.NodesContainer.position.x, _frame.NodesContainer.position.y);
                SelectionRectangle.sizeDelta = new Vector2(Mathf.Abs(offset.x), Mathf.Abs(offset.y));
                return;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _startedDragging = false;
            SelectionRectangle.gameObject.SetActive(false);
            if (eventData.button == PointerEventData.InputButton.Middle) {
                Dictionary<Transform, Vector3> children = new Dictionary<Transform, Vector3>();
                for (int i = 0; i < _frame.NodesContainer.transform.childCount; ++i) {
                    var cur = _frame.NodesContainer.transform.GetChild(i);
                    children[cur] = cur.position;
                }

                _frame.NodesContainer.anchoredPosition = Vector2.zero;
                foreach (var item in children) {
                    item.Key.transform.position = item.Value;
                }
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left) {
                bool overlaps(Transform tr1, Transform tr2)
                {
                    
                    float xMin = tr1.GetChild(0).position.x;
                    float xMax = tr1.GetChild(1).position.x;

                    if (xMin > tr2.GetChild(1).position.x) {
                        return false;
                    }

                    if (xMax < tr2.GetChild(0).position.x) {
                        return false;
                    }

                    float yMin = tr1.GetChild(0).position.y;
                    float yMax = tr1.GetChild(1).position.y;

                    if (yMin > tr2.GetChild(1).position.y) {
                        return false;
                    }

                    if (yMax < tr2.GetChild(0).position.y) {
                        return false;
                    }
                    return true;
                }

                var allHandles = _frame.GetComponentsInChildren<MoveHandle>();
                var selectedHandles = allHandles.Where(x => overlaps(SelectionRectangle, x.transform));

                _frame.UnselectAll();
                _frame.Select(selectedHandles);

                return;
            }
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_startedDragging && eventData.button == PointerEventData.InputButton.Left) {
                _frame.UnselectAll();
                return;
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class Frame : MonoBehaviour
    {
        public SessionHolder SessionHolder;
        public Link LinkPrefab;
        public Slider Slider;
        public RectTransform NodesAnchor;

        public void Scale()
        {
            NodesAnchor.transform.localScale = Slider.value * Vector3.one;
        }
    }
}

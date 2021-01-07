using UnityEngine;

namespace ScriptingLanguage.VisualScripting
{
    public class NodeHighlight : MonoBehaviour
    {
        public GameObject HighlightVisuals;

        public void Highlight(bool isHighlighted)
        {
            HighlightVisuals.SetActive(isHighlighted);
        }
    }
}
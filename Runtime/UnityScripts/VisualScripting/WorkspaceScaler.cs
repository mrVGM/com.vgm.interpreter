using UnityEngine;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class WorkspaceScaler : MonoBehaviour
    {
        private Frame _frame => GetComponentInParent<Frame>();
        private Slider _slider => GetComponent<Slider>();
        public void UpdateScale()
        {
            _frame.NodesContainer.localScale = _slider.value * Vector3.one;
        }
    }
}
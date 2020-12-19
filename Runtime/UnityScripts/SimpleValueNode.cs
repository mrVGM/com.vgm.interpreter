using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class SimpleValueNode : MonoBehaviour, ICodeGenerator, IKnobOwner
    {
        public Text Value;
        public InputField InputField;
        public Knob Knob;

        public void EnableTitleInputField()
        {
            if (InputField.gameObject.activeSelf) {
                return;
            }

            InputField.gameObject.SetActive(true);
            InputField.text = InputField.text;
            InputField.ActivateInputField();
        }

        public void OnValueChanged()
        {
            if (string.IsNullOrWhiteSpace(InputField.text)) {
                return;
            }

            Value.text = InputField.text;
            InputField.gameObject.SetActive(false);
        }

        public string GenerateCode(Knob knob)
        {
            return $"{Value.text}";
        }

        public IEnumerator<Knob> GetEnumerator()
        {
            yield return Knob;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

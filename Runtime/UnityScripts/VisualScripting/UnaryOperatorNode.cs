using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class UnaryOperatorNode : MonoBehaviour, ICodeGenerator, IKnobOwner
    {
        public Text Name;
        public InputField InputField;
        public Knob InputKnob;
        public Knob OutputKnob;

        public void EnableTitleInputField()
        {
            if (InputField.gameObject.activeSelf) {
                return;
            }

            InputField.gameObject.SetActive(true);
            InputField.text = Name.text;
            InputField.ActivateInputField();
        }

        public void OnTitleChanged()
        {
            if (string.IsNullOrWhiteSpace(InputField.text)) {
                return;
            }

            if (InputField.text.Contains(" ")) {
                return;
            }

            Name.text = InputField.text;
            InputField.gameObject.SetActive(false);
        }

        public string GenerateCode(Knob knob)
        {
            if (knob != OutputKnob) {
                throw new NotSupportedException();
            }

            var endpoint = InputKnob.LinkEndpoints.FirstOrDefault();
            
            var linkedKnob = endpoint.GetCounterpart().Knob;
            var codeGenerator = linkedKnob.GetComponentInParent<ICodeGenerator>();

            return $"{Name}{codeGenerator.GenerateCode(linkedKnob)}";
        }

        public IEnumerator<Knob> GetEnumerator()
        {
            yield return InputKnob;
            yield return OutputKnob;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

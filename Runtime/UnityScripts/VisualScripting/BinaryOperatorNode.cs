using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class BinaryOperatorNode : MonoBehaviour, ICodeGenerator, IKnobOwner
    {
        public Text Name;
        public InputField InputField;
        public Knob LeftKnob;
        public Knob RightKnob;
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

            var leftEndpoint = LeftKnob.LinkEndpoints.FirstOrDefault();
            var rightEndpoint = RightKnob.LinkEndpoints.FirstOrDefault();
            
            var leftLinkedKnob = leftEndpoint.GetCounterpart().Knob;
            var rightLinkedKnob = rightEndpoint.GetCounterpart().Knob;
            var leftCodeGenerator = leftLinkedKnob.GetComponentInParent<ICodeGenerator>();
            var rightCodeGenerator = rightLinkedKnob.GetComponentInParent<ICodeGenerator>();

            return $"({leftCodeGenerator.GenerateCode(leftLinkedKnob)}{Name}{rightCodeGenerator.GenerateCode(rightLinkedKnob)})";
        }

        public IEnumerator<Knob> GetEnumerator()
        {
            yield return LeftKnob;
            yield return RightKnob;
            yield return OutputKnob;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

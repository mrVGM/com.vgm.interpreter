using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class ObjectNode : MonoBehaviour, ICodeGenerator, IKnobOwner
    {
        public Text Name;
        public InputField InputField;
        public Knob LeftKnob;
        public Knob RightKnob;
        public Text InfoText;

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
            if (knob != RightKnob) {
                throw new NotSupportedException();
            }

            var endpoint = LeftKnob.LinkEndpoints.FirstOrDefault();
            if (endpoint == null) {
                return Name.text;
            }

            var linkedKnob = endpoint.GetCounterpart().Knob;
            var codeGenerator = linkedKnob.GetComponentInParent<ICodeGenerator>();
            if (codeGenerator == null) {
                return Name.text;
            }

            return $"{codeGenerator.GenerateCode(linkedKnob)}.{Name.text}";
        }

        public void Evaluate()
        {
            var frame = GetComponentInParent<Frame>();
            var res = frame.SessionHolder.RunCommand($"{GenerateCode(RightKnob)};");

            InfoText.text = res.LastOrDefault();
            InfoText.gameObject.SetActive(true);
        }

        public IEnumerator<Knob> GetEnumerator()
        {
            yield return LeftKnob;
            yield return RightKnob;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

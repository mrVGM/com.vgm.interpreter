using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class VariableNode : MonoBehaviour, ICodeGenerator, IKnobOwner
    {
        public Text Name;
        public Knob DeclarationKnob;
        public Knob NameReferenceKnob;
        public InputField InputField;

        public void EnableTitleInputField()
        {
            if (InputField.gameObject.activeSelf)
            {
                return;
            }

            InputField.gameObject.SetActive(true);
            InputField.text = Name.text;
            InputField.ActivateInputField();
        }

        public void OnTitleChanged()
        {
            if (string.IsNullOrWhiteSpace(InputField.text))
            {
                return;
            }

            if (InputField.text.Contains(" "))
            {
                return;
            }

            Name.text = InputField.text;
            InputField.gameObject.SetActive(false);
        }
        public string GenerateCode(Knob knob)
        {
            if (knob == NameReferenceKnob) {
                return $"{Name.text}";
            }

            if (knob == DeclarationKnob) {
                return $"let {Name.text} = null;";
            }

            throw new NotSupportedException();
        }

        public void Evaluate()
        {
        }

        public IEnumerator<Knob> GetEnumerator()
        {
            yield return DeclarationKnob;
            yield return NameReferenceKnob;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

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
        public Knob InputKnob;
        public Knob OutputKnob;

        public string GenerateCode(Knob knob)
        {
            if (knob != OutputKnob) {
                throw new NotSupportedException();
            }

            var endpoint = InputKnob.LinkEndpoints.FirstOrDefault();
            
            var linkedKnob = endpoint.GetCounterpart().Knob;
            var codeGenerator = linkedKnob.GetComponentInParent<ICodeGenerator>();

            return $"{Name.text}{codeGenerator.GenerateCode(linkedKnob)}";
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

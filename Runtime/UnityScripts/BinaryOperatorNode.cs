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
        public Knob LeftKnob;
        public Knob RightKnob;
        public Knob OutputKnob;

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

            return $"({leftCodeGenerator.GenerateCode(leftLinkedKnob)}{Name.text}{rightCodeGenerator.GenerateCode(rightLinkedKnob)})";
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

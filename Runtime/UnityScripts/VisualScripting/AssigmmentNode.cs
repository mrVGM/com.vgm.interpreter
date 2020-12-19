using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class AssigmmentNode : MonoBehaviour, ICodeGenerator, IKnobOwner
    {
        public Knob LeftKnob;
        public Knob RightKnob;
        public Knob AssignmentNode;

        public string GenerateCode(Knob knob)
        {
            if (knob != AssignmentNode) {
                throw new NotSupportedException();
            }

            var leftItem = LeftKnob.LinkEndpoints.FirstOrDefault();
            if (leftItem == null) {
                throw new NotSupportedException();
            }

            var leftItemKnob = leftItem.GetCounterpart().Knob;
            var leftItemCodeGenerator = leftItemKnob.GetComponentInParent<ICodeGenerator>();

            var rightItem = RightKnob.LinkEndpoints.FirstOrDefault();
            if (rightItem == null)
            {
                throw new NotSupportedException();
            }

            var rightItemKnob = rightItem.GetCounterpart().Knob;
            var rightItemCodeGenerator = rightItemKnob.GetComponentInParent<ICodeGenerator>();


            return $"{leftItemCodeGenerator.GenerateCode(leftItemKnob)} = {rightItemCodeGenerator.GenerateCode(rightItemKnob)};";
        }

        public void Evaluate()
        {
        }

        public IEnumerator<Knob> GetEnumerator()
        {
            yield return LeftKnob;
            yield return RightKnob;
            yield return AssignmentNode;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

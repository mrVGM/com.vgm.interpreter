using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class ConditionalBlockNode : MonoBehaviour, ICodeGenerator, IKnobOwner
    {
        public Text Name;
        public Knob Expression;
        public Knob Block;
        public Knob Instruction;

        public string GenerateCode(Knob knob)
        {
            if (knob != Instruction) {
                throw new NotSupportedException();
            }

            var expressionLinkKnob = Expression.LinkEndpoints.FirstOrDefault().GetCounterpart().Knob;
            var expressionCodeGenerator = expressionLinkKnob.GetComponentInParent<ICodeGenerator>();

            var blockLinkKnob = Block.LinkEndpoints.FirstOrDefault().GetCounterpart().Knob;
            var blockCodeGenerator = blockLinkKnob.GetComponentInParent<ICodeGenerator>();

            return $"{Name} ({expressionCodeGenerator.GenerateCode(expressionLinkKnob)})\n{blockCodeGenerator.GenerateCode(blockLinkKnob)}";
        }

        public IEnumerator<Knob> GetEnumerator()
        {
            yield return Expression;
            yield return Block;
            yield return Instruction;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage.VisualScripting
{
    public class InstructionsBlockNode : MonoBehaviour, ICodeGenerator, IKnobOwner
    {
        enum KnobOperation
        {
            Add,
            Remove,
        }
        
        public Knob KnobPrefab;
        public RectTransform InstructionKnobs;
        public Knob BlockKnob;

        private void UpdateKnobs(KnobOperation knobOperation, RectTransform parentTransform)
        {
            if (knobOperation == KnobOperation.Add) {
                Instantiate(KnobPrefab, parentTransform);
                return;
            }

            if (knobOperation == KnobOperation.Remove) {
                var knob = parentTransform.GetComponentsInChildren<Knob>().LastOrDefault();
                if (knob == null) {
                    return;
                }
                foreach (var linkEndpoint in KnobPrefab.LinkEndpoints) {
                    linkEndpoint.GetLink().DestroyLink();
                }

                Destroy(knob.gameObject);
            }
        }

        public void AddInstruction()
        {
            UpdateKnobs(KnobOperation.Add, InstructionKnobs);
        }
        
        public void RemoveInstruction()
        {
            UpdateKnobs(KnobOperation.Remove, InstructionKnobs);
        }

        public string GenerateCode(Knob knob)
        {
            if (knob != BlockKnob) {
                throw new NotSupportedException();
            }

            var instructionKnobs = InstructionKnobs.GetComponentsInChildren<Knob>();
            string instructions = "";
            if (instructionKnobs.Any()) {
                foreach (var templateArgKnob in instructionKnobs) {
                    var counterPart = templateArgKnob.LinkEndpoints.FirstOrDefault().GetCounterpart().Knob;
                    var codeGenerator = counterPart.GetComponentInParent<ICodeGenerator>();
                    instructions += $"{codeGenerator.GenerateCode(counterPart)}\n";
                }
            }
            
            return $"{{\n{instructionKnobs}\n}}";
        }

        public IEnumerator<Knob> GetEnumerator()
        {
            yield return BlockKnob;
            foreach (var knob in InstructionKnobs.GetComponentsInChildren<Knob>()) {
                yield return knob;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

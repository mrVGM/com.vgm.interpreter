using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage.VisualScripting
{
    public class ExecutionNode : MonoBehaviour, IKnobOwner
    {
        enum KnobOperation
        {
            Add,
            Remove,
        }
        
        public Knob KnobPrefab;
        public RectTransform InstructionKnobs;

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

        public void ExecuteCode()
        {
            var instructionKnobs = InstructionKnobs.GetComponentsInChildren<Knob>();
            string instructions = "";
            if (instructionKnobs.Any()) {
                foreach (var templateArgKnob in instructionKnobs) {
                    var counterPart = templateArgKnob.LinkEndpoints.FirstOrDefault().GetCounterpart().Knob;
                    var codeGenerator = counterPart.GetComponentInParent<ICodeGenerator>();
                    instructions += $"{codeGenerator.GenerateCode(counterPart)}\n";
                }
            }

            var frame = GetComponentInParent<Frame>();
            var sessionHolder = frame.SessionHolder;

            var commands = instructions.Split('\n');
            foreach (var command in commands) {
                var output = sessionHolder.RunCommand(command);
                foreach (var message in output) {
                    Debug.Log(message);
                }
            }
        }

        public IEnumerator<Knob> GetEnumerator()
        {
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

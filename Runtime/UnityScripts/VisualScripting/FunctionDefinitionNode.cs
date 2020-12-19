using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage.VisualScripting
{
    public class FunctionDefinitionNode : MonoBehaviour, ICodeGenerator, IKnobOwner
    {
        enum KnobOperation
        {
            Add,
            Remove,
        }
        
        public Knob KnobPrefab;
        public Knob Function;
        public RectTransform Parameters;
        public Knob Block;
    

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

        public void AddParameter()
        {
            UpdateKnobs(KnobOperation.Add, Parameters);
        }
        
        public void RemoveParameter()
        {
            UpdateKnobs(KnobOperation.Remove, Parameters);
        }

        public string GenerateCode(Knob knob)
        {
            if (knob != Function) {
                throw new NotSupportedException();
            }

            var parameterKnobs = Parameters.GetComponentsInChildren<Knob>();
            
            string parameters = "";
            foreach (var templateArgKnob in parameterKnobs) {
                var counterPart = templateArgKnob.LinkEndpoints.FirstOrDefault().GetCounterpart().Knob;
                var codeGenerator = counterPart.GetComponentInParent<ICodeGenerator>();
                parameters += $"{codeGenerator.GenerateCode(counterPart)},";
            }
            parameters = $"({parameters})";

            var blockLinkKnob = Block.LinkEndpoints.FirstOrDefault().GetCounterpart().Knob;
            var blockCodeGenerator = blockLinkKnob.GetComponentInParent<ICodeGenerator>();
            
            return $"function{parameters}\n{blockCodeGenerator.GenerateCode(blockLinkKnob)}";
        }

        public IEnumerator<Knob> GetEnumerator()
        {
            yield return Function;
            foreach (var knob in Parameters.GetComponentsInChildren<Knob>()) {
                yield return knob;
            }
            yield return Block;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

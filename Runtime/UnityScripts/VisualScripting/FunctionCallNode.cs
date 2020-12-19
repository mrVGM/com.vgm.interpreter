using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage.VisualScripting
{
    public class FunctionCallNode : MonoBehaviour, ICodeGenerator, IKnobOwner
    {
        enum KnobOperation
        {
            Add,
            Remove,
        }
        
        public Knob KnobPrefab;
        public RectTransform TemplateArgs;
        public RectTransform Args;
        public Knob Function;
        public Knob FunctionCall;
        public Knob Output;

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

        public void AddTemplateArg()
        {
            UpdateKnobs(KnobOperation.Add, TemplateArgs);
        }
        
        public void RemoveTemplateArg()
        {
            UpdateKnobs(KnobOperation.Remove, TemplateArgs);
        }
        
        public void AddArg()
        {
            UpdateKnobs(KnobOperation.Add, Args);
        }
        
        public void RemoveArg()
        {
            UpdateKnobs(KnobOperation.Remove, Args);
        }

        public void RunFunction()
        {
            var frame = GetComponentInParent<Frame>();
            string code = $"{GenerateCode(Output)};";
            frame.SessionHolder.RunCommand(code).ToList();
        }

        public string GenerateCode(Knob knob)
        {
            if (knob != Output && knob != FunctionCall) {
                throw new NotSupportedException();
            }
            var funcKnob = Function.LinkEndpoints.FirstOrDefault().GetCounterpart().Knob;
            var funcCodeGenerator = funcKnob.GetComponentInParent<ICodeGenerator>();

            var templateArgKnobs = TemplateArgs.GetComponentsInChildren<Knob>();
            
            var argKnobs = Args.GetComponentsInChildren<Knob>();

            string templateArgs = "";
            if (templateArgKnobs.Any()) {
                templateArgs += "|";
                foreach (var templateArgKnob in templateArgKnobs) {
                    var counterPart = templateArgKnob.LinkEndpoints.FirstOrDefault().GetCounterpart().Knob;
                    var codeGenerator = counterPart.GetComponentInParent<ICodeGenerator>();
                    templateArgs += $"{codeGenerator.GenerateCode(counterPart)}|";
                }
            }
            
            string args = "";
            foreach (var argKnob in argKnobs) {
                var counterPart = argKnob.LinkEndpoints.FirstOrDefault().GetCounterpart().Knob;
                var codeGenerator = counterPart.GetComponentInParent<ICodeGenerator>();
                args += $"{codeGenerator.GenerateCode(counterPart)},";
            }
            if (args.Length > 1) {
                args = args.Substring(0, args.Length - 1);
            }
            args = $"({args})";
            
            return $"{funcCodeGenerator.GenerateCode(funcKnob)}{templateArgs}{args}{(knob == FunctionCall ? ";" : "")}";
        }

        public IEnumerator<Knob> GetEnumerator()
        {
            yield return Function;
            foreach (var knob in TemplateArgs.GetComponentsInChildren<Knob>()) {
                yield return knob;
            }
            foreach (var knob in Args.GetComponentsInChildren<Knob>()) {
                yield return knob;
            }
            yield return Output;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

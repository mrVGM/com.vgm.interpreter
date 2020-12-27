using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class SimpleValueNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(SimpleValueNodeComponent))]
        public class SimpleValueNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return ValueEndpoint;
                }
            }

            public Endpoint ValueEndpoint = new Endpoint();
            public string Value = "null";

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                if (string.IsNullOrWhiteSpace(Value))
                {
                    Value = context.GenerateVarName();
                }

                if (endpoint == ValueEndpoint)
                {
                    return $"{Value}";
                }

                throw new InvalidOperationException();
            }
        }

        public EndpointComponent ValueEndpoint;
        public InputField InputField;
        public Text Value;

        private void ActivateInputField(bool activate) 
        {
            InputField.gameObject.SetActive(activate);
            if (activate)
            {
                InputField.text = _node.Value;
            }
            else 
            {
                _node.Value = InputField.text;
                RefreshNodeComponents();
            }
        }

        public void StartChangingName() 
        {
            ActivateInputField(true);
        }
        public void EndChangingName() 
        {
            if (string.IsNullOrWhiteSpace(InputField.text)) {
                return;
            }
            ActivateInputField(false);
        }

        public SimpleValueNode _node;

        public override INode NodeTemplate => new SimpleValueNode();

        public override INode Node => _node;

        private void RefreshNodeComponents() 
        {
            ValueEndpoint.Endpoint = _node.ValueEndpoint;
            Value.text = _node.Value;
        }
        public override void ParticularInit(INode node)
        {
            _node = node as SimpleValueNode;
            RefreshNodeComponents();
        }
    }
}
using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class VariableNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(VariableNodeComponent))]
        public class VariableNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return NameEndpoint;
                }
            }
            public Endpoint NameEndpoint = new Endpoint();
            public string VariableName = "";

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                if (string.IsNullOrWhiteSpace(VariableName)) {
                    VariableName = context.GenerateVarName();
                }

                if (endpoint == NameEndpoint) {
                    var assignmentRequest = context.CustomContext as VariableCreationRequest;
                    if (assignmentRequest == null || context.IsVariableDeclared(VariableName)) {
                        return $"{VariableName}";
                    }
                    context.DeclaredVar(VariableName);
                    return $"let {VariableName}";
                }

                throw new InvalidOperationException();
            }
        }
        public EndpointComponent NameEndpoint;
        public InputField InputField;
        public Text VarName;

        private void ActivateInputField(bool activate) 
        {
            InputField.gameObject.SetActive(activate);
            if (activate)
            {
                InputField.text = _node.VariableName;
                InputField.ActivateInputField();
                InputField.Select();
            }
            else 
            {
                _node.VariableName = InputField.text;
                RefreshNodeComponent();
            }
        }

        public void StartChangingName() 
        {
            ActivateInputField(true);
        }
        public void EndChangingName() 
        {
            var regex = new Regex(@"^[a-zA-Z]+[a-zA-Z0-9_]*$");
            var match = regex.Match(InputField.text);
            if (match.Success)
            {
                ActivateInputField(false);
            }
        }

        public VariableNode _node;

        public override INode NodeTemplate => new VariableNode();

        public override INode Node => _node;

        private void RefreshNodeComponent() 
        {
            NameEndpoint.Endpoint = _node.NameEndpoint;
            VarName.text = _node.VariableName;
        }
        public override void ParticularInit(INode node)
        {
            _node = node as VariableNode;
            RefreshNodeComponent();
            if (string.IsNullOrWhiteSpace(VarName.text)) {
                VarName.text = "<unnamed>";
            }
        }
    }
}
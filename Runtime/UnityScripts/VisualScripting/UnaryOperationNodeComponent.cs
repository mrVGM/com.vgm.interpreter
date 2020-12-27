using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class UnaryOperationNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(UnaryOperationNodeComponent))]
        public class UnaryOperationNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return OperandEndpoint;
                    yield return ResultEndpoint;
                }
            }

            public Endpoint OperandEndpoint = new Endpoint();
            public Endpoint ResultEndpoint = new Endpoint();
            public string Operation = "!";

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                if (endpoint != ResultEndpoint)
                {
                    throw new InvalidOperationException();
                }
                var operand = OperandEndpoint.LinkedEndpoints.FirstOrDefault();
                var operandNode = nodesDB.GetNodeByEndpoint(operand);

                return $"({Operation} {operandNode.GenerateCode(operand, nodesDB, context)})";
            }
        }

        public EndpointComponent OperandEndpoint;
        public EndpointComponent ResultEndpoint;
        public Dropdown Operation;

        public UnaryOperationNode _node;

        public override INode NodeTemplate => new UnaryOperationNode();

        public override INode Node => _node;

        public void OnOperationChanged() 
        {
            var curOption = Operation.options[Operation.value];
            _node.Operation = curOption.text;
        }
        public override void ParticularInit(INode node)
        {
            _node = node as UnaryOperationNode;

            OperandEndpoint.Endpoint = _node.OperandEndpoint;
            ResultEndpoint.Endpoint = _node.ResultEndpoint;
            int index = -1;
            for (int i = 0; i < Operation.options.Count; ++i)
            {
                var cur = Operation.options[i];
                if (cur.text == _node.Operation)
                {
                    index = i;
                    break;
                }
            }
            Operation.value = index;
        }
    }
}
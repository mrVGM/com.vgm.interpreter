using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class BinaryOperationNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(BinaryOperationNodeComponent))]
        public class BinaryOperationNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return LeftOperandEndpoint;
                    yield return RightOperandEndpoint;
                    yield return ResultEndpoint;
                }
            }

            public Endpoint LeftOperandEndpoint = new Endpoint();
            public Endpoint RightOperandEndpoint = new Endpoint();
            public Endpoint ResultEndpoint = new Endpoint();
            public string Operation = "+";

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                if (endpoint != ResultEndpoint)
                {
                    throw new InvalidOperationException();
                }
                var left = LeftOperandEndpoint.LinkedEndpoints.FirstOrDefault();
                var right = RightOperandEndpoint.LinkedEndpoints.FirstOrDefault();
                var leftOperandNode = nodesDB.GetNodeByEndpoint(left);
                var rightOperandNode = nodesDB.GetNodeByEndpoint(right);

                return $"({leftOperandNode.GenerateCode(left, nodesDB, context)} {Operation} {rightOperandNode.GenerateCode(right, nodesDB, context)})";
            }
        }

        public EndpointComponent LeftOperandEndpoint;
        public EndpointComponent RightOperandEndpoint;
        public EndpointComponent ResultEndpoint;
        public Dropdown Operation;

        public BinaryOperationNode _node;

        public override INode NodeTemplate => new BinaryOperationNode();

        public override INode Node => _node;

        public void OnOperationChanged() 
        {
            var curOption = Operation.options[Operation.value];
            _node.Operation = curOption.text;
        }
        public override void ParticularInit(INode node)
        {
            _node = node as BinaryOperationNode;

            LeftOperandEndpoint.Endpoint = _node.LeftOperandEndpoint;
            RightOperandEndpoint.Endpoint = _node.RightOperandEndpoint;
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
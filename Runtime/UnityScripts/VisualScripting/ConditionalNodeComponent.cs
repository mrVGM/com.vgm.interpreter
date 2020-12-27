using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class ConditionalNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(ConditionalNodeComponent))]
        public class ConditionalNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return LeftFlowEndpoint;
                    yield return RightFlowEndpoint;
                    yield return ExpressionEndpoint;
                    yield return BodyEndpoint;
                }
            }


            public Endpoint LeftFlowEndpoint = new Endpoint();
            public Endpoint RightFlowEndpoint = new Endpoint();
            public Endpoint ExpressionEndpoint = new Endpoint();
            public Endpoint BodyEndpoint = new Endpoint();
            public string Operator = "if";

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                if (endpoint != LeftFlowEndpoint)
                {
                    throw new InvalidOperationException();
                }
                var expressionEndpoint = ExpressionEndpoint.LinkedEndpoints.FirstOrDefault();
                var expressionNode = nodesDB.GetNodeByEndpoint(expressionEndpoint);
                string expressionCode = expressionNode.GenerateCode(expressionEndpoint, nodesDB, context);

                var bodyEndpoint = BodyEndpoint.LinkedEndpoints.FirstOrDefault();
                var bodyNode = nodesDB.GetNodeByEndpoint(bodyEndpoint);
                string bodyCode;
                using (context.CreateTemporaryScope()) {
                    bodyCode = bodyNode.GenerateCode(bodyEndpoint, nodesDB, context);
                }

                var nextEndpoint = RightFlowEndpoint.LinkedEndpoints.FirstOrDefault();
                string nextNodeCode = "";
                if (nextEndpoint != null) {
                    var nextNode = nodesDB.GetNodeByEndpoint(nextEndpoint);
                    nextNodeCode = nextNode.GenerateCode(nextEndpoint, nodesDB, context);
                }
                return $"{Operator} ({expressionCode}) {{\n{bodyCode}}}\n{nextNodeCode}";
            }
        }

        public EndpointComponent LeftFlowEndpoint;
        public EndpointComponent RightFlowEndpoint;
        public EndpointComponent ExpressionEndpoint;
        public EndpointComponent BodyEndpoint;

        public Dropdown Operator;

        public ConditionalNode _node;

        public override INode NodeTemplate => new ConditionalNode();

        public override INode Node => _node;

        public void OnOperationChanged() 
        {
            var curOption = Operator.options[Operator.value];
            _node.Operator = curOption.text;
        }
        public override void ParticularInit(INode node)
        {
            _node = node as ConditionalNode;

            LeftFlowEndpoint.Endpoint = _node.LeftFlowEndpoint;
            RightFlowEndpoint.Endpoint = _node.RightFlowEndpoint;
            ExpressionEndpoint.Endpoint = _node.ExpressionEndpoint;
            BodyEndpoint.Endpoint = _node.BodyEndpoint;

            int index = -1;
            for (int i = 0; i < Operator.options.Count; ++i)
            {
                var cur = Operator.options[i];
                if (cur.text == _node.Operator)
                {
                    index = i;
                    break;
                }
            }
            Operator.value = index;
        }
    }
}
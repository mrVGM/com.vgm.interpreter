using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptingLanguage.VisualScripting
{
    public class AssignmentNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(AssignmentNodeComponent))]
        public class AssignmentNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return LeftFlowEndpoint;
                    yield return RightFlowEndpoint;
                    yield return From;
                    yield return To;
                }
            }

            public Endpoint LeftFlowEndpoint = new Endpoint();
            public Endpoint RightFlowEndpoint = new Endpoint();
            public Endpoint From = new Endpoint();
            public Endpoint To = new Endpoint();

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context) 
            {
                string ownCode = GenerateOwnCode(endpoint, nodesDB, context);
                var nextEndpoint = RightFlowEndpoint.LinkedEndpoints.FirstOrDefault();
                if (nextEndpoint == null) {
                    return ownCode;
                }
                var nextNode = nodesDB.GetNodeByEndpoint(nextEndpoint);
                var nextCode = nextNode.GenerateCode(nextEndpoint, nodesDB, context);

                return $"{ownCode}{nextCode}";
            }
            private string GenerateOwnCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                if (endpoint != LeftFlowEndpoint) 
                {
                    throw new InvalidOperationException();
                }

                var fromEndpoint = From.LinkedEndpoints.FirstOrDefault();
                var toEndpoint = To.LinkedEndpoints.FirstOrDefault();
                var fromNode = nodesDB.GetNodeByEndpoint(fromEndpoint);
                var toNode = nodesDB.GetNodeByEndpoint(toEndpoint);

                var assignmentRequest = new VariableCreationRequest();
                string toCode, fromCode;
                using (context.CreateTemporaryCustomContext(assignmentRequest))
                {
                    toCode = toNode.GenerateCode(toEndpoint, nodesDB, context);
                }
                fromCode = fromNode.GenerateCode(fromEndpoint, nodesDB, context);
                return $"{toCode} = {fromCode};\n";
            }
        }

        public EndpointComponent LeftFlowEndpoint;
        public EndpointComponent RightFlowEndpoint;
        public EndpointComponent From;
        public EndpointComponent To;

        public AssignmentNode _node;

        public override INode NodeTemplate => new AssignmentNode();

        public override INode Node => _node;

        public override void ParticularInit(INode node)
        {
            _node = node as AssignmentNode;

            LeftFlowEndpoint.Endpoint = _node.LeftFlowEndpoint;
            RightFlowEndpoint.Endpoint = _node.RightFlowEndpoint;
            From.Endpoint = _node.From;
            To.Endpoint = _node.To;
        }
    }
}
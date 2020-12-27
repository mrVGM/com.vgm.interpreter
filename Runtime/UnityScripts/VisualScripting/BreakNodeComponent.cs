using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;

namespace ScriptingLanguage.VisualScripting
{
    public class BreakNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(BreakNodeComponent))]
        public class BreakNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return LeftFlowEndpoint;
                }
            }
            public Endpoint LeftFlowEndpoint = new Endpoint();

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                if (endpoint != LeftFlowEndpoint) {
                    throw new InvalidOperationException();
                }

                return $"break;\n";
            }
        }
        public EndpointComponent LeftFlowEndpoint;

        public BreakNode _node;

        public override INode NodeTemplate => new BreakNode();

        public override INode Node => _node;

        public override void ParticularInit(INode node)
        {
            _node = node as BreakNode;

            LeftFlowEndpoint.Endpoint = _node.LeftFlowEndpoint;
        }
    }
}
using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptingLanguage.VisualScripting
{
    public class ReturnNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(ReturnNodeComponent))]
        public class ReturnNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return LeftFlowEndpoint;
                    yield return ValueEndpoint;
                }
            }
            public Endpoint LeftFlowEndpoint = new Endpoint();
            public Endpoint ValueEndpoint = new Endpoint();

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                if (endpoint != LeftFlowEndpoint) {
                    throw new InvalidOperationException();
                }

                string code = "";
                var valueEndpoint = ValueEndpoint.LinkedEndpoints.FirstOrDefault();
                if (valueEndpoint != null) {
                    var valueNode = nodesDB.GetNodeByEndpoint(valueEndpoint);
                    code = valueNode.GenerateCode(valueEndpoint, nodesDB, context);
                }
                return $"return {code};\n";
            }
        }
        public EndpointComponent LeftFlowEndpoint;
        public EndpointComponent ValueEndpoint;

        public ReturnNode _node;

        public override INode NodeTemplate => new ReturnNode();

        public override INode Node => _node;

        public override void ParticularInit(INode node)
        {
            _node = node as ReturnNode;

            LeftFlowEndpoint.Endpoint = _node.LeftFlowEndpoint;
            ValueEndpoint.Endpoint = _node.ValueEndpoint;
        }
    }
}
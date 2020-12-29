using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage.VisualScripting
{
    public class ExecutionStartNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(ExecutionStartNodeComponent))]
        public class ExecutionStartNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return RightEndpoint;
                }
            }

            public Endpoint RightEndpoint = new Endpoint();

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                var otherEndpoint = endpoint.LinkedEndpoints.FirstOrDefault();
                var otherNode = nodesDB.GetNodeByEndpoint(otherEndpoint);
                if (context != null) 
                {
                    throw new InvalidOperationException();
                }
                context = new CodeGenerationContext();

                string res;
                using (context.CreateTemporaryScope()) {
                    res = otherNode.GenerateCode(otherEndpoint, nodesDB, context); 
                }
                return res;
            }
        }

        public EndpointComponent RightEndpoint;

        public ExecutionStartNode _node;

        public override INode NodeTemplate => new ExecutionStartNode();

        public override INode Node => _node;

        public void TriggerExecution() 
        {
            var frame = GetComponentInParent<Frame>();
            string code = _node.GenerateCode(_node.RightEndpoint, frame.NodesDB, null);
            var codeLines = code.Split('\n');
            foreach (var line in codeLines)
            {
                var output = frame.SessionHolder.RunCommand(line);
                foreach (var s in output)
                {
                    Debug.Log(s);
                }
            }
        }

        public override void ParticularInit(INode node)
        {
            _node = node as ExecutionStartNode;
            RightEndpoint.Endpoint = _node.RightEndpoint;
        }
    }
}
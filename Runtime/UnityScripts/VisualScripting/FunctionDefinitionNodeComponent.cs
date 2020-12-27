using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptingLanguage.VisualScripting
{
    public class FunctionDefinitionNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(FunctionDefinitionNodeComponent))]
        public class FunctionDefinitionNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return DefinitionEndpoint;
                    yield return BodyEndpoint;

                    foreach (var endpoint in ParameterEndpoints)
                    {
                        yield return endpoint;
                    }
                }
            }

            public Endpoint DefinitionEndpoint = new Endpoint();
            public Endpoint BodyEndpoint = new Endpoint();

            public List<Endpoint> ParameterEndpoints = new List<Endpoint>();

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                if (endpoint != DefinitionEndpoint) {
                    throw new InvalidOperationException();
                }

                var parameters = "";
                List<string> varNames = new List<string>();
                if (ParameterEndpoints.Count > 0)
                {
                    foreach (var arg in ParameterEndpoints)
                    {
                        var otherEndpoint = arg.LinkedEndpoints.FirstOrDefault();
                        var otherNode = nodesDB.GetNodeByEndpoint(otherEndpoint);
                        string code;
                        using (context.CreateTemporaryCustomContext(null)) {
                            code = otherNode.GenerateCode(otherEndpoint, nodesDB, context);
                        }
                        varNames.Add(code);
                        parameters += code + ",";
                    }
                    parameters = parameters.Substring(0, parameters.Length - 1);
                }
                parameters = $"({parameters})";

                var bodyEndpoint = BodyEndpoint.LinkedEndpoints.FirstOrDefault();
                var bodyNode = nodesDB.GetNodeByEndpoint(bodyEndpoint);
                string bodyCode;
                using (context.CreateTemporaryScope(varNames.ToArray())) {
                    bodyCode = bodyNode.GenerateCode(bodyEndpoint, nodesDB, context);
                }

                return $"function({parameters}) {{\n{bodyCode}}}";
            }
        }

        public EndpointComponent DefinitionEndpoint;
        public EndpointComponent BodyEndpoint;
        public List<Endpoint> ParameterEndpoints;

        public EndpointComponent EndpointPrefab;
        public RectTransform Parameters;

        public FunctionDefinitionNode _node;

        public override INode NodeTemplate => new FunctionDefinitionNode();

        public override INode Node => _node;

        public override void ParticularInit(INode node)
        {
            _node = node as FunctionDefinitionNode;

            DefinitionEndpoint.Endpoint = _node.DefinitionEndpoint;
            BodyEndpoint.Endpoint = _node.BodyEndpoint;

            foreach (var endpoint in _node.ParameterEndpoints) {
                AddParameterGeneric(endpoint, Parameters, null);
            }
        }

        private void AddParameterGeneric(Endpoint endpoint, RectTransform parentTransform, List<Endpoint> nodeEndPoints)
        {
            var endpointComponent = Instantiate(EndpointPrefab, parentTransform);
            if (endpoint == null)
            {
                endpoint = new Endpoint();
                nodeEndPoints.Add(endpoint);
            }
            endpointComponent.Endpoint = endpoint;
        }

        private void RemoveParameterGeneric(RectTransform parentTransform, List<Endpoint> nodeEndPoints)
        {
            if (parentTransform.childCount == 0) {
                return;
            }
            var endpointComponent = parentTransform.GetChild(parentTransform.childCount - 1).GetComponent<EndpointComponent>();
            endpointComponent.UnLinkAll();

            Destroy(parentTransform.GetChild(parentTransform.childCount - 1).gameObject);
            nodeEndPoints.RemoveAt(nodeEndPoints.Count - 1);
        }

        public void AddParam()
        {
            AddParameterGeneric(null, Parameters, _node.ParameterEndpoints);
        }
        public void RemoveParam()
        {
            RemoveParameterGeneric(Parameters, _node.ParameterEndpoints);
        }
    }
}
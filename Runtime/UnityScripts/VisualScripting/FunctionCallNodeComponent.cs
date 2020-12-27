using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class FunctionCallNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(FunctionCallNodeComponent))]
        public class FunctionCallNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return LeftFlowEndpoint;
                    yield return RightFlowEndpoint;
                    yield return FunctionEndpoint;
                    yield return OutputEndpoint;

                    foreach (var endpoint in TemplateArgumentEndpoints) {
                        yield return endpoint;
                    }
                    foreach (var endpoint in ArgumentEndpoints)
                    {
                        yield return endpoint;
                    }
                }
            }

            public Endpoint LeftFlowEndpoint = new Endpoint();
            public Endpoint RightFlowEndpoint = new Endpoint();
            public Endpoint FunctionEndpoint = new Endpoint();
            public Endpoint OutputEndpoint = new Endpoint();

            public List<Endpoint> TemplateArgumentEndpoints = new List<Endpoint>();
            public List<Endpoint> ArgumentEndpoints = new List<Endpoint>();

            

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                if (endpoint != LeftFlowEndpoint && endpoint != OutputEndpoint) {
                    throw new InvalidOperationException();
                }

                var funcEndpoint = FunctionEndpoint.LinkedEndpoints.FirstOrDefault();
                var funcNode = nodesDB.GetNodeByEndpoint(funcEndpoint);
                var func = funcNode.GenerateCode(funcEndpoint, nodesDB, context);

                var templateArgs = "";
                if (TemplateArgumentEndpoints.Count > 0) {
                    templateArgs = "|";
                    foreach (var templateArg in TemplateArgumentEndpoints) {
                        var otherEndpoint = templateArg.LinkedEndpoints.FirstOrDefault();
                        var otherNode = nodesDB.GetNodeByEndpoint(otherEndpoint);
                        var code = otherNode.GenerateCode(otherEndpoint, nodesDB, context);
                        templateArgs += code + "|";
                    }
                }

                var args = "";
                if (ArgumentEndpoints.Count > 0) {
                    foreach (var arg in ArgumentEndpoints) {
                        var otherEndpoint = arg.LinkedEndpoints.FirstOrDefault();
                        var otherNode = nodesDB.GetNodeByEndpoint(otherEndpoint);
                        var code = otherNode.GenerateCode(otherEndpoint, nodesDB, context);
                        args += code + ",";
                    }
                    args = args.Substring(0, args.Length - 1);
                }
                args = $"({args})";


                string res = $"{func}{templateArgs}{args}";
                if (endpoint == OutputEndpoint) {
                    return res;
                }
                var nextEndpoint = RightFlowEndpoint.LinkedEndpoints.FirstOrDefault();
                string nextNodeCode = "";
                if (nextEndpoint != null)
                {
                    var nextNode = nodesDB.GetNodeByEndpoint(nextEndpoint);
                    nextNodeCode = nextNode.GenerateCode(nextEndpoint, nodesDB, context);
                }
                return res + $";\n{nextNodeCode}";
            }
        }

        public EndpointComponent LeftFlowEndpoint;
        public EndpointComponent RightFlowEndpoin;
        public EndpointComponent FunctionEndpoint;
        public EndpointComponent OutputEndpoint;

        public List<EndpointComponent> TemplateArgumentEndpoints;
        public List<EndpointComponent> ArgumentEndpoints;

        public EndpointComponent EndpointPrefab;
        public RectTransform TemplateArguments;
        public RectTransform Arguments;

        public FunctionCallNode _node;

        public override INode NodeTemplate => new FunctionCallNode();

        public override INode Node => _node;

        public override void ParticularInit(INode node)
        {
            _node = node as FunctionCallNode;

            LeftFlowEndpoint.Endpoint = _node.LeftFlowEndpoint;
            RightFlowEndpoin.Endpoint = _node.RightFlowEndpoint;
            FunctionEndpoint.Endpoint = _node.FunctionEndpoint;
            OutputEndpoint.Endpoint = _node.OutputEndpoint;

            foreach (var endpoint in _node.TemplateArgumentEndpoints) {
                AddArgumentGeneric(endpoint, TemplateArguments, null);
            }

            foreach (var endpoint in _node.ArgumentEndpoints) {
                AddArgumentGeneric(endpoint, Arguments, null);
            }
        }

        private void AddArgumentGeneric(Endpoint endpoint, RectTransform parentTransform, List<Endpoint> nodeEndPoints)
        {
            var endpointComponent = Instantiate(EndpointPrefab, parentTransform);
            if (endpoint == null)
            {
                endpoint = new Endpoint();
                nodeEndPoints.Add(endpoint);
            }
            endpointComponent.Endpoint = endpoint;
        }

        private void RemoveArgumentGeneric(RectTransform parentTransform, List<Endpoint> nodeEndPoints)
        {
            if (parentTransform.childCount == 0) {
                return;
            }
            var endpointComponent = parentTransform.GetChild(parentTransform.childCount - 1).GetComponent<EndpointComponent>();
            endpointComponent.UnLinkAll();

            Destroy(parentTransform.GetChild(parentTransform.childCount - 1).gameObject);
            nodeEndPoints.RemoveAt(nodeEndPoints.Count - 1);
        }

        public void AddTemplateArg()
        {
            AddArgumentGeneric(null, TemplateArguments, _node.TemplateArgumentEndpoints);
        }
        public void RemoveTemplateArg()
        {
            RemoveArgumentGeneric(TemplateArguments, _node.TemplateArgumentEndpoints);
        }

        public void AddArg()
        {
            AddArgumentGeneric(null, Arguments, _node.ArgumentEndpoints);
        }
        public void RemoveArg()
        {
            RemoveArgumentGeneric(Arguments, _node.ArgumentEndpoints);
        }
    }
}
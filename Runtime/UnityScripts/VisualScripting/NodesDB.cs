using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptingLanguage.VisualScripting
{
    [Serializable]
    public class NodesDB
    {
        [NonSerialized]
        private HashSet<INode> __nodes;
        private HashSet<INode> _nodes 
        {
            get 
            {
                if (__nodes == null) {
                    __nodes = new HashSet<INode>();
                }
                return __nodes;
            }
        }

        [Serializable]
        public class NodeWithPosition 
        {
            public INode Node;
            public float x;
            public float y;
        }

        public NodeWithPosition[] NodesWithPositions;

        public void AddNode(INode node)
        {
            _nodes.Add(node);
        }

        public void RemoveNode(INode node)
        {
            _nodes.Remove(node);
        }

        public IEnumerable<INode> GetAllNodes()
        {
            return _nodes;
        }

        public IEnumerable<Endpoint> GetEndpoints()
        {
            return _nodes.SelectMany(x => x.Endpoints);
        }

        public INode GetNodeByEndpoint(Endpoint endpoint)
        {
            return _nodes.FirstOrDefault(node => node.Endpoints.Contains(endpoint));
        }
    }
}
using UnityEngine;

namespace ScriptingLanguage.VisualScripting
{
    public abstract class NodeComponent : MonoBehaviour
    {
        public void Init(INode node) 
        {
            var frame = GetComponentInParent<Frame>();
            frame.NodesDB.AddNode(node);
            ParticularInit(node);
        }

        public void DeInit()
        {
            var frame = GetComponentInParent<Frame>();
            frame.NodesDB.RemoveNode(Node);
            var endpoints = GetComponentsInChildren<EndpointComponent>();

            foreach (var endpoint in endpoints)
            {
                endpoint.UnLinkAll();
            }
        }

        public abstract void ParticularInit(INode node);
        public abstract INode NodeTemplate { get; }
        public abstract INode Node { get; }
    }
}
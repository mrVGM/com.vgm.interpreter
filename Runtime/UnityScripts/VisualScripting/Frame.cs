using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using static ScriptingLanguage.VisualScripting.NodesDB;

namespace ScriptingLanguage.VisualScripting
{
    public class Frame : MonoBehaviour
    {
        public Link LinkPrefab;
        public RectTransform NodesContainer;
        public RectTransform LinksContainer;
        public RectTransform TemplatesContainer;
        public VisualScriptingSession VisualScriptingSession;

        private readonly HashSet<MoveHandle> _selected = new HashSet<MoveHandle>();
        public IEnumerable<MoveHandle> Selected => _selected;

        public NodesDB NodesDB = new NodesDB();

        public InputField Filename;

        public void Select(MoveHandle moveHandle)
        {
            var highlight = moveHandle.NodeComponent.gameObject.GetComponent<NodeHighlight>();
            if (highlight != null) {
                highlight.Highlight(true);
            }
            _selected.Add(moveHandle);
        }

        public void Select(IEnumerable<MoveHandle> moveHandles)
        {
            foreach (var moveHandle in moveHandles) {
                Select(moveHandle);
            }
        }

        public void UnselectAll()
        {
            var highlights = _selected.Select(x => x.NodeComponent.GetComponent<NodeHighlight>());
            foreach (var highlight in highlights) {
                highlight.Highlight(false);
            }
            _selected.Clear();
        }

        public void ClearWorkspace() 
        {
            UnselectAll();
            var go = new GameObject();
            go.SetActive(false);

            while (NodesContainer.childCount > 0) {
                var child = NodesContainer.GetChild(0);
                child.SetParent(go.transform);
            }

            while (LinksContainer.childCount > 0) {
                var child = LinksContainer.GetChild(0);
                child.SetParent(go.transform);
            }

            Destroy(go);
        }

        private NodeComponent RestoreNode(NodeWithPosition nodeWithPosition)
        {
            var nodeTarget = nodeWithPosition.Node.GetType()
                .GetCustomAttributes(false).OfType<NodeTargetAttribute>().FirstOrDefault();
            var targetType = nodeTarget.Type;

            var templates = TemplatesContainer.GetComponentsInChildren<NodeTemplate>().Select(x => x.NodePrefab);
            var prefab = templates.FirstOrDefault(template => {
                var nodeComponent = template.GetComponent<NodeComponent>();
                return targetType.IsAssignableFrom(nodeComponent.GetType());
            });

            var node = Instantiate(prefab, NodesContainer);
            var rect = node.GetComponent<RectTransform>();
            rect.anchoredPosition = nodeWithPosition.x * Vector2.right + nodeWithPosition.y * Vector2.up;

            node.Init(nodeWithPosition.Node);
            return node;
        }

        private void FlushDB() 
        {
            var nodes = NodesContainer.GetComponentsInChildren<NodeComponent>();
            NodesDB.NodesWithPositions = nodes.Select(x => {
                var rectTransform = x.GetComponent<RectTransform>();
                var tmp = new NodeWithPosition {
                    Node = x.Node,
                    x = rectTransform.anchoredPosition.x,
                    y = rectTransform.anchoredPosition.y,
                };
                return tmp;
            }).ToArray();
        }

        private IEnumerable<NodeComponent> RestoreNodes(NodesDB nodesDB) 
        {
            List<EndpointComponent> endPointComponents = new List<EndpointComponent>();
            var nodeComponents = new List<NodeComponent>();
            foreach (var node in nodesDB.NodesWithPositions) {
                var nodeComponent = RestoreNode(node);
                nodeComponents.Add(nodeComponent);
                endPointComponents.AddRange(nodeComponent.GetComponentsInChildren<EndpointComponent>());
            }

            var allEndpoints = nodesDB.GetEndpoints();

            foreach (var endpoint in allEndpoints) {
                endpoint.RestoreLinkedEndpoints(allEndpoints);
            }

            HashSet<EndpointComponent> visited = new HashSet<EndpointComponent>();
            foreach (var ednpointComponent in endPointComponents) {
                var curEndpoint = ednpointComponent.Endpoint;

                foreach (var linkedEndPoint in curEndpoint.LinkedEndpoints) {
                    var otherEndpoint = endPointComponents.FirstOrDefault(x => x.Endpoint.Guid == linkedEndPoint.Guid);
                    if (!visited.Contains(otherEndpoint)) {
                        EndpointComponent.LinkVisually(ednpointComponent, otherEndpoint, LinkPrefab, LinksContainer);
                    }
                }
                visited.Add(ednpointComponent);
            }
            return nodeComponents;
        }

        public void SaveWorkspace() 
        {
            FlushDB();

            string workingDir = VisualScriptingSession.GetWorkingDir();
            string filename = workingDir + Filename.text;
            if (string.IsNullOrWhiteSpace(filename)) {
                return;
            }
            FileStream fs = File.Open(Filename.text, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, NodesDB);
            fs.Close();
        }
        public void LoadWorkspace() 
        {
            string workingDir = VisualScriptingSession.GetWorkingDir();
            string filename = workingDir + Filename.text;
            if (!File.Exists(filename))
            {
                return;
            }
            FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read);
            BinaryFormatter bf = new BinaryFormatter();
            NodesDB = bf.Deserialize(fs) as NodesDB;
            fs.Close();
            RestoreNodes(NodesDB);
        }

        public void ResetSession()
        {
            VisualScriptingSession.ResetSession(Filename.text);
        }

        public IEnumerable<NodeComponent> CopyNodes(IEnumerable<INode> nodes)
        {
            FlushDB();
            var bf = new BinaryFormatter();
            NodesDB clone = null;
            using (MemoryStream ms = new MemoryStream()) {
                bf.Serialize(ms, NodesDB);
                ms.Flush();
                ms.Position = 0;
                clone = bf.Deserialize(ms) as NodesDB;
            }

            nodes = clone.NodesWithPositions.Select(x => x.Node).Where(node => {
                var endpointID = node.Endpoints.First().Guid;
                return nodes.Any(x => x.Endpoints.Any(y => y.Guid == endpointID));
            }).ToList();

            var allEndpoints = clone.NodesWithPositions.Select(x => x.Node).SelectMany(x => x.Endpoints).Distinct().ToList();
            Endpoint.RemapIds(allEndpoints);

            foreach (var endpoint in allEndpoints) {
                endpoint.RestoreLinkedEndpoints(allEndpoints);
            }

            var skippedNodes = clone.NodesWithPositions.Select(x => x.Node).Except(nodes);
            foreach (var skippedNode in skippedNodes) {
                foreach (var endpoint in skippedNode.Endpoints) {
                    foreach (var link in endpoint.LinkedEndpoints.ToList()) {
                        Endpoint.UnLink(endpoint, link);
                    }
                }
            }

            clone.NodesWithPositions = clone.NodesWithPositions.Where(x => nodes.Contains(x.Node)).ToArray();
            var newNodes = RestoreNodes(clone);
            return newNodes;
        }
    }
}
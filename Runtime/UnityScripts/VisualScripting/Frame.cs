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
        public SessionHolder SessionHolder;

        public NodesDB NodesDB = new NodesDB();

        public InputField Filename;

        public void ClearWorkspace() 
        {
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

        private void RestoreNodes() 
        {
            List<EndpointComponent> endPointComponents = new List<EndpointComponent>();
            foreach (var node in NodesDB.NodesWithPositions) {
                var nodeComponent = RestoreNode(node);
                endPointComponents.AddRange(nodeComponent.GetComponentsInChildren<EndpointComponent>());
            }

            var allEndpoints = NodesDB.GetEndpoints();

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
        }

        public void SaveWorkspace() 
        {
            FlushDB();

            string filename = Filename.text;
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
            string filename = Filename.text;
            if (!File.Exists(filename))
            {
                return;
            }
            FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read);
            BinaryFormatter bf = new BinaryFormatter();
            NodesDB = bf.Deserialize(fs) as NodesDB;
            fs.Close();
            RestoreNodes();
        }
    }
}
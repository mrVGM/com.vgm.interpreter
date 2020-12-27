using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class PropertiesNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(PropertiesNodeComponent))]
        public class PropertiesNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return ObjectEndpoint;
                    yield return ResultEndpoint;
                }
            }
            public Endpoint ObjectEndpoint = new Endpoint();
            public Endpoint ResultEndpoint = new Endpoint();
            public List<string> Properties = new List<string>();

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                if (endpoint != ResultEndpoint) {
                    throw new InvalidOperationException();
                }

                string str = "";
                foreach (var prop in Properties)
                {
                    str += $".{prop}";
                }

                var objectEndpoint = ObjectEndpoint.LinkedEndpoints.FirstOrDefault();
                if (objectEndpoint == null) {
                    str = str.Substring(1);
                    return str;
                }

                var objectEndpointNode = nodesDB.GetNodeByEndpoint(objectEndpoint);
                string code;
                using (context.CreateTemporaryCustomContext(null)) {
                    code = objectEndpointNode.GenerateCode(objectEndpoint, nodesDB, context);
                }
                
                return $"{code}{str}";
            }
        }
        public EndpointComponent ObjectEndpoint;
        public EndpointComponent ResultEndpoint;
        public Button PropertyTemplate;

        public PropertiesNode _node;

        public override INode NodeTemplate => new PropertiesNode();

        public override INode Node => _node;

        public override void ParticularInit(INode node)
        {
            _node = node as PropertiesNode;

            ObjectEndpoint.Endpoint = _node.ObjectEndpoint;
            ResultEndpoint.Endpoint = _node.ResultEndpoint;
            foreach (var prop in _node.Properties) {
                AddPropertyTemplate(prop);
            }
        }

        public void AddPropertyTemplate() 
        {
            AddPropertyTemplate(null);
        }

        private void AddPropertyTemplate(string propertyName) 
        {
            var parentTransform = PropertyTemplate.transform.parent;
            var property = Instantiate(PropertyTemplate, parentTransform);
            property.gameObject.SetActive(true);
            var text = property.GetComponent<Text>();
            var inputField = property.GetComponentInChildren<InputField>(true);

            if (string.IsNullOrWhiteSpace(propertyName)) {
                _node.Properties.Add(text.text);
            } else {
                text.text = propertyName;
            }

            inputField.onEndEdit.AddListener(str => {
                if (str.Contains(' ')) {
                    return;
                }
                text.text = str;
                inputField.gameObject.SetActive(false);
                int index = 0;
                for (int i = 0; i < parentTransform.childCount; ++i) {
                    if (parentTransform.GetChild(i) == property.transform) {
                        index = i;
                        break;
                    }
                }
                _node.Properties[index - 1] = str;
            });

            property.onClick.AddListener(() => {
                if (inputField.gameObject.activeSelf) {
                    return;
                }

                inputField.gameObject.SetActive(true);
                inputField.text = text.text;
                inputField.ActivateInputField();
            });
        }
        public void RemovePropertyTemplate()
        {
            var parentTransform = PropertyTemplate.transform.parent;
            int childCount = parentTransform.transform.childCount;
            if (childCount > 1) {
                Destroy(parentTransform.GetChild(childCount - 1).gameObject);
                _node.Properties.RemoveAt(_node.Properties.Count - 1);
            }
        }
    }
}
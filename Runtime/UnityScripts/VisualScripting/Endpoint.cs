using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptingLanguage.VisualScripting
{
    [Serializable]
    public class Endpoint
    {
        public Guid Guid = Guid.NewGuid();

        [NonSerialized]
        private List<Endpoint> linkedEndpoints = new List<Endpoint>();
        private List<Guid> _linkedEndpointIds = new List<Guid>();

        public IEnumerable<Endpoint> LinkedEndpoints => linkedEndpoints;

        public void RestoreLinkedEndpoints(IEnumerable<Endpoint> allEndpoints)
        {
            linkedEndpoints = new List<Endpoint>();
            foreach (var id in _linkedEndpointIds) {
                var endpoint = allEndpoints.FirstOrDefault(x => x.Guid == id);
                linkedEndpoints.Add(endpoint);
            }
        }

        public static void Link(Endpoint endpoint1, Endpoint endpoint2)
        {
            if (endpoint2.linkedEndpoints.Contains(endpoint1) || endpoint1.linkedEndpoints.Contains(endpoint2)) {
                throw new InvalidOperationException();
            }

            endpoint1.linkedEndpoints.Add(endpoint2);
            endpoint2.linkedEndpoints.Add(endpoint1);

            endpoint1._linkedEndpointIds.Add(endpoint2.Guid);
            endpoint2._linkedEndpointIds.Add(endpoint1.Guid);
        }

        public static void UnLink(Endpoint endpoint1, Endpoint endpoint2)
        {
            endpoint1.linkedEndpoints.Remove(endpoint2);
            endpoint2.linkedEndpoints.Remove(endpoint1);

            endpoint1._linkedEndpointIds.Remove(endpoint2.Guid);
            endpoint2._linkedEndpointIds.Remove(endpoint1.Guid);
        }

        public static void RemapIds(IEnumerable<Endpoint> allEndpoints)
        {
            Dictionary<Guid, Guid> idMap = new Dictionary<Guid, Guid>();
            foreach (var endpoint in allEndpoints) {
                idMap[endpoint.Guid] = Guid.NewGuid();
            }

            foreach (var endpoint in allEndpoints) {
                endpoint.Guid = idMap[endpoint.Guid];
                for (int i = 0; i < endpoint._linkedEndpointIds.Count; ++i) {
                    endpoint._linkedEndpointIds[i] = idMap[endpoint._linkedEndpointIds[i]];
                }
            }
        }
    }
}
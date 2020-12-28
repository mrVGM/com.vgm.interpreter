using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class EndpointComponent : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [Serializable]
        private class LinkData
        {
            public EndpointComponent OtherComponent;
            public Link Link;
        }

        private Frame _frame => GetComponentInParent<Frame>();
        [SerializeField]
        private string Guid;
        private Endpoint _endpoint;
        public Endpoint Endpoint 
        {
            get 
            {
                return _endpoint;
            }
            set
            {
                _endpoint = value;
                Guid = _endpoint.Guid.ToString();
            }
        }

        [SerializeField]
        private List<LinkData> LinksData;

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void UnLinkAll() 
        {
            while (LinksData.Any()) {
                var linkData = LinksData.FirstOrDefault();
                VisuallyUnlink(this, linkData.OtherComponent);
                UnLink(this, linkData.OtherComponent);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerEnter == null) {
                return;
            }
            var endpoint = eventData.pointerEnter.GetComponent<EndpointComponent>();
            if (endpoint == null) {
                return;
            }

            Link(this, endpoint);
            LinkVisually(this, endpoint, _frame.LinkPrefab, _frame.LinksContainer);
        }
        public static void LinkVisually(EndpointComponent endpointComponent1, EndpointComponent endpointComponent2, Link linkPrefab, RectTransform linksContainer)
        {
            var link = Instantiate(linkPrefab, linksContainer);
            link.Endpoint1 = endpointComponent1.GetComponent<RectTransform>();
            link.Endpoint2 = endpointComponent2.GetComponent<RectTransform>();
            endpointComponent1.LinksData.Add(new LinkData { Link = link, OtherComponent = endpointComponent2 });
            endpointComponent2.LinksData.Add(new LinkData { Link = link, OtherComponent = endpointComponent1 });
        }
        public static void Link(EndpointComponent endpointComponent1, EndpointComponent endpointComponent2)
        {
            Endpoint.Link(endpointComponent1.Endpoint, endpointComponent2.Endpoint);
        }

        public static void VisuallyUnlink(EndpointComponent endpointComponent1, EndpointComponent endpointComponent2)
        {
            var linkData = endpointComponent1.LinksData.FirstOrDefault(x => x.OtherComponent == endpointComponent2);
            if (linkData != null) {
                Destroy(linkData.Link.gameObject);
                endpointComponent1.LinksData.Remove(linkData);
                endpointComponent2.LinksData.RemoveAll(x => x.OtherComponent == endpointComponent1);
            }
        }

        public static void UnLink(EndpointComponent endpointComponent1, EndpointComponent endpointComponent2)
        {
            Endpoint.UnLink(endpointComponent1.Endpoint, endpointComponent2.Endpoint);
        }
    }
}
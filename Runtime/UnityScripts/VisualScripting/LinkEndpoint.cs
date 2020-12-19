using UnityEngine;

namespace ScriptingLanguage.VisualScripting
{
    public class LinkEndpoint : MonoBehaviour
    {
        public Knob Knob;
        public LinkEndpoint GetCounterpart()
        {
            var link = GetLink();
            if (link.LinkEndpoint1 == this) {
                return link.LinkEndpoint2;
            }
            if (link.LinkEndpoint2 == this) {
                return link.LinkEndpoint1;
            }
            return null;
        }

        public Link GetLink()
        {
            return GetComponentInParent<Link>();
        }
    }
}

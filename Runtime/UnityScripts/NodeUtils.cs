using System.Linq;
using UnityEngine;

namespace ScriptingLanguage.VisualScripting
{
    public static class NodeUtils
    {
        public static void Destroy(IKnobOwner knobOwner)
        {
            if (knobOwner == null) {
                return;
            }

            var linksToDestroy = knobOwner.SelectMany(x => x.LinkEndpoints)
                .Select(x => x.GetLink()).Distinct();
            foreach (var link in linksToDestroy.ToList()) {
                link.DestroyLink();
            }

            var go = (knobOwner as MonoBehaviour).gameObject;
            Object.Destroy(go);
        }
    }
}

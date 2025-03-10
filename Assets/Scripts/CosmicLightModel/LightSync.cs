using UnityEngine;

namespace Wanderer
{
    public class LightSync : MonoBehaviour
    {
        public Transform targetTransform;
        public Transform sourceTransform;

        void Update()
        {
            transform.rotation = Quaternion.LookRotation(targetTransform.position - sourceTransform.position);
            Light l = GetComponent<Light>();
            l.range = (targetTransform.position - sourceTransform.position).magnitude * 2f;
        }
    }
}
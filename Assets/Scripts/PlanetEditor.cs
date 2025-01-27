using UnityEngine;
#if UNITY_EDITOR
namespace Wanderer
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class PlanetEditor : MonoBehaviour
    {
        public PlanetSettings settings;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void OnEnable()
        {
            
        }

        void OnDisable()
        {

        }

        void OnValidate()
        {
            OnDisable();
            OnEnable();
        }
    }
}
#endif
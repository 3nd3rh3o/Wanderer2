using UnityEngine;

namespace Wanderer
{
    
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class PlanetEditor : MonoBehaviour
    {
        public PlanetSettings settings;
        private TeluricGenerator surfGenerator;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        void OnEnable()
        {   
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshFilter.sharedMesh == null) meshFilter.sharedMesh = new();
            surfGenerator = new(settings);
            surfGenerator.Build(meshFilter, meshRenderer);
        }

        void Update()
        {
            surfGenerator.Regen(transform.position, meshFilter, meshRenderer);
            surfGenerator.Build(meshFilter, meshRenderer);
        }
        void OnDisable()
        {
            meshFilter.sharedMesh.Clear();
            surfGenerator?.Clear();
        }
    }
}

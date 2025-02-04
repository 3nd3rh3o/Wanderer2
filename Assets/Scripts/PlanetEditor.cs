using UnityEngine;

namespace Wanderer
{
    
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class PlanetEditor : MonoBehaviour
    {
        public PlanetSettings settings;
        public bool DynamicParams = true;
        private TeluricGenerator surfGenerator;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshFilter.sharedMesh == null) meshFilter.sharedMesh = new();
            surfGenerator = new(settings);
            surfGenerator.Build(meshFilter, meshRenderer);
        }

        void OnEnable()
        {
            if (settings == null) return;
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshFilter.sharedMesh == null) meshFilter.sharedMesh = new();
            surfGenerator = new(settings);
            surfGenerator.Build(meshFilter, meshRenderer);
        }

        void Update()
        {
            if (surfGenerator == null || settings == null) OnEnable();
            if (DynamicParams) surfGenerator?.Regen(transform.position, meshFilter, meshRenderer);
            else surfGenerator?.Update(transform.position, meshFilter, meshRenderer);
            surfGenerator?.Build(meshFilter, meshRenderer);
        }
        void OnDisable()
        {
            meshFilter.sharedMesh.Clear();
            surfGenerator?.Clear();
        }
    }
}

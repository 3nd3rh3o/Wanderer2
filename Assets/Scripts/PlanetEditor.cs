using UnityEngine;

namespace Wanderer
{
    
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Rigidbody))]
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

        void OnDrawGizmos()
        {
            Vector3 basePos = Camera.current.ScreenToWorldPoint(new Vector3(Screen.width*0.1f, Screen.height * 0.1f, 40f));
            
            for (int i = 0; i < settings.biomes.biomes.Length; i++)
            {
                BiomeSetting b = settings.biomes.biomes[i];
                float x = b.MaxPredicate.Altitude - b.MinPredicate.Altitude;
                float y = b.MaxPredicate.Humidity - b.MinPredicate.Humidity;
                float z = b.MaxPredicate.Temperature - b.MinPredicate.Temperature;
                Vector3 size = new Vector3(x, y, z);
                Vector3 pos = new(b.MinPredicate.Altitude + (0.5f * x), b.MinPredicate.Humidity + (0.5f * y), b.MinPredicate.Temperature + (0.5f * z));
                Color color = b.TopologySettings.DebugColor;
                Gizmos.color = new(color.r, color.g, color.b, 0.5f);
                Gizmos.DrawCube(basePos + pos, size);
            }        
        }
    }
}

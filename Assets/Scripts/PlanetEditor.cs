using System.Linq;
using UnityEngine;
using static Wanderer.TeluricGenerator;
#if UNITY_EDITOR
namespace Wanderer
{
    
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class PlanetEditor : MonoBehaviour
    {
        public ComputeShader cs;
        public PlanetSettings settings;
        public TeluricGenerator surfGenerator;
        public Material TerrainMat;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        void OnEnable()
        {   
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshFilter.sharedMesh == null) meshFilter.sharedMesh = new();
            surfGenerator = new(settings, cs);
            surfGenerator.Build(meshFilter, TerrainMat, meshRenderer);
        }

        void Update()
        {
            surfGenerator.Regen();
            surfGenerator.Build(meshFilter, TerrainMat, meshRenderer);
        }


        void OnDisable()
        {
            meshFilter.sharedMesh.Clear();
            surfGenerator?.Clear();
        }
    }
}
#endif
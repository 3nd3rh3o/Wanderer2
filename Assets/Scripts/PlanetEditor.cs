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
        public PlanetSettings settings;
        public TeluricGenerator surfGenerator;
        public Material TerrainMat;

        void OnEnable()
        {
            surfGenerator = new(settings);
            surfGenerator.Build(GetComponent<MeshFilter>(), TerrainMat, GetComponent<MeshRenderer>());
        }

        void Update()
        {
            
        }

        void OnValidate()
        {
            surfGenerator?.Build(GetComponent<MeshFilter>(), TerrainMat, GetComponent<MeshRenderer>());
        }

        void OnDisable()
        {
            surfGenerator?.Clear();
            surfGenerator = null;
        }
    }
}
#endif
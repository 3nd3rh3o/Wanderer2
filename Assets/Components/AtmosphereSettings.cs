using UnityEngine;
namespace Wanderer
{

    [CreateAssetMenu(fileName = "AtmosphereSettings", menuName = "Wanderer/Planet/AtmosphereSettings")]
    public class AtmosphereSettings : ScriptableObject
    {
        public float AtmosphereOffset;
        [Range(0f, 1f)] public float AtmosphereSize;
        public Vector3 ScatteringCoefficients;
        public float ScatteringStrength;
        public float DensityFalloff;
    }
}
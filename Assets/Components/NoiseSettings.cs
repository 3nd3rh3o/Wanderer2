using UnityEngine;
namespace Wanderer
{
    [CreateAssetMenu(fileName = "NoiseSettings", menuName = "Wanderer/Planet/NoiseSettings")]
    public class NoiseSettings : ScriptableObject
    {
        [Range(0f, 2f)] public float Scale;
        [Range(0f, 2f)] public float Multiplier;
        public float VerticalShift;
        public Vector3 Offset;
        public int NumLayers;
        public float Lacunarity;
        public Color DebugColor;
    }
}

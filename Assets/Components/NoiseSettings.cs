using UnityEngine;
namespace Wanderer
{
    [CreateAssetMenu(fileName = "NoiseSettings", menuName = "Wanderer/Planet/NoiseSettings")]
    public class NoiseSettings : ScriptableObject
    {
        [Range(0f, 10f)] public float Scale;
        [Range(0f, 2f)] public float Multiplier;
        public float VerticalShift;
        public Vector3 Offset;
        [Range(0, 10)] public int NumLayers;
        [Range(-1f, 1f)] public float Lacunarity;
        [Range(-1f, 1f)] public float Persistence;
        public Color DebugColor;
    }
}

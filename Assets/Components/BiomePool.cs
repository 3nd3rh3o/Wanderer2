using System.Collections.Generic;
using UnityEngine;
namespace Wanderer
{

    [CreateAssetMenu(fileName = "BiomePool", menuName = "Wanderer/Planet/BiomePool")]
    public class BiomePool : ScriptableObject
    {
        [Range(0f, 2f)] public float Scale;
        [Range(0f, 2f)] public float Multiplier;
        public Vector3 Offset;
        [Range(0, 10)] public int MaxLOD;
        public BiomeSetting[] biomes;
    }
}
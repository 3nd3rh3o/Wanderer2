using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
namespace Wanderer
{

    [CreateAssetMenu(fileName = "BiomePool", menuName = "Wanderer/Planet/BiomePool")]
    public class BiomePool : ScriptableObject
    {
        [Range(0f, 2f)] public float Scale;
        public Vector3 Offset;
        [Range(0, 10)] public int MaxLOD;
        public BiomeSetting[] biomes;

        internal float4[] CollectMinPreds()
        {
            float4[] b = new float4[biomes.Length];
            for (int i = 0; i < biomes.Length; i++) b[i] = new float4(biomes[i].GetMinPreds());
            return b;
        }
        internal float4[] CollectMaxPreds()
        {
            float4[] b = new float4[biomes.Length];
            for (int i = 0; i < biomes.Length; i++) b[i] = new float4(biomes[i].GetMaxPreds());
            return b;
        }

        internal float[] CollectBlendingFactors()
        {
            float[] b = new float[biomes.Length];
            for (int i = 0; i < biomes.Length; i++) b[i] = biomes[i].GetBlendingFactor();
            return b;
        }

        internal float[] CollectBiomeScales()
        {
            float[] b = new float[biomes.Length];
            for (int i = 0; i < biomes.Length; i++) b[i] = biomes[i].GetScale();
            return b;
        }
    }
}
using System;
using Unity.Mathematics;
using UnityEngine;
namespace Wanderer
{
    [CreateAssetMenu(fileName = "BiomeSetting", menuName = "Wanderer/Planet/BiomeSetting")]
    public class BiomeSetting : ScriptableObject
    {
        public string Name;
        public BiomePredicate MinPredicate;
        public BiomePredicate MaxPredicate;
        public float BlendingFactor;
        public NoiseSettings TopologySettings;

        internal float4 GetMinPreds()
        {
            return new float4(MinPredicate.Altitude, MinPredicate.Temperature, MinPredicate.Humidity, MinPredicate.Latitude);
        }

        internal float4 GetMaxPreds()
        {
            return new float4(MaxPredicate.Altitude, MaxPredicate.Temperature, MaxPredicate.Humidity, MaxPredicate.Latitude);
        }

        internal float GetScale()
        {
            return TopologySettings.Scale;
        }

        internal float GetMultiplier()
        {
            return TopologySettings.Multiplier;
        }

        internal float GetBlendingFactor()
        {
            return BlendingFactor;
        }
    }
}
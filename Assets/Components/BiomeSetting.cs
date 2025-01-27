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
        public NoiseSettings TopologySettings;

        internal float4 GetMinPreds()
        {
            return new float4(MinPredicate.Altitude, MinPredicate.Temperature, MinPredicate.Humidity, MinPredicate.Latitude);
        }

        internal float4 GetMaxPreds()
        {
            return new float4(MaxPredicate.Altitude, MaxPredicate.Temperature, MaxPredicate.Humidity, MaxPredicate.Latitude);
        }

        internal float4x4 GetGenParams()
        {
            float4x4 res = new();
            res[0][0] = TopologySettings.Scale;
            res[0][1] = TopologySettings.Multiplier;
            res[0][2] = TopologySettings.Offset.x;
            res[0][3] = TopologySettings.Offset.y;
            res[1][0] = TopologySettings.Offset.z;
            res[1][1] = TopologySettings.VerticalShift;
            res[1][2] = TopologySettings.Lacunarity;
            return res;
        }

        
    }
}
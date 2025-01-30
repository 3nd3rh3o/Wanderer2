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

        internal float GetNumLayers()
        {
            return TopologySettings.NumLayers;
        }

        internal Vector3 GetOffset()
        {
            return TopologySettings.Offset;
        }

        internal float GetPersistence()
        {
            return TopologySettings.Persistence;
        }

        internal float GetLacunarity()
        {
            return TopologySettings.Lacunarity;
        }

        internal float GetVShift()
        {
            return TopologySettings.VShift;
        }

        internal Vector3 GetDebugCol()
        {
            return new Vector3(TopologySettings.DebugColor.r, TopologySettings.DebugColor.g, TopologySettings.DebugColor.b);
        }
    }
}
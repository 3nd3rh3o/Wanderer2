using System;
using UnityEngine;
namespace Wanderer
{
    [CreateAssetMenu(fileName = "TexBuildingPass", menuName = "Wanderer/Planet/TexBuildingPass")]
    public class TexBuildingPass : ScriptableObject
    {
        public readonly PassType passType;


        [Serializable]
        public enum PassType
        {
            Fill,
            FractalNoise
        }
    }
}
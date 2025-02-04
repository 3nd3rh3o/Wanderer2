using UnityEngine;
namespace Wanderer
{
    [CreateAssetMenu(fileName = "TexBuildingPassSimplex", menuName = "Wanderer/Planet/TexBuildingPass/Simplex")]
    public class TexBuildingPassSimplex : TexBuildingPass
    {
        public Color mainColor;
        public Color secondaryColor;
        public float _Scale;
        public float _Multiplier;
        public int _NumLayers;
        public float _Lacunarity;
        public float _Persistence;
        public float _VerticalShift;
        public Vector3 _Offset;
    }
}
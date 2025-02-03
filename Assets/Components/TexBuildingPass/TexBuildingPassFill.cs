using UnityEngine;
namespace Wanderer
{
    [CreateAssetMenu(fileName = "TexBuildingPassFill", menuName = "Wanderer/Planet/TexBuildingPassFill")]
    public class TexBuildingPassFill : TexBuildingPass
    {
        public readonly new PassType passType = PassType.Fill;

        public Color color;
    }
}
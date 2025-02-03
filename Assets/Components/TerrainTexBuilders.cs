using UnityEngine;
namespace Wanderer
{
    [CreateAssetMenu(fileName = "TerrainTexBuilders", menuName = "Wanderer/Planet/TerrainTexBuilders")]
    public class TerrainTexBuilders : ScriptableObject
    {
        public TerrainTexBuilder baseTexture;
    }
}
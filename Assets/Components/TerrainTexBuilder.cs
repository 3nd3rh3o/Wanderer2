using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wanderer
{
    [CreateAssetMenu(fileName = "TerrainTexBuilder", menuName = "Wanderer/Planet/TerrainTexBuilder")]
    public class TerrainTexBuilder : ScriptableObject
    {
        public List<TexBuildingPass> BuildingPass;
    }
}
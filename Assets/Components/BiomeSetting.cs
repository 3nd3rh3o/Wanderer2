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
    }
}
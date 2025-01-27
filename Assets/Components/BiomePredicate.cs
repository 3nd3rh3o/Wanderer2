using UnityEngine;

namespace Wanderer
{
    [CreateAssetMenu(fileName = "BiomePredicate", menuName = "Wanderer/Planet/BiomePredicate")]
    public class BiomePredicate : ScriptableObject
    {
        [Range(-1f, 1f)] public float Altitude;
        [Range(-1f, 1f)] public float Temperature;
        [Range(-1f, 1f)] public float Humidity;
        [Range(-1f, 1f)] public float Latitude;

    }
}
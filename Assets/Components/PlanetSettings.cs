using UnityEngine;

[CreateAssetMenu(fileName = "PlanetSettings", menuName = "Wanderer/Planet/PlanetSettings")]
public class PlanetSettings : ScriptableObject
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 linearVelocity;
    public Vector3 angularVelocity;
    public float radius;
    public float mass;
    public bool isKynematic;

    public AtmosphereSettings atmosphereSettings;
}

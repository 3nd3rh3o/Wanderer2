using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SolarSystemsData : MonoBehaviour
{
    [SerializeField]
    private List<SolarSystemData> data = new();


    public SolarSystemData GetCurrentFromPPos(Vector3 position)
    {
        SolarSystemData result = data[0];
        float min = data[0].DistanceFrom(position);
        foreach (SolarSystemData system in data)
        {
            if (system.DistanceFrom(position) < min)
            {
                min = system.DistanceFrom(position);
                result = system;
            }
        }
        return result;
    }
     
}
[Serializable]
public class SolarSystemData
{
    [SerializeField]
    private string name;
    [SerializeField]
    private Vector3 position;
    [SerializeField]
    private Vector3 orientationE;
    [SerializeField]
    private List<MajorCelestialBodyData> MCB = new();

    public Vector3 GetPosition()
    {
        return position;
    }
    public Vector3 GetOrientation()
    {
        return orientationE;
    }
    public string GetName()
    {
        return name;
    }
    public float DistanceFrom(Vector3 position)
    {
        return (this.position - position).magnitude;
    }

    public List<MajorCelestialBodyData> GetPlanets()
    {
        return MCB;
    }
}
[Serializable]
public enum MCBType
{
    STAR,
    TELURIC_PLANET,
    GAZEOUS_PLANET
}
[Serializable]
public class MajorCelestialBodyData : MajorCelestialBodyMoonData
{
    [SerializeField]
    private List<MajorCelestialBodyMoonData> moons = new();
}

[Serializable]
public class MajorCelestialBodyMoonData
{
    [SerializeField]
    private string name;
    [SerializeField]
    private MCBType type;
    [SerializeField]
    private Vector3 position;
    [SerializeField]
    private Vector3 initialVelocity;
    [SerializeField]
    private Vector3 initialTorque;
    [SerializeField]
    private float radius;
    [SerializeField]
    private float mass;
    [SerializeField]
    private Vector3 orientationE;
    [SerializeField]
    private bool isKynematic;
    [SerializeField]
    private bool hasAtmo;
    [SerializeField]
    private AtmoData atmoData;
    [SerializeField]
    private Vector2 BiomeScaleAndMultiplier;
    [SerializeField]
    private Vector3 BiomeOffset;
    [SerializeField]
    private int MaxLOD;
    [SerializeField]
    private List<Biome> biomes;

    internal string GetName()
    {
        return name;
    }

    internal float GetRadius()
    {
        return radius;
    }
    internal MCBType GetMCBType()
    {
        return type;
    }

    internal float GetMass()
    {
        return mass;
    }
    internal Vector3 GetInitialVelocity()
    {
        return initialVelocity;
    }
    internal Vector3 GetInitialPosition()
    {
        return position;
    }
    internal Vector3 GetInitialTorque()
    {
        return initialTorque;
    }
    internal Vector3 GetInitialOrientation()
    {
        return orientationE;
    }
    internal bool IsKynematic()
    {
        return isKynematic;
    }
    internal bool HasAtmosphere()
    {
        return hasAtmo;
    }

    internal AtmoData GetAtmoData()
    {
        return atmoData;
    }

    internal float GetBiomeScale()
    {
        return BiomeScaleAndMultiplier.x;
    }
    internal float GetBiomeMul()
    {
        return BiomeScaleAndMultiplier.y;
    }
    internal Vector3 GetBiomeOffset()
    {
        return BiomeOffset;
    }
    internal List<Biome> GetBiomes()
    {
        return biomes;
    }
    internal int GetMLOD()
    {
        return MaxLOD;
    }
}
[Serializable]
public class AtmoData
{
    [SerializeField]
    public Vector3 _lightDirection;
    [SerializeField]
    public Vector3 _ScatteringCoefficients;
    [SerializeField]
    public Vector3 _PlanetPosition;
    [SerializeField]
    [Range(0f, 1f)]
    public float _AtmosphereRadius;
    [SerializeField]
    public float _PlanetRadius;
    public float _DensityFalloff;
    public float _ScatteringStrenght;
}
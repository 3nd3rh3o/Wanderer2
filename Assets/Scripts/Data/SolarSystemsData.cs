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
    private List<PlanetData> planets = new();

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

    public List<PlanetData> GetPlanets()
    {
        return planets;
    }
}
[Serializable]
public class PlanetData
{
    [SerializeField]
    private string name;
    [SerializeField]
    private Vector3 position;
    [SerializeField]
    private Vector3 initialVelocity;
    [SerializeField]
    private Vector3 initialTorque;
    [SerializeField]
    private float radius;
    [SerializeField]
    private Vector3 orientationE;
    [SerializeField]
    private List<MoonData> moons = new();

    internal string GetName()
    {
        return name;
    }

    internal float GetRadius()
    {
        return radius;
    }
}
[Serializable]
public class MoonData
{
    [SerializeField]
    private string name;
    [SerializeField]
    private Vector3 position;
    [SerializeField]
    private Vector3 initialVelocity;
    [SerializeField]
    private Vector3 initialTorque;
    [SerializeField]
    private float radius;
    [SerializeField]
    private Vector3 orientationE;
}
public class AtmoData
{
    public Vector3 _lightDirection;
    public Vector3 _lightColor;
    public Vector3 _ScatteringCoeafficients;
    public Vector3 _PlanetPosition;
    public float _AtmosphereRadius;
    public float _PlanetRadius;
}
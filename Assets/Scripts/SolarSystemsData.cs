using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[Serializable]
public class SolarSystemsData : MonoBehaviour
{
    [SerializeField]
    private List<SolarSystemData> data = new();


    public SolarSystemData GetCurrentFromPPos(Vector3 position)
    {
        SolarSystemData result = null;
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


    public float DistanceFrom(Vector3 position)
    {
        return (this.position - position).magnitude;
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
    private float radius;
    [SerializeField]
    private Vector3 orientationE;
    [SerializeField]
    private List<MoonData> moons = new();
    

}
[Serializable]
public class MoonData
{
    [SerializeField]
    private string name;
    [SerializeField]
    private Vector3 position;
    [SerializeField]
    private float radius;
    [SerializeField]
    private Vector3 orientationE;

}

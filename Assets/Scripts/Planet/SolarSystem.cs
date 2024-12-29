using System;
using System.Collections.Generic;
using UnityEngine;



public class SolarSystem : MonoBehaviour
{
    SolarSystemData data;
    List<Planet> planets = new();
    void Start()
    {
        //TODO spawn star(s)
        // assign materials


        data.GetPlanets().ForEach(p => 
        {
            GameObject planetGO = new(p.GetName());
            planetGO.transform.parent = transform;
            Planet planet = planetGO.AddComponent<Planet>();
            planet.LoadData(p);
            planets.Add(planet);
        });
    }

    void Update()
    {

    }

    public void LoadData(SolarSystemData data)
    {
        this.data = data;
    }

    public void Kill()
    {
        
    }

    public List<AtmoData> GetAtmoData()
    {
        List<AtmoData> res = new();
        planets.ForEach(p => 
        {
            if (p.HasAtmo()) res.Add(p.GetAtmoData());
            if (p.HasMoon()) p.GetAtmoDataForMoons(res);
        });
        return res;
    }
}
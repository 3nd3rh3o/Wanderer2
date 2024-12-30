using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class SolarSystem : MonoBehaviour
{
    public 
    SolarSystemData data;
    private TerrainAtlas telluricAtlas;
    private ComputeShader geoTelluricCS;
    private Material telluricTerrainMat;

    List<IMajorCellestialBody> planets;
    void OnEnable()
    {
        planets = new();
        data?.GetPlanets().ForEach(p => 
        {
            GameObject planetGO = new(p.GetName());
            planetGO.SetActive(false);
            planetGO.transform.parent = transform;
            switch (p.GetMCBType())
            {
                case MCBType.TELURIC_PLANET:
                    planetGO.AddComponent<Rigidbody>();
                    TelluricMajorCelestialBody planet = planetGO.AddComponent<TelluricMajorCelestialBody>();
                    planet.Init(p.GetRadius(), p.GetMass(), p.GetInitialVelocity(), p.GetInitialPosition(), p.GetInitialTorque(), p.GetInitialOrientation(), p.IsKynematic(), telluricAtlas, telluricTerrainMat, p.HasAtmosphere()? p.GetAtmoData() : null, p.GetBiomeScale(), p.GetBiomeMul(), p.GetBiomeOffset(), p.GetBiomes().ToArray(), geoTelluricCS, p.GetMLOD());
                    
                    planets.Add(planet);
                    break;
            }
        });
    }

    void OnDisable()
    {
        planets?.ForEach(p => p.Kill());
        planets = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
    }

    void Update()
    {

    }

    public void LoadData(SolarSystemData data, TerrainAtlas telluricAtlas, ComputeShader geoTelluricCS, Material telluricTerrainMat)
    {
        this.data = data;
        this.telluricAtlas = telluricAtlas;
        this.geoTelluricCS = geoTelluricCS;
        this.telluricTerrainMat = telluricTerrainMat;
        gameObject.SetActive(true);
    }

    public void Kill()
    {
        planets?.ForEach(p => p.Kill());
        planets = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
        
    }

    public List<AtmoData> GetAtmoData()
    {
        List<AtmoData> res = new();
        planets?.ForEach(p => 
        {
            
        });
        return res;
    }
}
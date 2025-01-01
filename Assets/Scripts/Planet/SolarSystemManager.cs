using System.Collections.Generic;
using UnityEngine;

public class SolarSystemManager : MonoBehaviour
{
    public Vector3 playerPosition = new();
    public Vector3 playerRotation = new();

    public TerrainAtlas telluricAtlas;
    public ComputeShader TelluricPlanetSurfaceGeometryShader;
    public Material TelluricPlanetSurfaceShader;
    public Material PlanetAtmosphereShader;
    [SerializeField]
    public AtmosphereFullScreenPassRendererFeature atmosphereRFClose;
    [SerializeField]
    public AtmosphereFullScreenPassRendererFeature atmosphereRFFar;
    private SolarSystemsData solarSystemsData;

    //Solar system
    private SolarSystem currentSys;

    void OnEnable()
    {
        telluricAtlas.Init();
        solarSystemsData = GetComponent<SolarSystemsData>();
        SolarSystemData currentSystemData = solarSystemsData.GetCurrentFromPPos(playerPosition);
        GameObject solarSystemGO = new GameObject(currentSystemData.GetName());
        solarSystemGO.SetActive(false);
        solarSystemGO.transform.SetParent(transform);
        solarSystemGO.transform.position = currentSystemData.GetPosition();
        solarSystemGO.transform.rotation = Quaternion.Euler(currentSystemData.GetOrientation());
        SolarSystem solarSystem = solarSystemGO.AddComponent<SolarSystem>();
        solarSystem.LoadData(currentSystemData, telluricAtlas, TelluricPlanetSurfaceGeometryShader, TelluricPlanetSurfaceShader);
        currentSys = solarSystem;
    }
    void OnDisable()
    {
        currentSys.Kill();
        Destroy(transform.GetChild(0).gameObject);
        currentSys = null;
        telluricAtlas.Cleanup();
    }

    void Update()
    {
        //Handle registration of planets in atmosphere shader.
        List<AtmoData> atmoDatas = currentSys.GetAtmoData();
        if (atmoDatas.Count == 0) {
            atmosphereRFClose.enabled = false;
            atmosphereRFFar.enabled = false;
        } else {
            atmosphereRFClose.enabled = true;
            atmosphereRFFar.enabled = true;
        }
        //Construct arrays to send to the atmosphere shader.
        


        //Handle player relative positions.
        currentSys.transform.position = Quaternion.Inverse(Quaternion.Euler(playerRotation))*-playerPosition;
        currentSys.transform.rotation = Quaternion.Inverse(Quaternion.Euler(playerRotation));
    }
}
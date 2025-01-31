using System.Collections.Generic;
using UnityEngine;

public class SolarSystemManager : MonoBehaviour
{
    #if !UNITY_EDITOR
    public Vector3 playerPosition = new();
    public Vector3 playerRotation = new();

    public TerrainAtlas telluricAtlas;
    public ComputeShader TelluricPlanetSurfaceGeometryShader;
    public Material TelluricPlanetSurfaceShader;
    public Material PlanetAtmosphereShader;
    [SerializeField]
    public MultiMaterialFullScreenPassRendererFeature atmosphereRFClose;
    [SerializeField]
    public MultiMaterialFullScreenPassRendererFeature atmosphereRFFar;
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
        solarSystem.LoadData(currentSystemData, telluricAtlas, TelluricPlanetSurfaceGeometryShader, TelluricPlanetSurfaceShader, PlanetAtmosphereShader);
        currentSys = solarSystem;
    }
    void OnDisable()
    {
        currentSys.Kill();
        Destroy(transform.GetChild(0).gameObject);
        currentSys = null;
        telluricAtlas.Cleanup();
        atmosphereRFClose.passMaterial = new Material[0];
        atmosphereRFFar.passMaterial = new Material[0];
    }

    void Update()
    {
        //Handle registration of planets in atmosphere shader.
        List<Material> atmoDatas = currentSys.GetAtmoData();
        if (atmoDatas.Count == 0) {
            atmosphereRFClose.enabled = false;
            atmosphereRFFar.enabled = false;
            atmosphereRFClose.passMaterial = new Material[0];
            atmosphereRFFar.passMaterial = new Material[0];
        } else {
            atmosphereRFClose.enabled = true;
            atmosphereRFFar.enabled = true;
            atmosphereRFClose.passMaterial = atmoDatas.ToArray();
            atmosphereRFFar.passMaterial = atmoDatas.ToArray();
        }
        //Construct arrays to send to the atmosphere shader.
        


        //Handle player relative positions.
        currentSys.transform.position = Quaternion.Inverse(Quaternion.Euler(playerRotation))*-playerPosition;
        currentSys.transform.rotation = Quaternion.Inverse(Quaternion.Euler(playerRotation));
    }
    #endif
}
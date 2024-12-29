using System.Collections.Generic;
using UnityEngine;

public class SolarSystemManager : MonoBehaviour
{
    private Vector3 playerPosition = new();
    private Vector3 playerRotation = new();
    
    public ComputeShader TellicPlanetSurfaceGeometryShader;
    public Shader TelluricPlanetSurfaceShader;
    public Material PlanetAtmosphereShader;
    [SerializeField]
    public AtmosphereFullScreenPassRendererFeature atmosphereRFClose;
    [SerializeField]
    public AtmosphereFullScreenPassRendererFeature atmosphereRFFar;

    //private Vector3 playerRotation = new();
    private SolarSystemsData solarSystemsData;

    //Solar system
    private SolarSystem currentSys;

    void OnEnable()
    {
        solarSystemsData = GetComponent<SolarSystemsData>();
    }
    void OnDisable()
    {
        currentSys.Kill();
        currentSys = null;
    }



    void Start()
    {
        // spawn nearest solar system
        SolarSystemData currentSystemData = solarSystemsData.GetCurrentFromPPos(playerPosition);
        GameObject solarSystemGO = new GameObject(currentSystemData.GetName());
        solarSystemGO.transform.SetParent(transform);
        solarSystemGO.transform.position = currentSystemData.GetPosition();
        solarSystemGO.transform.rotation = Quaternion.Euler(currentSystemData.GetOrientation());
        SolarSystem solarSystem = solarSystemGO.AddComponent<SolarSystem>();
        solarSystem.LoadData(currentSystemData);
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
        currentSys.transform.position = -playerPosition;
        currentSys.transform.rotation = Quaternion.Inverse(Quaternion.Euler(playerRotation));
    }
}
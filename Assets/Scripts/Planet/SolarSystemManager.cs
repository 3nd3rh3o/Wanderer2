using UnityEngine;

public class SolarSystemManager : MonoBehaviour
{
    private Vector3 playerPosition = new();
    private Vector3 playerRotation = new();
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
        //move planet
        //handle playermotions?
    }
}
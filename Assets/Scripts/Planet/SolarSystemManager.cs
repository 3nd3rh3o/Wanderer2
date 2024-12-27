using UnityEngine;

public class SolarSystemManager : MonoBehaviour
{
    private Vector3 playerPosition = new();
    private Vector3 playerRotation = new();
    //private Vector3 playerRotation = new();
    private SolarSystemsData solarSystemsData;

    //Solar system
    private GameObject currentSys;

    void OnEnable()
    {
        solarSystemsData = GetComponent<SolarSystemsData>();
    }
    void OnDisable()
    {
        solarSystemsData=null;
    }



    void Start()
    {
        // spawn nearest solar system
        SolarSystemData currentSystemData = solarSystemsData.GetCurrentFromPPos(playerPosition);
        GameObject solarSystem = new(currentSystemData.GetName());
        solarSystem.transform.position = currentSystemData.GetPosition();
        currentSys = solarSystem;
        solarSystem.transform.rotation = Quaternion.Euler(currentSystemData.GetOrientation());
        solarSystem.AddComponent<SolarSystem>();        
    }

    void Update()
    {
        //move planet
        //handle playermotions?
    }
}
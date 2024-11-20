using UnityEngine;

public class SolarSystemManager : MonoBehaviour
{
    private Vector3 playerPosition = new();
    private Vector3 playerRotation = new();
    private SolarSystemsData solarSystemsData;

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
        SolarSystemData currentSystem = solarSystemsData.GetCurrentFromPPos(playerPosition);
    }

    void Update()
    {
        //move planet
        //handle playermotions?
    }
}

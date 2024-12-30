using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelluricMajorCelestialBody : IMajorCellestialBody
{
    private TerrainAtlas atlas;
    private Material terrainMaterial;
    private float atmosphereRadius;
    private float planetAtmRadius;
    private float BiomeScale;
    private float BiomeMul;
    private Vector3 BiomeOffset;
    private Biome[] biomes;
    private ComputeShader geometryGen;
    private int mLOD;
    private Queue<ChunkTask> queue;
    private bool hasAtmosphere;
    private AtmoData atmoData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }


    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {

    }
    public override void Kill()
    {
        base.Kill();
        this.OnDisable();
        Destroy(gameObject);
    }

    public void Init(
        float radius, float mass,
        Vector3 initialVelocity, Vector3 initialPosition,
        Vector3 initialTorque, Vector3 initialOrientation,
        bool IsKynematic,
        TerrainAtlas atlas, Material terrainMaterial,
        AtmoData atmoData, 
        float BiomeScale, float BiomeMul, 
        Vector3 BiomeOffset, Biome[] biomes,
        ComputeShader geometryGen, int mLOD
    )
    {
        base.Init(radius, mass, initialVelocity, initialPosition, initialTorque, initialOrientation, IsKynematic);
        this.atlas = atlas;
        this.terrainMaterial = terrainMaterial;
        if (atmoData == null)
        {
            hasAtmosphere = false;
        }
        else
        {
            hasAtmosphere = true;
            this.atmoData = atmoData;
        }
        this.BiomeScale = BiomeScale;
        this.BiomeMul = BiomeMul;
        this.BiomeOffset = BiomeOffset;
        this.biomes = biomes;
        this.geometryGen = geometryGen;
        this.mLOD = mLOD;
        gameObject.SetActive(true);
    }
}
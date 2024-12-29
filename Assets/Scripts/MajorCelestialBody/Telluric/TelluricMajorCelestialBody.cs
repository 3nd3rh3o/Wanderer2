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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }

    public void Init(
        TerrainAtlas atlas, Material terrainMaterial,
        float AtmosphereRadius, float PlanetAtmRadius, 
        float BiomeScale, float BiomeMul, 
        Vector3 BiomeOffset, Biome[] biomes,
        ComputeShader geometryGen, int mLOD
    )
    {
        this.atlas = atlas;
    }
}

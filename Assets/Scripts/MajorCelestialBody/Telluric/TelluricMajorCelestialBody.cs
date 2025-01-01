using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private Queue<ChunkTask> queue = new();
    private ChunkNHMapCSManager csMan;
    private bool hasAtmosphere;
    private AtmoData atmoData;
    private Chunk[] chunks;


    private void Build()
    {
        List<Tuple<CombineInstance, RenderTexture, RenderTexture, RenderTexture, RenderTexture, RenderTexture, RenderTexture>> chunkData = new();

        // Parcourt les faces principales (chunks racines)
        foreach (var face in chunks)
        {
            face.CollectCombineData(chunkData);
        }
        CombineInstance[] combines = new CombineInstance[chunkData.Count];
        RenderTexture[] albedosTextures = new RenderTexture[chunkData.Count];
        RenderTexture[] normalsTextures = new RenderTexture[chunkData.Count];
        RenderTexture[] heightsTextures = new RenderTexture[chunkData.Count];
        RenderTexture[] metalicsTextures = new RenderTexture[chunkData.Count];
        RenderTexture[] rougnesssTextures = new RenderTexture[chunkData.Count];
        RenderTexture[] occlusionsTextures = new RenderTexture[chunkData.Count];
        for (int i = 0; i < chunkData.Count; i++) (combines[i], albedosTextures[i], normalsTextures[i], heightsTextures[i], metalicsTextures[i], rougnesssTextures[i], occlusionsTextures[i]) = chunkData[i];
        // Crée un mesh global combiné
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combines, false, true);


        // ajuste le renderer
        Material[] m = new Material[combines.Length];
        for (int i = 0; i < combines.Length; i++) m[i] = terrainMaterial;
        // Applique le mesh combiné au MeshFilter principal
        GetComponent<MeshRenderer>().SetMaterials(m.ToList());
        for (int i = 0; i < combines.Length; i++)
        {
            MaterialPropertyBlock mpb = new();
            mpb.SetTexture("_BaseMap", albedosTextures[i]);
            mpb.SetTexture("_BumpMap", normalsTextures[i]);
            mpb.SetVector("_LightDirection", transform.parent.rotation*(transform.localPosition).normalized);
            mpb.SetVector("_LightColor", new Vector3(1, 1 , 1));
            //TODO fix me?
            //mpb.SetTexture("_ParallaxMap", heightsTextures[i]);
            GetComponent<MeshRenderer>().SetPropertyBlock(mpb, i);
        }
        GetComponent<MeshFilter>().mesh = combinedMesh;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (chunks != null && queue.Count == 0)
            chunks.ToList().ForEach(c => c.Update(Quaternion.Inverse(transform.rotation) * transform.position * -1f, queue));
        else
        {
            ChunkTask t = queue.Dequeue();
            t.chunk.ConsumeChunkTask(t);
        }
        Build();
    }


    public override void OnEnable()
    {
        
        base.OnEnable();
        if (geometryGen) csMan = new(geometryGen);
        chunks = new Chunk[]{
            new(new Vector3(0, radius, 0), radius * 2, 0, mLOD, radius, csMan, BiomeScale, BiomeMul, BiomeOffset, biomes, atlas.atlas, 0),
            new(new Vector3(0, -radius, 0), radius * 2, 1, mLOD, radius, csMan, BiomeScale, BiomeMul, BiomeOffset, biomes, atlas.atlas, 0),
            new(new Vector3(0, 0, radius), radius * 2, 2, mLOD, radius, csMan, BiomeScale, BiomeMul, BiomeOffset, biomes, atlas.atlas, 0),
            new(new Vector3(0, 0, -radius), radius * 2, 3, mLOD, radius, csMan, BiomeScale, BiomeMul, BiomeOffset, biomes, atlas.atlas, 0),
            new(new Vector3(radius, 0, 0), radius * 2, 4, mLOD, radius, csMan, BiomeScale, BiomeMul, BiomeOffset, biomes, atlas.atlas, 0),
            new(new Vector3(-radius, 0, 0), radius * 2, 5, mLOD, radius, csMan, BiomeScale, BiomeMul, BiomeOffset, biomes, atlas.atlas, 0)
        };
        Build();
    }

    public override void OnDisable()
    {
        chunks?.ToList().ForEach(c => c.Kill());
        chunks=null;
        GetComponent<MeshFilter>().mesh = null;
        GetComponent<MeshRenderer>().SetMaterials(new());
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
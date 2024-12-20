#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class PlanetPrototype : MonoBehaviour
{
    //TODO only for test, later will be given by super dataholder.
    public TerrainAtlas atlas = new();
    private Chunk[] chunks;
    public Material sharedMat;
    public ComputeShader cs;
    private ChunkNHMapCSManager csMan;
    public float BiomeScale = 1f;
    public float BiomeMultiplier = 1f;
    public Vector3 BiomeOffset = new();
    public Biome[] biomes = new Biome[0];
    public bool autoUpdate = true;
    public bool update = false;
    public bool oneFace;


    public float radius;
    [Range(0, 10)]
    public int LOD;

    protected void Build()
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
        combinedMesh.CombineMeshes(combines.ToArray(), false, true);


        // ajuste le renderer
        Material[] m = new Material[combines.Length];
        for (int i = 0; i < combines.Length; i++) m[i] = sharedMat;
        // Applique le mesh combiné au MeshFilter principal
        GetComponent<MeshRenderer>().SetMaterials(m.ToList());
        for (int i = 0; i < combines.Length; i++)
        {
            MaterialPropertyBlock mpb = new();
            mpb.SetTexture("_Albedo", albedosTextures[i]);
            mpb.SetTexture("_Normals", normalsTextures[i]);
            mpb.SetTexture("_Height", heightsTextures[i]);
            GetComponent<MeshRenderer>().SetPropertyBlock(mpb, i);
        }
        GetComponent<MeshFilter>().mesh = combinedMesh;
    }

    void Start()
    { 

    }

    void OnEnable()
    {
        atlas.Init();
        if (cs) csMan = new(cs);
        Mesh mesh = new();
        if (oneFace)
        {
            chunks = new Chunk[]{
            new Chunk(new Vector3(0, radius, 0), radius * 2, 0, LOD, radius, csMan, BiomeScale, BiomeMultiplier, BiomeOffset, biomes, atlas.atlas, 0)};
        }
        else
        {
            chunks = new Chunk[]{
            new Chunk(new Vector3(0, radius, 0), radius * 2, 0, LOD, radius, csMan, BiomeScale, BiomeMultiplier, BiomeOffset, biomes, atlas.atlas, 0),
            new Chunk(new Vector3(0, -radius, 0), radius * 2, 1, LOD, radius, csMan, BiomeScale, BiomeMultiplier, BiomeOffset, biomes, atlas.atlas, 0),
            new Chunk(new Vector3(0, 0, radius), radius * 2, 2, LOD, radius, csMan, BiomeScale, BiomeMultiplier, BiomeOffset, biomes, atlas.atlas, 0),
            new Chunk(new Vector3(0, 0, -radius), radius * 2, 3, LOD, radius, csMan, BiomeScale, BiomeMultiplier, BiomeOffset, biomes, atlas.atlas, 0),
            new Chunk(new Vector3(radius, 0, 0), radius * 2, 4, LOD, radius, csMan, BiomeScale, BiomeMultiplier, BiomeOffset, biomes, atlas.atlas, 0),
            new Chunk(new Vector3(-radius, 0, 0), radius * 2, 5, LOD, radius, csMan, BiomeScale, BiomeMultiplier, BiomeOffset, biomes, atlas.atlas, 0)
        };
        }
        for (int i = 0; i <= LOD; i++) chunks?.ToList().ForEach(c => c.Update(LOD));
        Build();
    }

    void OnDisable()
    {
        chunks?.ToList().ForEach(c => c.Kill());
        GetComponent<MeshFilter>().mesh = null;
        GetComponent<MeshRenderer>().SetMaterials(new());
        atlas.Cleanup();
    }

    void Update()
    {
        if (!autoUpdate && !update) return;
        if (LOD > 4) autoUpdate = false;
        update = false;
        OnDisable();
        OnEnable();
        /*for (int i = 0; i <= LOD; i++) chunks?.ToList().ForEach(c => c.Update(LOD));
        Build();*/
    }

    void LateUpdate()
    {

    }
}
#endif
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
    public Material atmosphereMat;
    public float AtmosphereRadius = 0f;
    public MultiMaterialFullScreenPassRendererFeature atmosphere;
    public float PlanetAtmRad;
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
            mpb.SetTexture("_BaseMap", albedosTextures[i]);
            mpb.SetTexture("_BumpMap", normalsTextures[i]);
            
            //TODO fix me?
            //mpb.SetTexture("_ParallaxMap", heightsTextures[i]);
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
        chunks=null;
        GetComponent<MeshFilter>().mesh = null;
        GetComponent<MeshRenderer>().SetMaterials(new());
        atlas.Cleanup();
    }
    public Vector3 wavelength = new Vector3(700, 530, 440);
    public float scatteringStrength;
    void Update()
    {
        
        float scatterR = Mathf.Pow(400 / wavelength.x, 4) * scatteringStrength;
        float scatterG = Mathf.Pow(400 / wavelength.y, 4) * scatteringStrength;
        float scatterB = Mathf.Pow(400 / wavelength.z, 4) * scatteringStrength;
        Vector3 scatteringCoefficients = new Vector3(scatterR, scatterG, scatterB);


        sharedMat?.SetVector("_LightDirection", (-transform.position).normalized);
        sharedMat?.SetVector("_LightColor", new Vector3(1, 1 , 1));
        atmosphereMat?.SetVector("_ScatteringCoefficients", scatteringCoefficients);
        atmosphereMat?.SetVector("_PlanetPosition", transform.position);
        atmosphereMat?.SetFloat("_AtmosphereRadius", AtmosphereRadius);
        atmosphereMat?.SetFloat("_PlanetRadius", PlanetAtmRad);
        
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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class Planet : MonoBehaviour
{
    private Queue<ChunkTask> chunkTasks;
    private Chunk[] chunks;
    public Material sharedMat;
    public Material atmosphereMat;
    public float AtmosphereRadius = 0f;
    public float PlanetAtmRad;
    public ComputeShader cs;
    private ChunkNHMapCSManager csMan;
    public float BiomeScale;
    public float BiomeMultiplier;
    public Vector3 BiomeOffset = new();
    public Biome[] biomes = new Biome[0];
    public bool hasAtmo;

    [SerializeField]
    public ChunkNHMapCSManager.Instr[] instructions = new ChunkNHMapCSManager.Instr[0];
    public float radius;
    [SerializeField]
    private int mLOD;

    private Planet[] moons;

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
            GetComponent<MeshRenderer>().SetPropertyBlock(mpb, i);
        }
        GetComponent<MeshFilter>().mesh = combinedMesh;
    }

    void Start()
    {

    }

    void OnEnable()
    {
        if (cs) csMan = new(cs);
        Mesh mesh = new();
        chunks = new Chunk[]{
            /*new Chunk(new Vector3(0, radius, 0), radius * 2, 0, mLOD, radius, csMan, 1, 1, new(), new Biome[0]),
            new Chunk(new Vector3(0, -radius, 0), radius * 2, 1, mLOD, radius, csMan, 1, 1, new(), new Biome[0]),
            new Chunk(new Vector3(0, 0, radius), radius * 2, 2, mLOD, radius, csMan, 1, 1, new(), new Biome[0]),
            new Chunk(new Vector3(0, 0, -radius), radius * 2, 3, mLOD, radius, csMan, 1, 1, new(), new Biome[0]),
            new Chunk(new Vector3(radius, 0, 0), radius * 2, 4, mLOD, radius, csMan, 1, 1, new(), new Biome[0]),
            new Chunk(new Vector3(-radius, 0, 0), radius * 2, 5, mLOD, radius, csMan, 1, 1, new(), new Biome[0])*/
        };
        chunkTasks = new();
        Build();
    }

    void OnDisable()
    {
        chunks.ToList().ForEach(c => c.Kill());
        GetComponent<MeshFilter>().mesh = null;
        GetComponent<MeshRenderer>().SetMaterials(new());
        chunkTasks = null;
    }

    void Update()
    {
        if (chunks != null && chunkTasks.Count == 0)
            chunks.ToList().ForEach(c => c.Update(Quaternion.Inverse(transform.localRotation) * transform.position * -1f, chunkTasks));
        else
        {
            ChunkTask t = chunkTasks.Dequeue();
            t.chunk.ConsumeChunkTask(t);
        }
        Build();
    }

    void LateUpdate()
    {

    }

    internal void LoadData(PlanetData p)
    {
        this.radius = p.GetRadius();
    }

    internal bool HasAtmo()
    {
        return hasAtmo;
    }
    internal bool HasMoon()
    {
        return moons.Length > 0;
    }


    internal AtmoData GetAtmoData()
    {
        return new();
    }

    internal void GetAtmoDataForMoons(List<AtmoData> res)
    {
        throw new NotImplementedException();
    }
}
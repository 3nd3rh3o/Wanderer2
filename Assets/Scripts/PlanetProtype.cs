#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class PlanetPrototype : MonoBehaviour
{
    private Chunk[] chunks;
    public Material sharedMat;
    public ComputeShader cs;
    private ChunkNHMapCSManager csMan;
    [SerializeField]
    public ChunkNHMapCSManager.Instr[] instructions = new ChunkNHMapCSManager.Instr[0];
    public Biome[] biomes = new Biome[0];


    public float radius;
    [Range(0, 3)]
    public int LOD;

    protected void Build()
    {
        List<Tuple<CombineInstance, RenderTexture>> chunkData = new();

        // Parcourt les faces principales (chunks racines)
        foreach (var face in chunks)
        {
            face.CollectCombineData(chunkData);
        }
        CombineInstance[] combines = new CombineInstance[chunkData.Count];
        RenderTexture[] renderTextures = new RenderTexture[chunkData.Count];
        for (int i = 0; i < chunkData.Count; i++) (combines[i], renderTextures[i]) = chunkData[i];
        // Crée un mesh global combiné
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combines.ToArray(), false, true);


        // ajuste le renderer
        Material[] m = new Material[combines.Length];
        for (int i = 0; i < combines.Length; i++) m[i] = sharedMat;
        // Applique le mesh combiné au MeshFilter principal
        GetComponent<MeshRenderer>().SetMaterials(m.ToList());
        for (int i = 0; i < renderTextures.Length; i++)
        {
            MaterialPropertyBlock mpb = new();
            mpb.SetTexture("_NHMap", renderTextures[i]);
            GetComponent<MeshRenderer>().SetPropertyBlock(mpb, i);
        }
        GetComponent<MeshFilter>().mesh = combinedMesh;
    }

    void Start()
    {

    }

    void OnEnable()
    {
        if (cs) csMan = new(cs, instructions);
        Mesh mesh = new();
        chunks = new Chunk[]{
            new Chunk(new Vector3(0, radius, 0), radius * 2, 0, LOD, radius, csMan),
            new Chunk(new Vector3(0, -radius, 0), radius * 2, 1, LOD, radius, csMan),
            new Chunk(new Vector3(0, 0, radius), radius * 2, 2, LOD, radius, csMan),
            new Chunk(new Vector3(0, 0, -radius), radius * 2, 3, LOD, radius, csMan),
            new Chunk(new Vector3(radius, 0, 0), radius * 2, 4, LOD, radius, csMan),
            new Chunk(new Vector3(-radius, 0, 0), radius * 2, 5, LOD, radius, csMan)
        };
        Build();
    }

    void OnDisable()
    {
        chunks.ToList().ForEach(c => c.Kill());
        GetComponent<MeshFilter>().mesh = null;
        GetComponent<MeshRenderer>().SetMaterials(new());
    }

    void Update()
    {
        csMan?.UpdateSettings(instructions);
        chunks?.ToList().ForEach(c => c.Update(LOD));
        Build();
    }

    void LateUpdate()
    {

    }
}
#endif
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
    public ComputeShader cs;
    private ChunkNHMapCSManager csMan;
    [SerializeField]
    public ChunkNHMapCSManager.Instr[] instructions = new ChunkNHMapCSManager.Instr[0];


    public float radius;
    public int mLOD;

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
            new Chunk(new Vector3(0, radius, 0), radius * 2, 0, mLOD, radius, csMan),
            new Chunk(new Vector3(0, -radius, 0), radius * 2, 1, mLOD, radius, csMan),
            new Chunk(new Vector3(0, 0, radius), radius * 2, 2, mLOD, radius, csMan),
            new Chunk(new Vector3(0, 0, -radius), radius * 2, 3, mLOD, radius, csMan),
            new Chunk(new Vector3(radius, 0, 0), radius * 2, 4, mLOD, radius, csMan),
            new Chunk(new Vector3(-radius, 0, 0), radius * 2, 5, mLOD, radius, csMan)
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
        csMan?.UpdateSettings(instructions);
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
}
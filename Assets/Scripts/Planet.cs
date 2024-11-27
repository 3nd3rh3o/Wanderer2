using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
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

    //TODO get Texture here ! and feed it to the material ! one combine => one texture2D 
    // iterate over the length of the list of tuple, and set material property block
    private void Build()
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
        combinedMesh.RecalculateTangents();
        combinedMesh.RecalculateBounds();


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
        sharedMat.SetTexture("_MainTex", renderTextures[0]);
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
    //TODO queue empty -> update
    // else dequeue + build
    void Update()
    {
        csMan?.UpdateSettings(instructions);
        if (chunks != null && chunkTasks.Count == 0)
            chunks.ToList().ForEach(c => c.Update(transform.position * -1f, chunkTasks));
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


    /*
    0 ---- 1      0 --c- 1
    |     /       |A/D|B/
    |    /        |/  |/
    |   /      => a - b
    |  /          |  /
    | /           |C/
    2             2

    a ----- b    a - e - b
    |       |    |   |   |
    |       |    |   |   |
    |       | => h - j - f
    |       |    |   |   |
    |       |    |   |   |
    d ----- c    d - g - c
    TODO chunk id(hash of pos in qTree)


    a = t[0]
    b = t[1]
    c = t[2]
    d = t[5]

    e = a+b*0.5
    f = a+c*0.5
    g = b+d*0.5
    h = c+d*0.5
    j = e+h*0.5

    AB aejf => to tri => aj<=ef? (aef, jfe) : (eja, faj)
    CD ebgj => to tri => eg<=bj? (ebj, gjb) : (bge, jeg)
    EF fjhc => to tri => fh<=jc? (fjc, hcj) : (jhf, cfh)
    GH jgdh => to tri => jd<=gh? (jgh, dhg) : (gdj, hjd)

        Y (Up)
        |
        |
       / \
      /   \
     X (L) Z (Forward)
    
    up: 0
            -[-0.5, 0.5, -0.5]
            -[-0.5, 0.5, 0.5]
            -[0.5, 0.5, -0.5]
            -[0.5, 0.5, 0.5]
    down: 1
            -[-0.5, -0.5, 0.5]
            -[-0.5, -0.5, -0.5]
            -[0.5, -0.5, 0.5]
            -[0.5, -0.5, -0.5]
    front: 2
            -[0.5, 0.5, 0.5]
            -[-0.5, 0.5, 0.5]
            -[0.5, -0.5, 0.5]
            -[-0.5, -0.5, 0.5]
    back: 3
            -[-0.5, 0.5, -0.5]
            -[0.5, 0.5, -0.5]
            -[-0.5, -0.5, -0.5]
            -[0.5, -0.5, -0.5]
    left: 4
            -[0.5, 0.5, -0.5]
            -[0.5, 0.5, 0.5]
            -[0.5, -0.5, -0.5]
            -[0.5, -0.5, 0.5]
    right: 5
            -[-0.5, 0.5, 0.5]
            -[-0.5, 0.5, -0.5]
            -[-0.5, -0.5, 0.5]
            -[-0.5, -0.5, -0.5]

    face => v : 
            -[-0.5, 0.5, -0.5]
            -[-0.5, 0.5, 0.5]
            -[0.5, 0.5, -0.5]  => ++ a, b, c
            -[0.5, 0.5, 0.5]
            t :
            - {0, 1, 2}, {2, 1, 3} => ({0,1,2} => c, b, a) ++ (0, c, a), (c, 1, b), (a, b, 2)

    SubDivide : (v[], t[]) => 
                        const mI = len(t)

                        for i=0; i<mI; i++:
                            for a,b,c in t[i]:
                                int vC = len(v)
                                a + b * 0.5 => ab
                                b + c * 0.5 => bc
                                c + a * 0.5 => ca

                                abi = ab inside v?
                                    ye => id of ab in v
                                    nu => vC; (insert); vC++
                                bci = bc inside v?
                                    ye => id of bc in v
                                    nu => vC; (insert); vC++
                                cai = bc inside v?
                                    ye => id of ca in v
                                    nu => vC; (insert); vC++
                                t[i] = {abi, bci, cai}
                                t[len(t)] = {a, abi, cai}
                                t[len(t)] = {abi, b, bci}
                                t[len(t)] = {cai, bci, c}
                            

                                
                            
    8 * 8 => 1 chunk
    */
}
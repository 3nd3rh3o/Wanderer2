using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class Planet : MonoBehaviour
{
    public Material sharedMat;
    private Mesh mesh;
    private List<int> chunksReg;
    
    private class Chunk
    {
        private struct QuadMesh
        {
            public Vector3[] vertices;
            public int[] faces;
            /*
            TODO uv (for nhmap)
                 normals
            */

            public QuadMesh(Vector3[] v, int[] f)
            {
                vertices = v;
                faces = f;
            }
        }
        private Vector3 center;
        private float size;
        private Mesh cachedMesh;
        private CombineInstance combineData;
        private int LOD = 0;
        private float LOD_threshold;
        private int DIR;
        private Chunk[] chunks;
        private float gRad;
        private Material mat;
        private Material hmat;

        public void CollectCombineData(List<Tuple<CombineInstance, Material>> combineInstances)
        {
            if (chunks == null)
            {
                // Chunk terminal, ajoute son mesh
                if (cachedMesh != null)
                {
                    CombineInstance combine = new CombineInstance
                    {
                        mesh = cachedMesh,
                        transform = Matrix4x4.identity // Identité si tout est en local space
                    };
                    combineInstances.Add(new(combine, mat));

                }
            }
            else
            {
                // Parcourt les enfants pour collecter leurs données
                foreach (var child in chunks)
                {
                    child.CollectCombineData(combineInstances);
                }
            }
        }


        public Chunk(Vector3 center, float size, int Dir, int LOD, float gRad, Material mat)
        {

            this.gRad = gRad;
            this.center = center;
            cachedMesh = ToMesh(/*GenNHMap*/SubDivide(SubDivide(SubDivide(GenInitMesh(Dir, center, size)))));
            this.size = size;
            this.LOD = LOD;
            DIR = Dir;
            hmat = mat; // reference. NEVER USED DIRECTLY!
            this.mat = Instantiate(hmat);
            
        }


        private QuadMesh GenNHMap(QuadMesh iMesh)
        {
            return new();
        }

        private Mesh ToMesh(QuadMesh qMesh)
        {
            Mesh mesh = new();
            int mF = qMesh.faces.Length;
            int[] f = qMesh.faces;
            Vector3[] v = qMesh.vertices;
            List<int> t = new((int)(mF * 1.5f));
            for (int i = 0; i < mF; i+=4)
            {
                int a = f[i];
                int b = f[i + 1];
                int c = f[i + 2];
                int d = f[i + 3];
                t.AddRange(((v[b]+v[d])*0.5f).sqrMagnitude <= ((v[a]+v[c]) * 0.5f).sqrMagnitude ? new int[]{a, b, d, c, d, b} : new int[]{a, b, c, c, d, a});
            }
            mesh.SetVertices(v);
            mesh.SetTriangles(t, 0);
            mesh.RecalculateBounds();
            return mesh;
        }


        // true = needRebuild
        //TODO finish it => QUADTREE !!!
        // it's here that you align borders
        public bool Update(Vector3 pPos)
        {
            if ((pPos - center).sqrMagnitude <= 3f * size * size)
            {
                // need to be cut;
                if (chunks != null)
                {
                    // give potato to childs
                    if (chunks.Any(c => c.Update(pPos))) return true;
                }
                else
                {
                    if (LOD <= 0) return false;
                    if (mat != null) DestroyImmediate(mat);
                    chunks = GenChilds(center, DIR, LOD - 1, size, gRad);
                    return true;
                }
            }
            else
            {
                // need to be uncut
                if (chunks != null)
                {
                    mat = Instantiate(hmat);
                    chunks.ToList().ForEach(c => c.Kill());
                    chunks = null;

                    return true;
                }
            }
            return false; // No changes needed for this chunk
        }

        public void Kill()
        {
            if (mat != null) DestroyImmediate(mat);
            if (chunks != null) chunks.ToList().ForEach(c => c.Kill()); // Nettoie les enfants
            if (cachedMesh != null) DestroyImmediate(cachedMesh); // Libère la mémoire GPU
            cachedMesh = null;
        }

        private Chunk[] GenChilds(Vector3 center, int Dir, int LOD, float size, float gRad)
        {
            float nS = size * 0.5f;
            float ofs = nS * 0.5f;
            Chunk[] chunks = Dir switch
            {
                0 => new Chunk[] { new Chunk(center + new Vector3(-ofs, 0, -ofs), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(-ofs, 0, ofs), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(ofs, 0, -ofs), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(ofs, 0, ofs), nS, Dir, LOD, gRad, hmat) },
                1 => new Chunk[] { new Chunk(center + new Vector3(-ofs, 0, ofs), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(-ofs, 0, -ofs), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(ofs, 0, ofs), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(ofs, 0, -ofs), nS, Dir, LOD, gRad, hmat) },
                2 => new Chunk[] { new Chunk(center + new Vector3(ofs, ofs, 0), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(-ofs, ofs, 0), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(ofs, -ofs, 0), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(-ofs, -ofs, 0), nS, Dir, LOD, gRad, hmat) },
                3 => new Chunk[] { new Chunk(center + new Vector3(-ofs, ofs, 0), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(ofs, ofs, 0), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(-ofs, -ofs, 0), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(ofs, -ofs, 0), nS, Dir, LOD, gRad, hmat) },
                4 => new Chunk[] { new Chunk(center + new Vector3(0, ofs, -ofs), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(0, ofs, ofs), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(0, -ofs, -ofs), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(0, -ofs, ofs), nS, Dir, LOD, gRad, hmat) },
                5 => new Chunk[] { new Chunk(center + new Vector3(0, ofs, ofs), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(0, ofs, -ofs), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(0, -ofs, ofs), nS, Dir, LOD, gRad, hmat), new Chunk(center + new Vector3(0, -ofs, -ofs), nS, Dir, LOD, gRad, hmat) },
                _ => throw new System.NotImplementedException()
            };
            return chunks;
        }
        private QuadMesh SubDivide(QuadMesh mesh)
        {
            List<Vector3> v = mesh.vertices.ToList();
            List<int> t = mesh.faces.ToList();
            int mI = t.Count;
            for (int i = 0; i < mI; i += 4)
            {
                int ai = t[i];
                int bi = t[i + 1];
                int ci = t[i + 2];
                int di = t[i + 3];

                int vC = v.Count;

                Vector3 e = ((v[ai] + v[bi]) * 0.5f).normalized * gRad;
                Vector3 f = ((v[bi] + v[ci]) * 0.5f).normalized * gRad;
                Vector3 g = ((v[ci] + v[di]) * 0.5f).normalized * gRad;
                Vector3 h = ((v[di] + v[ai]) * 0.5f).normalized * gRad;
                Vector3 j = ((e + g) * 0.5f).normalized * gRad;
                int ei, fi, gi, hi, ji;
                if (v.Contains(e))
                {
                    ei = v.FindIndex(0, m => m.Equals(e));
                }
                else
                {
                    ei = vC;
                    v.Add(e);
                    vC++;
                }
                if (v.Contains(f))
                {
                    fi = v.FindIndex(0, m => m.Equals(f));
                }
                else
                {
                    fi = vC;
                    v.Add(f);
                    vC++;
                }
                if (v.Contains(g))
                {
                    gi = v.FindIndex(0, m => m.Equals(g));
                }
                else
                {
                    gi = vC;
                    v.Add(g);
                    vC++;
                }
                if (v.Contains(h))
                {
                    hi = v.FindIndex(0, m => m.Equals(h));
                }
                else
                {
                    hi = vC;
                    v.Add(h);
                    vC++;
                }
                ji = vC;
                v.Add(j);
                vC++;
                t[i] = ai;
                t[i + 1] = ei;
                t[i + 2] = ji;
                t[i + 3] = hi;
                t.AddRange(new int[]{ei, bi, fi, ji, hi, ji, gi, di, ji, fi, ci, gi});
            }
            mesh.vertices = v.ToArray();
            mesh.faces = t.ToArray();
            return mesh;
        }
        private QuadMesh GenInitMesh(int Dir, Vector3 center, float size)
        {
            float s = size * 0.5f;
            QuadMesh mesh = new(Dir switch
            {
                0 => new Vector3[]{
                    (new Vector3(-s, 0, -s) + center).normalized * gRad,
                    (new Vector3(-s, 0, s) + center).normalized * gRad,
                    (new Vector3(s, 0, -s) + center).normalized * gRad,
                    (new Vector3(s, 0, s) + center).normalized * gRad
                },
                1 => new Vector3[]{
                    (new Vector3(-s, 0, s) + center).normalized * gRad,
                    (new Vector3(-s, 0, -s) + center).normalized * gRad,
                    (new Vector3(s, 0, s) + center).normalized * gRad,
                    (new Vector3(s, 0, -s) + center).normalized * gRad
                },
                2 => new Vector3[]{
                    (new Vector3(s, s, 0) + center).normalized * gRad,
                    (new Vector3(-s, s, 0) + center).normalized * gRad,
                    (new Vector3(s, -s, 0) + center).normalized * gRad,
                    (new Vector3(-s, -s, 0) + center).normalized * gRad
                },
                3 => new Vector3[]{
                    (new Vector3(-s, s, 0) + center).normalized * gRad,
                    (new Vector3(s, s, 0) + center).normalized * gRad,
                    (new Vector3(-s, -s, 0) + center).normalized * gRad,
                    (new Vector3(s, -s, 0) + center).normalized * gRad
                },
                4 => new Vector3[]{
                    (new Vector3(0, s, -s) + center).normalized * gRad,
                    (new Vector3(0, s, s) + center).normalized * gRad,
                    (new Vector3(0, -s, -s) + center).normalized * gRad,
                    (new Vector3(0, -s, s) + center).normalized * gRad
                },
                5 => new Vector3[]{
                    (new Vector3(0, s, s) + center).normalized * gRad,
                    (new Vector3(0, s, -s) + center).normalized * gRad,
                    (new Vector3(0, -s, s) + center).normalized * gRad,
                    (new Vector3(0, -s, -s) + center).normalized * gRad
                },
                _ => throw new System.NotImplementedException()
            } , new int[] { 0, 1, 3, 2});
            return mesh;
        }
    }


    private Chunk[] chunks;
    public float radius;
    public int mLOD;

    private void Build()
    {
        List<Tuple<CombineInstance, Material>> combineInstances = new();

        // Parcourt les faces principales (chunks racines)
        foreach (var face in chunks)
        {
            face.CollectCombineData(combineInstances);
        }
        Material[] m = new Material[combineInstances.Count];
        CombineInstance[] c = new CombineInstance[combineInstances.Count];
        for (int i = 0; i < combineInstances.Count; i++)
        {
            c[i] = combineInstances[i].Item1;
            m[i] = combineInstances[i].Item2;
        }
        // Crée un mesh global combiné
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(c, false, true);
        combinedMesh.RecalculateBounds();
        // Applique le mesh combiné au MeshFilter principal
        GetComponent<MeshRenderer>().SetMaterials(m.ToList());
        GetComponent<MeshFilter>().mesh = combinedMesh;
    }

    void Start()
    {
        
    }

    void OnEnable()
    {
        Mesh mesh = new();
        chunks = new Chunk[]{
            new Chunk(new Vector3(0, radius, 0), radius * 2, 0, mLOD, radius, sharedMat),
            new Chunk(new Vector3(0, -radius, 0), radius * 2, 1, mLOD, radius, sharedMat),
            new Chunk(new Vector3(0, 0, radius), radius * 2, 2, mLOD, radius, sharedMat),
            new Chunk(new Vector3(0, 0, -radius), radius * 2, 3, mLOD, radius, sharedMat),
            new Chunk(new Vector3(radius, 0, 0), radius * 2, 4, mLOD, radius, sharedMat),
            new Chunk(new Vector3(-radius, 0, 0), radius * 2, 5, mLOD, radius, sharedMat)
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
        if (chunks.Any(c => c.Update(transform.position * -1f))) Build();
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
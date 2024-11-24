using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;

[ExecuteInEditMode]
public class Planet : MonoBehaviour
{
    private Mesh mesh;
    private List<int> chunksReg;
    private class Quads
    {
        List<Vector3> vector3s;
        List<int> tris;
        //lod 0 => Cube;
        //lod 2 => minimal mesh;
    }
    private class Chunk
    {
        private Vector3 center;
        private float size;
        private Mesh cachedMesh;
        private CombineInstance combineData;
        private int LOD = 0;
        private float LOD_threshold;
        private int DIR;
        private Chunk[] chunks;
        private float gRad;

        public void CollectCombineData(List<CombineInstance> combineInstances)
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
                    combineInstances.Add(combine);
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


        public Chunk(Vector3 center, float size, int Dir, int LOD, float gRad)
        {

            this.gRad = gRad;
            this.center = center;
            cachedMesh = SubDivide(SubDivide(GenInitMesh(Dir, center, size)));
            this.size = size;
            this.LOD = LOD;
            this.DIR = Dir;
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
                    chunks = GenChilds(center, DIR, LOD - 1, size, gRad);
                    return true;
                }
            }
            else
            {
                // need to be uncut
                if (chunks != null)
                {
                    chunks.ToList().ForEach(c => c.Kill());
                    chunks = null;

                    return true;
                }
            }
            return false; // No changes needed for this chunk
        }

        public void Kill()
        {
            if (chunks != null) chunks.ToList().ForEach(c => c.Kill()); // Nettoie les enfants
            if (cachedMesh != null) Object.DestroyImmediate(cachedMesh); // Libère la mémoire GPU
            cachedMesh = null;
        }

        private static Chunk[] GenChilds(Vector3 center, int Dir, int LOD, float size, float gRad)
        {
            float nS = size * 0.5f;
            float ofs = nS * 0.5f;
            Chunk[] chunks = Dir switch
            {
                0 => new Chunk[] { new Chunk(center + new Vector3(-ofs, 0, -ofs), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(-ofs, 0, ofs), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(ofs, 0, -ofs), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(ofs, 0, ofs), nS, Dir, LOD, gRad) },
                1 => new Chunk[] { new Chunk(center + new Vector3(-ofs, 0, ofs), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(-ofs, 0, -ofs), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(ofs, 0, ofs), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(ofs, 0, -ofs), nS, Dir, LOD, gRad) },
                2 => new Chunk[] { new Chunk(center + new Vector3(ofs, ofs, 0), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(-ofs, ofs, 0), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(ofs, -ofs, 0), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(-ofs, -ofs, 0), nS, Dir, LOD, gRad) },
                3 => new Chunk[] { new Chunk(center + new Vector3(-ofs, ofs, 0), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(ofs, ofs, 0), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(-ofs, -ofs, 0), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(ofs, -ofs, 0), nS, Dir, LOD, gRad) },
                4 => new Chunk[] { new Chunk(center + new Vector3(0, ofs, -ofs), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(0, ofs, ofs), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(0, -ofs, -ofs), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(0, -ofs, ofs), nS, Dir, LOD, gRad) },
                5 => new Chunk[] { new Chunk(center + new Vector3(0, ofs, ofs), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(0, ofs, -ofs), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(0, -ofs, ofs), nS, Dir, LOD, gRad), new Chunk(center + new Vector3(0, -ofs, -ofs), nS, Dir, LOD, gRad) },
                _ => throw new System.NotImplementedException()
            };
            return chunks;
        }
        private Mesh SubDivide(Mesh mesh)
        {
            List<Vector3> v = new();
            List<int> t = mesh.GetTriangles(0).ToList();
            mesh.GetVertices(v);
            int mI = t.Count;
            for (int i = 0; i < mI; i += 6)
            {
                int a = t[i];
                int b = t[i + 1];
                int c = t[i + 2];
                int d = t[i + 3];

                int vC = v.Count;

                Vector3 e = ((v[a] + v[b]) * 0.5f).normalized * gRad;
                Vector3 f = ((v[a] + v[c]) * 0.5f).normalized * gRad;
                Vector3 g = ((v[b] + v[d]) * 0.5f).normalized * gRad;
                Vector3 h = ((v[c] + v[d]) * 0.5f).normalized * gRad;
                Vector3 j = ((e + h) * 0.5f).normalized * gRad;
                int ei, fi, gi, hi, ji;
                if (v.Contains(e))
                {
                    ei = v.FindIndex(0, e => e.Equals(e));
                }
                else
                {
                    ei = vC;
                    v.Add(e);
                    vC++;
                }
                if (v.Contains(f))
                {
                    fi = v.FindIndex(0, e => e.Equals(f));
                }
                else
                {
                    fi = vC;
                    v.Add(f);
                    vC++;
                }
                if (v.Contains(g))
                {
                    gi = v.FindIndex(0, e => e.Equals(g));
                }
                else
                {
                    gi = vC;
                    v.Add(g);
                    vC++;
                }
                if (v.Contains(h))
                {
                    hi = v.FindIndex(0, e => e.Equals(h));
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
                if (((v[a]+j)*0.5f).sqrMagnitude <= ((e+f)*0.5f).sqrMagnitude)
                {
                    t[i] = a;
                    t[i + 1] = ei;
                    t[i + 2] = fi;
                    t[i + 3] = ji;
                    t[i + 4] = fi;
                    t[i + 5] = ei;
                } else {
                    t[i] = ei;
                    t[i + 1] = ji;
                    t[i + 2] = a;
                    t[i + 3] = fi;
                    t[i + 4] = a;
                    t[i + 5] = ji;
                }
                int[] res = new int[18];
                if (((e+g)*0.5f).sqrMagnitude <= ((v[b]+j)*0.5f).sqrMagnitude)
                {
                    res[0] = ei;
                    res[1] = b;
                    res[2] = ji;
                    res[3] = gi;
                    res[4] = ji;
                    res[5] = b;
                } else {
                    res[0] = b;
                    res[1] = gi;
                    res[2] = ei;
                    res[3] = ji;
                    res[4] = ei;
                    res[5] = gi;
                }
                if (((f+h)*0.5f).sqrMagnitude <= ((j+v[c])*0.5f).sqrMagnitude)
                {
                    res[6] = fi;
                    res[7] = ji;
                    res[8] = c;
                    res[9] = hi;
                    res[10] = c;
                    res[11] = ji;
                } else {
                    res[6] = ji;
                    res[7] = hi;
                    res[8] = fi;
                    res[9] = c;
                    res[10] = fi;
                    res[11] = hi;
                }
                if (((j+v[d])*0.5f).sqrMagnitude <= ((g+h)*0.5f).sqrMagnitude)
                {
                    res[12] = ji;
                    res[13] = gi;
                    res[14] = hi;
                    res[15] = d;
                    res[16] = hi;
                    res[17] = gi;
                } else {
                    res[12] = gi;
                    res[13] = d;
                    res[14] = ji;
                    res[15] = hi;
                    res[16] = ji;
                    res[17] = d;
                }
                t.AddRange(res);
            }
            mesh.SetVertices(v);
            mesh.SetTriangles(t, 0);
            return mesh;
        }
        private Mesh GenInitMesh(int Dir, Vector3 center, float size)
        {
            float s = size * 0.5f;
            Mesh mesh = new();
            mesh.SetVertices(Dir switch
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
            });
            if (((mesh.vertices[1] + mesh.vertices[2])*0.5f).sqrMagnitude <= ((mesh.vertices[0] + mesh.vertices[3])*0.5f).sqrMagnitude)
            mesh.SetTriangles(new int[] { 0, 1, 2, 3, 1, 2 }, 0);
            else
            mesh.SetTriangles(new int[] { 1, 3, 0, 2, 0, 3 }, 0);
            return mesh;
        }
    }


    private Chunk[] chunks;
    public float radius;
    public int mLOD;

    private void Build()
    {
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        // Parcourt les faces principales (chunks racines)
        foreach (var face in chunks)
        {
            face.CollectCombineData(combineInstances);
        }

        // Crée un mesh global combiné
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

        // Applique le mesh combiné au MeshFilter principal
        GetComponent<MeshFilter>().mesh = combinedMesh;
    }

    void Start()
    {

    }

    void OnEnable()
    {
        Mesh mesh = new();
        chunks = new Chunk[]{
            new Chunk(new Vector3(0, radius, 0), radius * 2, 0, mLOD, radius),
            new Chunk(new Vector3(0, -radius, 0), radius * 2, 1, mLOD, radius),
            new Chunk(new Vector3(0, 0, radius), radius * 2, 2, mLOD, radius),
            new Chunk(new Vector3(0, 0, -radius), radius * 2, 3, mLOD, radius),
            new Chunk(new Vector3(radius, 0, 0), radius * 2, 4, mLOD, radius),
            new Chunk(new Vector3(-radius, 0, 0), radius * 2, 5, mLOD, radius)
        };
    }

    void OnDisable()
    {
        chunks.ToList().ForEach(c => c.Kill());
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

    a --e-- b    b --g-- d
    |     / |    | \     |
    |    /  |    |  \    |
    f   j   g :: e   j   h
    |  /    |    |    \  |
    | /     |    |     \ |
    c --h-- d    a --f-- c



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
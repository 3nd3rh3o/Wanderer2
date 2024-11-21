using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


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
        private float[] LOD_DISTS;
        private Mesh mesh;
        private Mesh cachedMesh;
        private int LOD = 0;
        private Chunk[] chunks;


        public Chunk(Vector3 center, float size, int Dir)
        {
            this.center = center;
            mesh = SubDivide(SubDivide(SubDivide(SubDivide(GenInitMesh(Dir, center, size)))));
        }
        // true = needRebuild
        //TODO finish it => QUADTREE !!!
        private bool Update(int parentLOD, Vector3 pPos)
        {
            return false;
        }

        private static Mesh GenInitMesh(int Dir, Vector3 center, float size)
        {
            float s = size * 0.5f;
            Mesh mesh = new();
            mesh.SetVertices(Dir switch
            {
                0 => new Vector3[]{
                    new Vector3(-s, s, -s) + center,
                    new Vector3(-s, s, s) + center,
                    new Vector3(s, s, -s) + center,
                    new Vector3(s, s, s) + center
                },
                1 => new Vector3[]{
                    new Vector3(-s, -s, s) + center,
                    new Vector3(-s, -s, -s) + center,
                    new Vector3(s, -s, -s) + center,
                    new Vector3(s, -s, s) + center
                },
                2 => new Vector3[]{
                    new Vector3(s, s, s) + center,
                    new Vector3(-s, s, s) + center,
                    new Vector3(s, -s, s) + center,
                    new Vector3(-s, -s, s) + center,
                },
                3 => new Vector3[]{
                    new Vector3(-s, s, -s) + center,
                    new Vector3(s, s, -s) + center,
                    new Vector3(-s, -s, -s) + center,
                    new Vector3(s, -s, -s) + center,
                },
                4 => new Vector3[]{
                    new Vector3(s, s, -s) + center,
                    new Vector3(s, s, s) + center,
                    new Vector3(s, -s, -s) + center,
                    new Vector3(s, -s, s) + center,
                },
                5 => new Vector3[]{
                    new Vector3(-s, s, s) + center,
                    new Vector3(-s, s, -s) + center,
                    new Vector3(s, -s, s) + center,
                    new Vector3(-s, -s, -s) + center,
                },
                _ => throw new System.NotImplementedException()
            });
            mesh.SetTriangles(new int[] { 0, 1, 2, 2, 1, 3 }, 0);
            return mesh;
        }
    }

    private static Mesh SubDivide(Mesh mesh)
    {
        List<Vector3> v = new();
        List<int> t = mesh.GetTriangles(0).ToList();
        mesh.GetVertices(v);
        int mI = t.Count;
        for (int i = 0; i < mI; i += 3)
        {
            int a = t[i];
            int b = t[i + 1];
            int c = t[i + 2];

            int vC = v.Count;

            Vector3 ab = (v[a] + v[b]) * 0.5f;
            Vector3 bc = (v[b] + v[c]) * 0.5f;
            Vector3 ca = (v[c] + v[a]) * 0.5f;
            int abi;
            int bci;
            int cai;
            if (v.Contains(ab))
            {
                abi = v.FindIndex(0, e => e.Equals(ab));
            }
            else
            {
                abi = vC;
                v.Add(ab);
                vC++;
            }
            if (v.Contains(bc))
            {
                bci = v.FindIndex(0, e => e.Equals(bc));
            }
            else
            {
                bci = vC;
                v.Add(bc);
                vC++;
            }
            if (v.Contains(ca))
            {
                cai = v.FindIndex(0, e => e.Equals(ca));
            }
            else
            {
                cai = vC;
                v.Add(ca);
                vC++;
            }
            t[i] = abi;
            t[i + 1] = bci;
            t[i + 2] = cai;
            t.AddRange(new int[] { a, abi, bci, abi, b, bci, cai, bci, c });
        }
        mesh.SetVertices(v);
        mesh.SetTriangles(t, 0);
        return mesh;
    }
    private Chunk[] chunks;


    void Start()
    {
        Mesh mesh = new();

    }

    void Update()
    {

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

        Y
        |
        |
       / \
      /   \
     X     Z
    
    up:
            -[-0.5, 0.5, -0.5]
            -[-0.5, 0.5, 0.5]
            -[0.5, 0.5, -0.5]
            -[0.5, 0.5, 0.5]
    down:
            -[-0.5, -0.5, 0.5]
            -[-0.5, -0.5, -0.5]
            -[0.5, -0.5, -0.5]
            -[0.5, -0.5, 0.5]
    front:
            -[0.5, 0.5, 0.5]
            -[-0.5, 0.5, 0.5]
            -[0.5, -0.5, 0.5]
            -[-0.5, -0.5, 0.5]
    back:
            -[-0.5, 0.5, -0.5]
            -[0.5, 0.5, -0.5]
            -[-0.5, -0.5, -0.5]
            -[0.5, -0.5, -0.5]
    left:
            -[0.5, 0.5, -0.5]
            -[0.5, 0.5, 0.5]
            -[0.5, -0.5, -0.5]
            -[0.5, -0.5, 0.5]
    right:
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
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;


public class Chunk
    {
        private struct QuadMesh
        {
            public Vector3[] vertices;
            public Vector3[] normals;
            public Vector2[] uvs;
            public int[] faces;

            /*
            TODO uv (for nhmap)
                 normals
            */

            public QuadMesh(Vector3[] v, Vector2[] uv, int[] f)
            {
                vertices = v;
                uvs = uv;
                faces = f;
                Vector3[] n = new Vector3[v.Length];
                for (int i = 0; i<v.Length; i++) n[i] = v[i].normalized;
                normals = n;
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
        private RenderTexture NHMap;
        private ChunkNHMapCSManager csMan;


        //TODO get Texture here !
        public void CollectCombineData(List<Tuple<CombineInstance, RenderTexture>> combineInstances)
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
                    combineInstances.Add(new(combine, NHMap));

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


        public Chunk(Vector3 center, float size, int Dir, int LOD, float gRad, ChunkNHMapCSManager csMan)
        {

            this.gRad = gRad;
            this.center = center;
            this.size = size;
            this.LOD = LOD;
            DIR = Dir;
            NHMap = new(100, 100, 0);
            NHMap.enableRandomWrite = true;
            NHMap.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            NHMap.Create();
            cachedMesh = ToMesh(GenNHMap(SubDivide(SubDivide(SubDivide(GenInitMesh(Dir, center, size))))));
        }


        private QuadMesh GenNHMap(QuadMesh iMesh)
        {
            if (csMan!=null && NHMap != null)
            {
                csMan.GenMap(NHMap);
            }
            return iMesh;
        }

        private Mesh ToMesh(QuadMesh qMesh)
        {
            Mesh mesh = new();
            int mF = qMesh.faces.Length;
            int[] f = qMesh.faces;
            Vector3[] v = qMesh.vertices;
            Vector3[] n = qMesh.normals;
            Vector2[] uv = qMesh.uvs;
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
            mesh.SetNormals(n);
            mesh.SetTriangles(t, 0);
            mesh.SetUVs(0, uv);
            mesh.RecalculateBounds();
            return mesh;
        }


        //TODO remove return, make it void
        public void Update(Vector3 pPos, Queue<ChunkTask> queue)
        {
            if ((pPos - center).sqrMagnitude <= 3f * size * size)
            {
                // need to be cut;
                if (chunks != null)
                {
                    // give potato to childs
                    chunks.ToList().ForEach(c => c.Update(pPos, queue));
                }
                else
                {
                    queue.Enqueue(new ChunkTask(ChunkTask.TYPE.ADDCHILD, this));
                }
            }
            else
            {
                // need to be uncut
                if (chunks != null)
                {
                    queue.Enqueue(new ChunkTask(ChunkTask.TYPE.KILLCHILD, this));
                }
            }
             // No changes needed for this chunk
        }

        public void ConsumeChunkTask(ChunkTask task)
        {
            switch (task.type)
            {
                case ChunkTask.TYPE.ADDCHILD:
                    if (LOD <= 0) return;
                    chunks = GenChilds(center, DIR, LOD - 1, size, gRad);
                    return;
                case ChunkTask.TYPE.KILLCHILD:
                    chunks.ToList().ForEach(c => c.Kill());
                    chunks = null;
                    return;
            }
        }
        public void Kill()
        {
            if (NHMap)
            {
                NHMap.Release();
                NHMap = null;
            }
            chunks?.ToList().ForEach(c => c.Kill()); // Nettoie les enfants
            if (cachedMesh != null) MonoBehaviour.DestroyImmediate(cachedMesh); // Libère la mémoire GPU
            cachedMesh = null;
        }

        private Chunk[] GenChilds(Vector3 center, int Dir, int LOD, float size, float gRad)
        {
            float nS = size * 0.5f;
            float ofs = nS * 0.5f;
            Chunk[] chunks = Dir switch
            {
                0 => new Chunk[] { new Chunk(center + new Vector3(-ofs, 0, -ofs), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(-ofs, 0, ofs), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(ofs, 0, -ofs), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(ofs, 0, ofs), nS, Dir, LOD, gRad, csMan) },
                1 => new Chunk[] { new Chunk(center + new Vector3(-ofs, 0, ofs), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(-ofs, 0, -ofs), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(ofs, 0, ofs), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(ofs, 0, -ofs), nS, Dir, LOD, gRad, csMan) },
                2 => new Chunk[] { new Chunk(center + new Vector3(ofs, ofs, 0), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(-ofs, ofs, 0), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(ofs, -ofs, 0), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(-ofs, -ofs, 0), nS, Dir, LOD, gRad, csMan) },
                3 => new Chunk[] { new Chunk(center + new Vector3(-ofs, ofs, 0), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(ofs, ofs, 0), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(-ofs, -ofs, 0), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(ofs, -ofs, 0), nS, Dir, LOD, gRad, csMan) },
                4 => new Chunk[] { new Chunk(center + new Vector3(0, ofs, -ofs), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(0, ofs, ofs), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(0, -ofs, -ofs), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(0, -ofs, ofs), nS, Dir, LOD, gRad, csMan) },
                5 => new Chunk[] { new Chunk(center + new Vector3(0, ofs, ofs), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(0, ofs, -ofs), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(0, -ofs, ofs), nS, Dir, LOD, gRad, csMan), new Chunk(center + new Vector3(0, -ofs, -ofs), nS, Dir, LOD, gRad, csMan) },
                _ => throw new System.NotImplementedException()
            };
            return chunks;
        }
        private QuadMesh SubDivide(QuadMesh mesh)
        {
            List<Vector3> v = mesh.vertices.ToList();
            List<Vector2> uv = mesh.uvs.ToList();
            List<int> t = mesh.faces.ToList();
            List<Vector3> n = mesh.normals.ToList();
            int mI = t.Count;
            for (int i = 0; i < mI; i += 4)
            {
                int ai = t[i];
                int bi = t[i + 1];
                int ci = t[i + 2];
                int di = t[i + 3];

                int vC = v.Count;

                Vector3 en = ((v[ai] + v[bi]) * 0.5f).normalized;
                Vector3 e = en * gRad;
                Vector2 eUV = (uv[ai] + uv[bi]) * 0.5f;

                Vector3 fn = ((v[bi] + v[ci]) * 0.5f).normalized;
                Vector3 f = fn * gRad;
                Vector2 fUV = (uv[bi] + uv[ci]) * 0.5f;

                Vector3 gn = ((v[ci] + v[di]) * 0.5f).normalized;
                Vector3 g = gn * gRad;
                Vector2 gUV = (uv[ci] + uv[di]) * 0.5f;

                Vector3 hn = ((v[di] + v[ai]) * 0.5f).normalized;
                Vector3 h = hn * gRad;
                Vector2 hUV = (uv[di] + uv[ai]) * 0.5f;

                Vector3 jn = ((e + g) * 0.5f).normalized;
                Vector3 j = jn * gRad;
                Vector2 jUV = (eUV + gUV) * 0.5f;

                int ei, fi, gi, hi, ji;
                if (v.Contains(e))
                {
                    ei = v.FindIndex(0, m => m.Equals(e));
                }
                else
                {
                    ei = vC;
                    v.Add(e);
                    n.Add(en);
                    uv.Add(eUV);
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
                    n.Add(fn);
                    uv.Add(fUV);
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
                    n.Add(gn);
                    uv.Add(gUV);
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
                    n.Add(hn);
                    uv.Add(hUV);
                    vC++;
                }
                ji = vC;
                v.Add(j);
                n.Add(jn);
                uv.Add(jUV);
                vC++;
                t[i] = ai;
                t[i + 1] = ei;
                t[i + 2] = ji;
                t[i + 3] = hi;
                t.AddRange(new int[]{ei, bi, fi, ji, hi, ji, gi, di, ji, fi, ci, gi});
            }
            mesh.vertices = v.ToArray();
            mesh.normals = n.ToArray();
            mesh.uvs = uv.ToArray();
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
            }, new Vector2[] {new(0,0), new(1, 0), new(1, 0), new(1, 1)}, new int[] { 0, 1, 3, 2});
            return mesh;
        }
    }

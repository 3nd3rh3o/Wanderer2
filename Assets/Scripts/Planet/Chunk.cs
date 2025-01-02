using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.Collections;


public class Chunk
{
    private struct QuadMesh
    {
        public Vector3 origin;
        public Vector3 mx;
        public Vector3 my;
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector2[] uvs;
        public Color[] colors;
        public int[] faces;

        public QuadMesh(Vector3[] v, Vector2[] uv, int[] f, Vector3 origin, Vector3 mx, Vector3 my)
        {
            vertices = v;
            uvs = uv;
            faces = f;
            Vector3[] n = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++) n[i] = v[i].normalized;
            normals = n;
            this.origin = origin;
            this.mx = mx;
            this.my = my;
            colors = null;
        }
    }
    private Vector3 center;
    private float size;
    private Mesh cachedMesh;
    private CombineInstance combineData = new();
    private int LOD = 0;
    private float LOD_threshold;
    private int DIR;
    private Chunk[] chunks;
    private float gRad;
    private bool isRoot;
    private RenderTexture albedo;
    private RenderTexture ambientOcclusion;
    private RenderTexture metalic;
    private RenderTexture roughness;
    private RenderTexture normalMap;
    private RenderTexture height;
    private Texture3D[][] refs;
    private ChunkNHMapCSManager csMan;
    private Vector3 geoCenter;
    private RenderTexture[] parent_tex;
    private int posRelToParent;

    //how deep in the tree
    private int lvl;

    private float BSca;
    private float BMul;
    private Vector3 BOff;

    private Biome[] biomes;

    public void CollectCombineData(List<Tuple<CombineInstance, RenderTexture, RenderTexture, RenderTexture, RenderTexture, RenderTexture, RenderTexture>> combineInstances)
    {
        if (chunks == null)
        {
            // Chunk terminal, ajoute son mesh
            if (cachedMesh != null)
            {
                combineData = new CombineInstance
                {
                    mesh = cachedMesh,
                    transform = Matrix4x4.identity // Identité si tout est en local space
                };
                combineInstances.Add(new(combineData, albedo, normalMap, height, metalic, roughness, ambientOcclusion));
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


    public Chunk(Vector3 center, float size, int Dir, int LOD, float gRad, ChunkNHMapCSManager csMan, float BSca, float BMul, Vector3 BOff, Biome[] biomes, Texture3D[][] refs, int lvl)
    {
        this.lvl = lvl;
        this.gRad = gRad;
        this.center = center;
        this.size = size;
        this.LOD = LOD;
        DIR = Dir;
        this.csMan = csMan;
        this.BOff = BOff;
        this.BSca = BSca;
        this.BMul = BMul;
        this.biomes = biomes;
        int MAX_TEX_SIZE = 256;
        isRoot = true;

        albedo = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
        albedo.enableRandomWrite = true;
        albedo.Create();
        ambientOcclusion = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
        ambientOcclusion.enableRandomWrite = true;
        ambientOcclusion.Create();
        metalic = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
        metalic.enableRandomWrite = true;
        metalic.Create();
        roughness = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
        roughness.enableRandomWrite = true;
        roughness.Create();
        normalMap = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
        normalMap.enableRandomWrite = true;
        normalMap.Create();
        height = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
        height.enableRandomWrite = true;
        height.Create();

        this.refs = refs;

        cachedMesh = ToMesh(GenNHMap(SubDivide(SubDivide(SubDivide(GenInitMesh(Dir, center, size))))));
    }

    public Chunk(Vector3 center, float size, int Dir, int LOD, float gRad, ChunkNHMapCSManager csMan, float BSca, float BMul, Vector3 BOff, Biome[] biomes, Texture3D[][] refs, int lvl, RenderTexture[] parent_tex, int posRelToParent)
    {
        this.lvl = lvl;
        this.gRad = gRad;
        this.center = center;
        this.size = size;
        this.LOD = LOD;
        DIR = Dir;
        this.csMan = csMan;
        this.BOff = BOff;
        this.BSca = BSca;
        this.BMul = BMul;
        this.biomes = biomes;
        int MAX_TEX_SIZE = 256;
        isRoot = false;
        this.parent_tex = parent_tex;
        this.posRelToParent = posRelToParent;

        albedo = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
        albedo.enableRandomWrite = true;
        albedo.Create();
        ambientOcclusion = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
        ambientOcclusion.enableRandomWrite = true;
        ambientOcclusion.Create();
        metalic = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
        metalic.enableRandomWrite = true;
        metalic.Create();
        roughness = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
        roughness.enableRandomWrite = true;
        roughness.Create();
        normalMap = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
        normalMap.enableRandomWrite = true;
        normalMap.Create();
        height = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
        height.enableRandomWrite = true;
        height.Create();

        this.refs = refs;

        cachedMesh = ToMesh(GenNHMap(SubDivide(SubDivide(SubDivide(GenInitMesh(Dir, center, size))))));
    }


    private QuadMesh GenNHMap(QuadMesh iMesh)
    {
        iMesh.colors = new Color[iMesh.vertices.Length];
        if (csMan != null)
        {
            if (isRoot) csMan.GenMap(refs, albedo, ambientOcclusion, metalic, roughness, normalMap, height, LOD, lvl, iMesh.vertices, iMesh.normals, iMesh.colors, iMesh.origin, iMesh.mx, iMesh.my, gRad, BSca, BMul, BOff, biomes, null, -1);
            else csMan.GenMap(refs, albedo, ambientOcclusion, metalic, roughness, normalMap, height, LOD, lvl, iMesh.vertices, iMesh.normals, iMesh.colors, iMesh.origin, iMesh.mx, iMesh.my, gRad, BSca, BMul, BOff, biomes, parent_tex, posRelToParent);
        }
        return iMesh;
    }

    private Mesh ToMesh(QuadMesh qMesh)
    {
        //TODO From normals and UVs, deduce tangent !
        Mesh mesh = new();
        int mF = qMesh.faces.Length;
        int[] f = qMesh.faces;
        Vector3[] v = qMesh.vertices;
        Vector3[] n = qMesh.normals;
        Vector2[] uv = qMesh.uvs;
        Vector4[] tan = new Vector4[qMesh.vertices.Length];
        List<int> t = new((int)(mF * 1.5f));
        // reading quads to put them in triangles
        for (int i = 0; i < mF; i += 4)
        {
            int a = f[i];
            int b = f[i + 1];
            int c = f[i + 2];
            int d = f[i + 3];
            Vector3 tan_a = Vector3.Cross(n[a], v[d] - v[a]).normalized;
            Vector3 tan_b = Vector3.Cross(n[b], v[c] - v[b]).normalized;
            Vector3 tan_c = Vector3.Cross(n[c], v[d] - v[a]).normalized;
            Vector3 tan_d = Vector3.Cross(n[d], v[c] - v[b]).normalized;

            tan[a] = new Vector4(tan_a.x, tan_a.y, tan_a.z, 1);
            tan[b] = new Vector4(tan_b.x, tan_b.y, tan_b.z, 1);
            tan[c] = new Vector4(tan_c.x, tan_c.y, tan_c.z, 1);
            tan[d] = new Vector4(tan_d.x, tan_d.y, tan_d.z, 1);



            t.AddRange(((v[b] + v[d]) * 0.5f).sqrMagnitude <= ((v[a] + v[c]) * 0.5f).sqrMagnitude ? new int[] { a, b, d, c, d, b } : new int[] { a, b, c, c, d, a });
        }
        mesh.SetVertices(v);
        mesh.SetNormals(n);
        mesh.SetTriangles(t, 0);
        mesh.SetUVs(0, uv);
        mesh.SetColors(qMesh.colors);
        mesh.RecalculateBounds();
        mesh.SetTangents(tan);
        geoCenter = mesh.bounds.center;
        return mesh;
    }
    public void Update(int LOD)
    {
        this.LOD = LOD;
        if (LOD == 0)
        {
            chunks?.ToList().ForEach(c => c.Kill());
            chunks = null;
            cachedMesh = ToMesh(GenNHMap(SubDivide(SubDivide(SubDivide(GenInitMesh(DIR, center, size))))));
        }
        else
        {
            if (chunks == null)
            {
                chunks = GenChilds(center, DIR, this.LOD - 1, size, gRad);
            }
            else
            {
                chunks.ToList().ForEach(c => c.Update(LOD - 1));
            }
        }
    }
    public void Update(Vector3 pPos, Queue<ChunkTask> queue)
    {
        if ((pPos - geoCenter).sqrMagnitude <= 3f * size * size)
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
                chunks.ToList().ForEach(c => c = null);
                chunks = null;
                return;
        }
    }
    public void Kill()
    {
        if (albedo)
        {
            albedo.Release();
            albedo = null;
        }
        if (ambientOcclusion)
        {
            ambientOcclusion.Release();
            ambientOcclusion = null;
        }
        if (metalic)
        {
            metalic.Release();
            metalic = null;
        }
        if (roughness)
        {
            roughness.Release();
            roughness = null;
        }
        if (normalMap)
        {
            normalMap.Release();
            normalMap = null;
        }
        if (height)
        {
            height.Release();
            height = null;
        }
        chunks?.ToList().ForEach(c => c.Kill()); // Nettoie les enfants

        cachedMesh?.Clear();
#if UNITY_EDITOR
        MonoBehaviour.DestroyImmediate(cachedMesh);
#else
        MonoBehaviour.Destroy(cachedMesh); // Libère la mémoire GPU
#endif
        cachedMesh = null;
        if (combineData.mesh != null)
        {
            combineData.mesh.Clear();
#if UNITY_EDITOR
            MonoBehaviour.DestroyImmediate(combineData.mesh);
#else
        MonoBehaviour.Destroy(combineData.mesh); // Libère la mémoire GPU
#endif
            combineData.mesh = null;
            chunks = null;
        }

    }

    private Chunk[] GenChilds(Vector3 center, int Dir, int LOD, float size, float gRad)
    {
        float nS = size * 0.5f;
        float ofs = nS * 0.5f;
        RenderTexture[] prt = new RenderTexture[] { albedo, normalMap, height, metalic, roughness, ambientOcclusion };
        Chunk[] chunks = Dir switch
        {
            0 => new Chunk[] { new Chunk(center + new Vector3(-ofs, 0, -ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 0), new Chunk(center + new Vector3(-ofs, 0, ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 1), new Chunk(center + new Vector3(ofs, 0, -ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 2), new Chunk(center + new Vector3(ofs, 0, ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 3) },
            1 => new Chunk[] { new Chunk(center + new Vector3(-ofs, 0, ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 0), new Chunk(center + new Vector3(-ofs, 0, -ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 1), new Chunk(center + new Vector3(ofs, 0, ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 2), new Chunk(center + new Vector3(ofs, 0, -ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 3) },
            2 => new Chunk[] { new Chunk(center + new Vector3(ofs, ofs, 0), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 0), new Chunk(center + new Vector3(-ofs, ofs, 0), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 1), new Chunk(center + new Vector3(ofs, -ofs, 0), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 2), new Chunk(center + new Vector3(-ofs, -ofs, 0), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 3) },
            3 => new Chunk[] { new Chunk(center + new Vector3(-ofs, ofs, 0), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 0), new Chunk(center + new Vector3(ofs, ofs, 0), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 1), new Chunk(center + new Vector3(-ofs, -ofs, 0), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 2), new Chunk(center + new Vector3(ofs, -ofs, 0), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 3) },
            4 => new Chunk[] { new Chunk(center + new Vector3(0, ofs, -ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 0), new Chunk(center + new Vector3(0, ofs, ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 1), new Chunk(center + new Vector3(0, -ofs, -ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 2), new Chunk(center + new Vector3(0, -ofs, ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 3) },
            5 => new Chunk[] { new Chunk(center + new Vector3(0, ofs, ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 0), new Chunk(center + new Vector3(0, ofs, -ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 1), new Chunk(center + new Vector3(0, -ofs, ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 2), new Chunk(center + new Vector3(0, -ofs, -ofs), nS, Dir, LOD, gRad, csMan, BSca, BMul, BOff, biomes, refs, lvl + 1, prt, 3) },
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
            t.AddRange(new int[] { ei, bi, fi, ji, hi, ji, gi, di, ji, fi, ci, gi });
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
        Vector3[] oxy = Dir switch
        {
            0 => new Vector3[]{
                    new Vector3(-s, 0, -s) + center,
                    new Vector3(-s, 0, s) + center,
                    new Vector3(s, 0, -s) + center
                },
            1 => new Vector3[]{
                    new Vector3(-s, 0, s) + center,
                    new Vector3(-s, 0, -s) + center,
                    new Vector3(s, 0, s) + center
                },
            2 => new Vector3[]{
                    new Vector3(s, s, 0) + center,
                    new Vector3(-s, s, 0) + center,
                    new Vector3(s, -s, 0) + center
                },
            3 => new Vector3[]{
                    new Vector3(-s, s, 0) + center,
                    new Vector3(s, s, 0) + center,
                    new Vector3(-s, -s, 0) + center
                },
            4 => new Vector3[]{
                    new Vector3(0, s, -s) + center,
                    new Vector3(0, s, s) + center,
                    new Vector3(0, -s, -s) + center
                },
            5 => new Vector3[]{
                    new Vector3(0, s, s) + center,
                    new Vector3(0, s, -s) + center,
                    new Vector3(0, -s, s) + center
                },
            _ => throw new System.NotImplementedException()
        };
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
        }, new Vector2[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1) }, new int[] { 0, 1, 3, 2 }, oxy[0], oxy[1], oxy[2]);
        return mesh;
    }
}

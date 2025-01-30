using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wanderer
{
    public partial class TeluricGenerator
    {
        public partial class Chunk
        {
            public class QuadMesh
            {
                public Vector3 origin;
                public Vector3 U;
                public Vector3 V;
                public Vector3[] vertices;
                public Vector3[] normals;
                public Vector2[] uvs;
                public Color[] vertexColor;
                public int[] quads;


                public QuadMesh
                (
                    Vector3[] v,
                    Vector2[] uv,
                    int[] q,
                    Vector3 o,
                    Vector3 uvU,
                    Vector3 uvV
                )
                {
                    vertices = v;
                    uvs = uv;
                    quads = q;
                    Vector3[] n = new Vector3[v.Length];
                    for (int i = 0; i < v.Length; i++) n[i] = v[i].normalized;
                    normals = n;
                    origin = o;
                    U = uvU;
                    V = uvV;
                    vertexColor = null;
                }

            }
            private int Dir;
            private float Size;
            private int posRelToParent;
            private Vector3 center;
            private Vector3 geoCenter;
            private int LOD;
            private int mLOD;

            private Chunk[] childrens;
            private Mesh cachedMesh;
            private CombineInstance combine = new();

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
                    (new Vector3(-s, 0, -s) + center).normalized * settings.radius,
                    (new Vector3(-s, 0, s) + center).normalized * settings.radius,
                    (new Vector3(s, 0, -s) + center).normalized * settings.radius,
                    (new Vector3(s, 0, s) + center).normalized * settings.radius
                },
                    1 => new Vector3[]{
                    (new Vector3(-s, 0, s) + center).normalized * settings.radius,
                    (new Vector3(-s, 0, -s) + center).normalized * settings.radius,
                    (new Vector3(s, 0, s) + center).normalized * settings.radius,
                    (new Vector3(s, 0, -s) + center).normalized * settings.radius
                },
                    2 => new Vector3[]{
                    (new Vector3(s, s, 0) + center).normalized * settings.radius,
                    (new Vector3(-s, s, 0) + center).normalized * settings.radius,
                    (new Vector3(s, -s, 0) + center).normalized * settings.radius,
                    (new Vector3(-s, -s, 0) + center).normalized * settings.radius
                },
                    3 => new Vector3[]{
                    (new Vector3(-s, s, 0) + center).normalized * settings.radius,
                    (new Vector3(s, s, 0) + center).normalized * settings.radius,
                    (new Vector3(-s, -s, 0) + center).normalized * settings.radius,
                    (new Vector3(s, -s, 0) + center).normalized * settings.radius
                },
                    4 => new Vector3[]{
                    (new Vector3(0, s, -s) + center).normalized * settings.radius,
                    (new Vector3(0, s, s) + center).normalized * settings.radius,
                    (new Vector3(0, -s, -s) + center).normalized * settings.radius,
                    (new Vector3(0, -s, s) + center).normalized * settings.radius
                },
                    5 => new Vector3[]{
                    (new Vector3(0, s, s) + center).normalized * settings.radius,
                    (new Vector3(0, s, -s) + center).normalized * settings.radius,
                    (new Vector3(0, -s, s) + center).normalized * settings.radius,
                    (new Vector3(0, -s, -s) + center).normalized * settings.radius
                },
                    _ => throw new System.NotImplementedException()
                }, new Vector2[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1) }, new int[] { 0, 1, 3, 2 }, oxy[0], oxy[1], oxy[2]);
                return mesh;
            }

            private QuadMesh SubDivide(QuadMesh mesh)
            {
                List<Vector3> v = mesh.vertices.ToList();
                List<Vector2> uv = mesh.uvs.ToList();
                List<int> t = mesh.quads.ToList();
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
                    Vector3 e = en * settings.radius;
                    Vector2 eUV = (uv[ai] + uv[bi]) * 0.5f;

                    Vector3 fn = ((v[bi] + v[ci]) * 0.5f).normalized;
                    Vector3 f = fn * settings.radius;
                    Vector2 fUV = (uv[bi] + uv[ci]) * 0.5f;

                    Vector3 gn = ((v[ci] + v[di]) * 0.5f).normalized;
                    Vector3 g = gn * settings.radius;
                    Vector2 gUV = (uv[ci] + uv[di]) * 0.5f;

                    Vector3 hn = ((v[di] + v[ai]) * 0.5f).normalized;
                    Vector3 h = hn * settings.radius;
                    Vector2 hUV = (uv[di] + uv[ai]) * 0.5f;

                    Vector3 jn = ((e + g) * 0.5f).normalized;
                    Vector3 j = jn * settings.radius;
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
                mesh.quads = t.ToArray();
                return mesh;
            }


            private Mesh ToMesh(QuadMesh qMesh)
            {
                //TODO From normals and UVs, deduce tangent !
                Mesh mesh = new();
                int mF = qMesh.quads.Length;
                int[] f = qMesh.quads;
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
                mesh.SetColors(qMesh.vertexColor);
                mesh.RecalculateBounds();
                mesh.SetTangents(tan);
                geoCenter = mesh.bounds.center;
                return mesh;
            }
        }
    }
}
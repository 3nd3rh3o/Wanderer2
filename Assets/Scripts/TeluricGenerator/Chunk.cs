using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wanderer
{
    public partial class TeluricGenerator
    {
        public class Chunk
        {
            public struct QuadMesh
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
            
            private Chunk[] childrens;
            private Mesh cachedMesh;
            private CombineInstance combine = new();
            private ChunkTextures textures;

            public Chunk()
            {

            }

            /// <summary>
            /// Used to collect sub-chunks meshs and pass them to the parent chunk, or the planet if we are on top level.
            /// </summary>
            public void CollectCombineData(List<Tuple<CombineInstance, ChunkTextures>> combineInstances)
            {
                if (childrens == null)
                {
                    if (cachedMesh != null)
                    {
                        combine = new CombineInstance
                        {
                            mesh = cachedMesh,
                            transform = Matrix4x4.identity
                        };
                        combineInstances.Add(new(combine, textures));
                    }
                }
                else
                {
                    foreach(var child in childrens) child.CollectCombineData(combineInstances);
                }
            }
            
        }
    }
}
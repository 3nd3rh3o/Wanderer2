using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static Wanderer.TeluricGenerator.Chunk;
namespace Wanderer
{
    public partial class TeluricGenerator
    {
        public static ComputeShader cs = Resources.Load<ComputeShader>("TelluricMeshGenerator");
        private PlanetSettings settings;
        private Chunk[] chunks;

        /// <summary>
        /// Compute shader caller, will deform mesh and generate textures
        /// </summary>
        /// <param name="properties"> Texture holder </param>
        /// <param name="mesh"> Model </param>
        /// <param name="settings"> Global settings of the planet </param>
        public static void Generate(ChunkTextures properties, QuadMesh mesh, PlanetSettings settings)
        {
            return;
            ComputeBuffer vBuff = CBuffHelper.Vec3Buff(mesh.vertices);
            ComputeBuffer nBuff = CBuffHelper.Vec3Buff(mesh.normals);
            ComputeBuffer cBuff = CBuffHelper.ColBuff(mesh.vertexColor);

            cs.SetBuffer(0, "_vertices", vBuff);
            cs.SetBuffer(0, "", nBuff);
            cs.SetBuffer(0, "", cBuff);

            vBuff.Release();
            nBuff.Release();
            cBuff.Release();





        }


        public void Build(MeshFilter meshFilter, Material m, MeshRenderer meshRenderer)
        {
#if UNITY_EDITOR
            Mesh mesh = meshFilter.sharedMesh;
#else
            Mesh mesh = meshFilter.mesh;
#endif
            mesh.Clear();

            List<Tuple<CombineInstance, ChunkTextures>> chunkData = new();
            foreach (var face in chunks)
            {
                face.CollectCombineData(chunkData);
            }
            CombineInstance[] combines = new CombineInstance[chunkData.Count];
            meshRenderer.sharedMaterials = new Material[chunkData.Count];
            for (int i = 0; i < chunkData.Count; i++)
            {
                //Mesh
                combines[i] = chunkData[i].Item1;
                //Mat
                meshRenderer.sharedMaterials[i] = m;
                MaterialPropertyBlock mpb = new();
                mpb.SetTexture("_BaseMap", chunkData[i].Item2.albedo);
                meshRenderer.SetPropertyBlock(mpb, i);
            }
            mesh.CombineMeshes(combines.ToArray(), false, true);
        }


        public TeluricGenerator(PlanetSettings settings)
        {
            this.settings = settings;
            chunks = new Chunk[]{
                new(0, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, settings.radius, 0), settings),
                new(1, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, -settings.radius, 0), settings),
                new(2, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, 0, settings.radius), settings),
                new(3, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, 0, -settings.radius), settings),
                new(4, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(settings.radius, 0, 0), settings),
                new(5, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(-settings.radius, 0, 0), settings)
            };
        }

        public void Clear()
        {
            chunks?.ToList().ForEach(c => c.Kill());
            chunks?.ToList().ForEach(c => c = null);
            chunks = null;
        }

        public void UpdateSettings(PlanetSettings settings)
        {
            this.settings = settings;
            chunks = new Chunk[]{
                new(0, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, settings.radius, 0), settings),
                new(1, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, -settings.radius, 0), settings),
                new(2, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, 0, settings.radius), settings),
                new(3, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, 0, -settings.radius), settings),
                new(4, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(settings.radius, 0, 0), settings),
                new(5, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(-settings.radius, 0, 0), settings)
            };
        }


        public partial class Chunk
        {
            private PlanetSettings settings;
            private ChunkTextures textures;

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
                    foreach (var child in childrens) child.CollectCombineData(combineInstances);
                }
            }


            /// <summary>
            /// Create a new Chunk, and initialize it.
            /// </summary>
            /// <param name="Dir">Direction of the face</param>
            /// <param name="Size">Side length of the plane</param>
            /// <param name="LOD">How deep we are in the quadtree</param>
            /// <param name="mLOD">Max quadtree depth</param>
            /// <param name="center">Initial center of the quad</param>
            /// <param name="settings">Global parameters of the planet</param>
            /// <param name="posRelToParent"> Position of the chunk inside his parent</param>
            public Chunk(int Dir, float Size, int LOD, int mLOD, Vector3 center, PlanetSettings settings)
            {
                this.Dir = Dir;
                this.Size = Size;
                this.LOD = LOD;
                this.mLOD = mLOD;
                this.center = center;
                this.settings = settings;
                textures = new();
                QuadMesh q = SubDivide(SubDivide(SubDivide(SubDivide(GenInitMesh(Dir, center, Size)))));
                Generate(textures, q, settings);
                cachedMesh = ToMesh(q);
            }
            /// <summary>
            /// Destroy this chunk(and it's childrens) and free his resources
            /// </summary>
            public void Kill()
            {
                textures.Clear();
                cachedMesh.Clear();
                cachedMesh = null;
                combine.mesh.Clear();
                combine.mesh = null;
                childrens?.ToList().ForEach(c => c.Kill());
                childrens?.ToList().ForEach(c => c = null);
                childrens = null;
#if UNITY_EDITOR
                MonoBehaviour.DestroyImmediate(cachedMesh);
                MonoBehaviour.DestroyImmediate(combine.mesh);
#else
                
                MonoBehaviour.Destroy(cachedMesh);
                MonoBehaviour.Destroy(combine.mesh);
#endif
            }

        }


    }
}
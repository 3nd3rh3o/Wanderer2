using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static Wanderer.TeluricGenerator.Chunk;
using static Wanderer.TexBuildingPass;
namespace Wanderer
{
    public partial class TeluricGenerator
    {
        private PlanetSettings settings;
        private Chunk[] chunks;
        private Material[] surfmats;
        private Queue<ChunkTask> queue;

        /// <summary>
        /// Compute shader caller, will deform mesh and generate textures
        /// </summary>
        /// <param name="properties"> Texture holder </param>
        /// <param name="mesh"> Model </param>
        /// <param name="settings"> Global settings of the planet </param>
        public static void GenerateTopo(ChunkTextures properties, QuadMesh mesh, PlanetSettings settings)
        {
            ComputeShader cs = settings.biomes.planetTopoCS;
            mesh.vertexColor = new Color[mesh.vertices.Length];
            //RW
            ComputeBuffer vBuff = CBuffHelper.Vec3Buff(mesh.vertices);
            cs.SetBuffer(0, "_vertices", vBuff);

            ComputeBuffer nBuff = CBuffHelper.Vec3Buff(mesh.normals);
            cs.SetBuffer(0, "_normals", nBuff);

            ComputeBuffer cBuff = CBuffHelper.ColBuff(mesh.vertexColor);
            cs.SetBuffer(0, "_color", cBuff);

            float4[] biomesMinPreds = settings.biomes.CollectMinPreds();
            ComputeBuffer BminP = CBuffHelper.Float4Buff(biomesMinPreds);
            cs.SetBuffer(0, "_minPredicates", BminP);

            float4[] biomesMaxPreds = settings.biomes.CollectMaxPreds();
            ComputeBuffer BmaxP = CBuffHelper.Float4Buff(biomesMaxPreds);
            cs.SetBuffer(0, "_maxPredicates", BmaxP);

            cs.SetInt("_vNum", mesh.vertices.Length);

            cs.SetInt("_numBiomes", settings.biomes.biomes.Length);

            cs.SetFloat("_scale", settings.biomes.Scale);

            cs.SetVector("_offset", settings.biomes.Offset);

            cs.SetFloat("_bRad", settings.radius);


            float[] blendingFactors = settings.biomes.CollectBlendingFactors();
            ComputeBuffer blendingFactorBuff = CBuffHelper.FloatBuff(blendingFactors);
            cs.SetBuffer(0, "_blendingFactor", blendingFactorBuff);


            float[] biomeScales = settings.biomes.CollectBiomeScales();
            ComputeBuffer biomeScaleBuff = CBuffHelper.FloatBuff(biomeScales);
            cs.SetBuffer(0, "_BiomeScale", biomeScaleBuff);

            float[] biomesMul = settings.biomes.CollectBiomeMul();
            ComputeBuffer biomesMulBuff = CBuffHelper.FloatBuff(biomesMul);
            cs.SetBuffer(0, "_BiomeMul", biomesMulBuff);

            float[] biomesNL = settings.biomes.CollectBiomeNumLayers();
            ComputeBuffer biomesNLBuff = CBuffHelper.FloatBuff(biomesNL);
            cs.SetBuffer(0, "_BiomeNL", biomesNLBuff);

            Vector3[] biomesOffset = settings.biomes.CollectBiomeOffset();
            ComputeBuffer biomesOffsetBuff = CBuffHelper.Vec3Buff(biomesOffset);
            cs.SetBuffer(0, "_BiomeOffset", biomesOffsetBuff);

            float[] biomesPersistence = settings.biomes.CollectBiomePersistence();
            ComputeBuffer biomesPersistenceBuff = CBuffHelper.FloatBuff(biomesPersistence);
            cs.SetBuffer(0, "_BiomePersistence", biomesPersistenceBuff);

            float[] biomesLacunarity = settings.biomes.CollectBiomeLacunarity();
            ComputeBuffer biomesLacunarityBuff = CBuffHelper.FloatBuff(biomesLacunarity);
            cs.SetBuffer(0, "_BiomeLacunarity", biomesLacunarityBuff);

            float[] biomesVShift = settings.biomes.CollectBiomeVShift();
            ComputeBuffer biomesVShiftBuff = CBuffHelper.FloatBuff(biomesVShift);
            cs.SetBuffer(0, "_BiomeVShift", biomesVShiftBuff);


            Vector3[] biomesDebugCol = settings.biomes.CollectDebugCol();
            ComputeBuffer biomesDebugColBuff = CBuffHelper.Vec3Buff(biomesDebugCol);
            cs.SetBuffer(0, "_BiomeCol", biomesDebugColBuff);

            cs.Dispatch(0, mesh.vertices.Length, 1, 1);
            
            //prep biome tex []
            RenderTexture biomesTempTex = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);
            biomesTempTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            biomesTempTex.volumeDepth = settings.biomes.biomes.Length;
            biomesTempTex.enableRandomWrite = true;
            biomesTempTex.Create();


            // gen each biome tex in the []
            for (int i = 0; i < settings.biomes.biomes.Length; i++)
            {
                cs.SetTexture(1, "_tex", biomesTempTex);
                cs.SetInt("_biomeID", i);
                int numPasses = settings.biomes.biomes[i].terrainTextureBuilders.baseTexture.BuildingPass.Count;
                for (int j = 0; j < numPasses; j++)
                {
                    PassType passType = settings.biomes.biomes[i].terrainTextureBuilders.baseTexture.BuildingPass[j].passType;
                    switch (passType)
                    {
                        case PassType.Fill:
                            cs.SetInt("_passType", 0);
                            cs.SetVector("_col", ((TexBuildingPassFill)settings.biomes.biomes[i].terrainTextureBuilders.baseTexture.BuildingPass[j]).color);
                            break;
                    }
                    cs.Dispatch(1, 256/8, 256/8, 1);
                }
            }
            //Launch assembly of all rendered tex;



            //Cleanup
            
            cs.SetTexture(2, "_source", biomesTempTex);
            cs.SetTexture(2, "_dest", properties.albedo);
            cs.SetBuffer(2, "_minPredicates", BminP);
            cs.SetBuffer(2, "_maxPredicates", BmaxP);
            cs.SetBuffer(2, "_blendingFactor", blendingFactorBuff);    

        


            cs.Dispatch(2, 256/8, 256/8, 1);

            biomesTempTex.Release();
            



            vBuff.GetData(mesh.vertices);
            nBuff.GetData(mesh.normals);

            mesh.vertexColor = CBuffHelper.ExtractColBuff(cBuff);

            




            vBuff.Release();
            nBuff.Release();
            cBuff.Release();

            BminP.Release();
            BmaxP.Release();
            blendingFactorBuff.Release();
            biomeScaleBuff.Release();
            biomesMulBuff.Release();
            biomesNLBuff.Release();
            biomesOffsetBuff.Release();
            biomesPersistenceBuff.Release();
            biomesLacunarityBuff.Release();
            biomesVShiftBuff.Release();
            biomesDebugColBuff.Release();
        }


        public void Build(MeshFilter meshFilter, MeshRenderer meshRenderer)
        {
#if UNITY_EDITOR
            if (meshFilter.sharedMesh == null) return;
            Mesh mesh = meshFilter.sharedMesh;
#else
            if (meshFilter.mesh == null) return;
            Mesh mesh = meshFilter.mesh;
#endif
            mesh.Clear();

            List<Tuple<CombineInstance, ChunkTextures>> chunkData = new();
            foreach (var face in chunks)
            {
                face.CollectCombineData(chunkData);
            }
            CombineInstance[] combines = new CombineInstance[chunkData.Count];
            surfmats = new Material[chunkData.Count];
            for (int i = 0; i < chunkData.Count; i++) surfmats[i] = settings.biomes.surfaceMaterial;
            meshRenderer.sharedMaterials = surfmats;
            for (int i = 0; i < chunkData.Count; i++)
            {
                //Mesh
                combines[i] = chunkData[i].Item1;
                //Mat
                MaterialPropertyBlock mpb = new();
                mpb.SetTexture("_BaseMap", chunkData[i].Item2.albedo);
                meshRenderer.SetPropertyBlock(mpb, i);
            }
            mesh.CombineMeshes(combines.ToArray(), false, false, false);
        }


        public TeluricGenerator(PlanetSettings settings)
        {
            queue = new();
            this.settings = settings;
            chunks = new Chunk[]{
                new(0, settings.radius * 2f, 0, new Vector3(0, settings.radius, 0), settings),
                new(1, settings.radius * 2f, 0, new Vector3(0, -settings.radius, 0), settings),
                new(2, settings.radius * 2f, 0, new Vector3(0, 0, settings.radius), settings),
                new(3, settings.radius * 2f, 0, new Vector3(0, 0, -settings.radius), settings),
                new(4, settings.radius * 2f, 0, new Vector3(settings.radius, 0, 0), settings),
                new(5, settings.radius * 2f, 0, new Vector3(-settings.radius, 0, 0), settings)
            };
        }

        /// <summary>
        /// Destroy all chunks and free their resources.
        /// (Set the quadTree on fire).
        /// </summary>
        public void Clear()
        {
            queue = null;
            chunks?.ToList().ForEach(c => c.Kill());
            chunks?.ToList().ForEach(c => c = null);
            chunks = null;
        }


        /// <summary>
        /// Only use In edit mode.<br/>
        /// Meshs Rebuilt at each frame.
        /// </summary>
        /// <param name="position of the transform of planet"></param>
        public void Regen(Vector3 planetPosition, MeshFilter meshFilter, MeshRenderer meshRenderer)
        {
            if (Camera.current == null || chunks == null || queue == null) return;
            // check if we need a split or not.
            chunks.ToList().ForEach(c => c.Update(Camera.current.transform.position - planetPosition, queue));
            // execute a change in quadTree
            if (queue.Count > 0) queue.Dequeue().Execute();
            chunks.ToList().ForEach(c => c.Regen());
            Build(meshFilter, meshRenderer);
        }

        /// <summary>
        /// <typeparamref name="Chunk"/> class.
        /// <br/>
        ///  - visually it's a tile on the surface of the planet.<br/>
        ///  - node of the quadtree. 
        /// 
        /// if it's lod is equal to mLOD<br/>
        ///     interpreted as a leaf(no childrens).<br/>
        ///  else<br/>
        ///     no visual or logic, it relay update to it's childrens.<bt/>
        /// A chunk have it's LOD determined by the proximity with the current camera(allow us to have lod updated even in edit mode.).
        /// </summary>
        public partial class Chunk
        {
            private PlanetSettings settings;
            private ChunkTextures textures;


            /// <summary>
            /// Chunk registered a task in queue? <br/> If <c> true </c> the chunk will not be updated, nor will be it's childrens.
            /// </summary>
            private bool pending = false;

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
            public Chunk(int Dir, float Size, int LOD, Vector3 center, PlanetSettings settings)
            {
                this.Dir = Dir;
                this.Size = Size;
                this.LOD = LOD;
                this.center = center;
                this.settings = settings;
                textures = new();
                Regen();
            }

            /// <summary>
            /// Destroy this chunk(and it's childrens) and free his resources
            /// </summary>
            public void Kill()
            {
                textures.Clear();
                cachedMesh.Clear();
                combine.mesh.Clear();
                childrens?.ToList().ForEach(c => c.Kill());
                childrens?.ToList().ForEach(c => c = null);
                textures = null;
                childrens = null;
#if UNITY_EDITOR
                MonoBehaviour.DestroyImmediate(cachedMesh);
                MonoBehaviour.DestroyImmediate(combine.mesh);
#else
                
                MonoBehaviour.Destroy(cachedMesh);
                MonoBehaviour.Destroy(combine);
#endif
            }
            

            /// <summary>
            /// Test if the chunk need to be splitted or not.
            /// </summary>
            /// <param name="position"> player position in <c>Object space</c>, relative to the Planet's transform.</param>
            internal void Update(Vector3 position, Queue<ChunkTask> queue)
            {
                if (pending) return;
                // Not splitted, not maxSplit lvl and player in split Radius
                if (playerInBound(position) && childrens == null && LOD <= settings.biomes.MaxLOD)
                {
                    ChunkTask newTask = new (ChunkTaskTYPE.Split, this);
                    pending = true;
                    queue.Enqueue(newTask);
                }
                // Splitted
                else if (childrens != null)
                {
                    // if player not clause enough to justify split.
                    if (!playerInBound(position))
                    {
                        ChunkTask newTask = new (ChunkTaskTYPE.UnSplit, this);
                        pending = true;
                        queue.Enqueue(newTask);
                    }
                    else // Propagate test to childrens
                    {
                        childrens.ToList().ForEach(c => c.Update(position, queue));
                    }
                }
            }

            

            /// <summary>
            ///    Evaluate if the player is in split bound.
            /// </summary>
            /// <param name="position">Player position in <c> Object Space </c>, relative to the Planet's transform.</param>
            /// <returns></returns>
            protected bool playerInBound(Vector3 position)
            {
                return (position - geoCenter).sqrMagnitude <= Mathf.Pow(2 * Size, 2);
            }

            /// <summary>
            /// Run all the proccedural generators of the chunk.
            /// </summary>
            internal void Regen()
            {
                QuadMesh q = SubDivide(SubDivide(SubDivide(SubDivide(GenInitMesh(Dir, center, Size)))));
                GenerateTopo(textures, q, settings);
                cachedMesh = ToMesh(q);
            }

            
        }


    }
}
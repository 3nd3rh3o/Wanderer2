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
        private PlanetSettings settings;
        private ComputeShader geoCS;
        private Chunk[] chunks;
        private Material[] surfmats;

        /// <summary>
        /// Compute shader caller, will deform mesh and generate textures
        /// </summary>
        /// <param name="properties"> Texture holder </param>
        /// <param name="mesh"> Model </param>
        /// <param name="settings"> Global settings of the planet </param>
        public static void Generate(ChunkTextures properties, QuadMesh mesh, PlanetSettings settings, ComputeShader cs)
        {
            
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


        public void Build(MeshFilter meshFilter, Material m, MeshRenderer meshRenderer)
        {
#if UNITY_EDITOR
            if (meshFilter.sharedMesh == null) meshFilter.sharedMesh = new();
            Mesh mesh = meshFilter.sharedMesh;
#else
            meshFilter.mesh == null) meshFilter.mesh = new();
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
            for (int i = 0; i < chunkData.Count; i++) surfmats[i] = m;
            meshRenderer.sharedMaterials = surfmats;
            for (int i = 0; i < chunkData.Count; i++)
            {
                //Mesh
                combines[i] = chunkData[i].Item1;
                //Mat
                MaterialPropertyBlock mpb = new();
                //mpb.SetTexture("_BaseMap", chunkData[i].Item2.albedo);
                meshRenderer.SetPropertyBlock(mpb, i);
            }
            mesh.CombineMeshes(combines.ToArray(), false, false, false);
        }


        public TeluricGenerator(PlanetSettings settings, ComputeShader cs)
        {
            this.geoCS = cs;
            this.settings = settings;
            chunks = new Chunk[]{
                new(0, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, settings.radius, 0), settings, cs),
                new(1, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, -settings.radius, 0), settings, cs),
                new(2, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, 0, settings.radius), settings, cs),
                new(3, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, 0, -settings.radius), settings, cs),
                new(4, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(settings.radius, 0, 0), settings, cs),
                new(5, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(-settings.radius, 0, 0), settings, cs)
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
                new(0, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, settings.radius, 0), settings, geoCS),
                new(1, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, -settings.radius, 0), settings, geoCS),
                new(2, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, 0, settings.radius), settings, geoCS),
                new(3, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(0, 0, -settings.radius), settings, geoCS),
                new(4, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(settings.radius, 0, 0), settings, geoCS),
                new(5, settings.radius * 2f, 0, settings.biomes.MaxLOD, new Vector3(-settings.radius, 0, 0), settings, geoCS)
            };
        }


        public partial class Chunk
        {
            private ComputeShader cs;
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
            public Chunk(int Dir, float Size, int LOD, int mLOD, Vector3 center, PlanetSettings settings, ComputeShader cs)
            {
                this.Dir = Dir;
                this.Size = Size;
                this.LOD = LOD;
                this.mLOD = mLOD;
                this.center = center;
                this.settings = settings;
                textures = new();
                this.cs = cs;
                QuadMesh q = SubDivide(SubDivide(SubDivide(SubDivide(GenInitMesh(Dir, center, Size)))));
                Generate(textures, q, settings, cs);
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
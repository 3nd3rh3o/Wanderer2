using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
namespace Wanderer
{
    
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class PlanetEditor : MonoBehaviour
    {
        private TerrainAtlas atlas;
        public PlanetSettings settings;
        public ComputeShader Generator;
        private Queue<ChunkTask> queue = new();
        private ChunkNHMapCSManager csMan;
        public Material atmosphereMat;
        public Material terrainMaterial;
        [SerializeField]
        public MultiMaterialFullScreenPassRendererFeature atmosphereRFClose;
        [SerializeField]
        public MultiMaterialFullScreenPassRendererFeature atmosphereRFFar;
        private Chunk[] chunks;
        private CombineInstance[] combines;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void OnEnable()
        {
            if (Generator) csMan = new(Generator);
            chunks = new Chunk[]{
                new(new Vector3(0, settings.radius, 0), settings.radius * 2, 0, settings.biomes.MaxLOD, settings.radius, csMan, settings.biomes.Scale, settings.biomes.Multiplier, settings.biomes.Offset, settings.biomes, atlas.atlas, 0),
                new(new Vector3(0, -settings.radius, 0), settings.radius * 2, 1, settings.biomes.MaxLOD, settings.radius, csMan, settings.biomes.Scale, settings.biomes.Multiplier, settings.biomes.Offset, settings.biomes, atlas.atlas, 0),
                new(new Vector3(0, 0, settings.radius), settings.radius * 2, 2, settings.biomes.MaxLOD, settings.radius, csMan, settings.biomes.Scale, settings.biomes.Multiplier, settings.biomes.Offset, settings.biomes, atlas.atlas, 0),
                new(new Vector3(0, 0, -settings.radius), settings.radius * 2, 3, settings.biomes.MaxLOD, settings.radius, csMan, settings.biomes.Scale, settings.biomes.Multiplier, settings.biomes.Offset, settings.biomes, atlas.atlas, 0),
                new(new Vector3(settings.radius, 0, 0), settings.radius * 2, 4, settings.biomes.MaxLOD, settings.radius, csMan, settings.biomes.Scale, settings.biomes.Multiplier, settings.biomes.Offset, settings.biomes, atlas.atlas, 0),
                new(new Vector3(-settings.radius, 0, 0), settings.radius * 2, 5, settings.biomes.MaxLOD, settings.radius, csMan, settings.biomes.Scale, settings.biomes.Multiplier, settings.biomes.Offset, settings.biomes, atlas.atlas, 0)
            };
            Build();
        }

        void Update()
        {
            if (chunks != null && queue.Count == 0)
                chunks.ToList().ForEach(c => c.Update(Quaternion.Inverse(transform.rotation) * transform.position * -1f, queue));
            else
            {
                ChunkTask t = queue.Dequeue();
                t.chunk.ConsumeChunkTask(t);
            }
            Build();
            if (settings.atmosphereSettings != null && atmosphereMat != null)
            {
                atmosphereMat.SetVector("_PlanetPosition", this.transform.position);
                atmosphereMat.SetVector("_LightDirection", Vector3.forward);
                Vector3 scattCoefs = new Vector3(Mathf.Pow(1 / settings.atmosphereSettings.ScatteringCoefficients.x, 4), Mathf.Pow(1 / settings.atmosphereSettings.ScatteringCoefficients.y, 4), Mathf.Pow(1 / settings.atmosphereSettings.ScatteringCoefficients.z, 4)) * settings.atmosphereSettings.ScatteringStrength;

                atmosphereMat.SetVector("_ScatteringCoefficients", scattCoefs);
                atmosphereMat.SetFloat("_PlanetRadius", settings.atmosphereSettings.AtmosphereOffset);
                atmosphereMat.SetFloat("_AtmosphereRadius", (1 + settings.atmosphereSettings.AtmosphereSize * .1f) * settings.atmosphereSettings.AtmosphereOffset);
                atmosphereMat.SetFloat("_DensityFallOff", settings.atmosphereSettings.DensityFalloff);
            }
        }

        void OnDisable()
        {
            chunks?.ToList().ForEach(c => c.Kill());
            chunks = null;

            combines.ToList().ForEach(c => DestroyImmediate(c.mesh));
            DestroyImmediate(GetComponent<MeshFilter>().mesh);

            GetComponent<MeshFilter>().mesh = null;
            GetComponent<MeshRenderer>().SetMaterials(new());
            Resources.UnloadUnusedAssets();
        }

        void OnValidate()
        {
            OnDisable();
            OnEnable();
        }



        private void Build()
        {
            List<Tuple<CombineInstance, RenderTexture, RenderTexture, RenderTexture, RenderTexture, RenderTexture, RenderTexture>> chunkData = new();

            // Parcourt les faces principales (chunks racines)

            foreach (var face in chunks)
            {
                face.CollectCombineData(chunkData);
            }
            if (combines == null) combines = new CombineInstance[chunkData.Count];
            if (combines.Length != chunkData.Count) combines = new CombineInstance[chunkData.Count];
            RenderTexture[] albedosTextures = new RenderTexture[chunkData.Count];
            RenderTexture[] normalsTextures = new RenderTexture[chunkData.Count];
            RenderTexture[] heightsTextures = new RenderTexture[chunkData.Count];
            RenderTexture[] metalicsTextures = new RenderTexture[chunkData.Count];
            RenderTexture[] rougnesssTextures = new RenderTexture[chunkData.Count];
            RenderTexture[] occlusionsTextures = new RenderTexture[chunkData.Count];
            for (int i = 0; i < chunkData.Count; i++)
            {
                (combines[i], albedosTextures[i], normalsTextures[i], heightsTextures[i], metalicsTextures[i], rougnesssTextures[i], occlusionsTextures[i]) = chunkData[i];
            }
            // Crée un mesh global combiné
            Mesh combinedMesh = GetComponent<MeshFilter>().mesh;
            combinedMesh.Clear();
            combinedMesh.CombineMeshes(combines, false, true);

            // ajuste le renderer
            Material[] m = new Material[combines.Length];
            for (int i = 0; i < combines.Length; i++) m[i] = terrainMaterial;
            // Applique le mesh combiné au MeshFilter principal
            GetComponent<MeshRenderer>().SetMaterials(m.ToList());
            for (int i = 0; i < combines.Length; i++)
            {
                MaterialPropertyBlock mpb = new();
                mpb.SetTexture("_BaseMap", albedosTextures[i]);
                mpb.SetTexture("_BumpMap", normalsTextures[i]);
                mpb.SetVector("_LightDirection", Vector3.for) ;
                mpb.SetVector("_LightColor", new Vector3(1, 1, 1));
                //TODO fix me?
                //mpb.SetTexture("_ParallaxMap", heightsTextures[i]);
                GetComponent<MeshRenderer>().SetPropertyBlock(mpb, i);
            }
            GetComponent<MeshFilter>().mesh = combinedMesh;
        }
    }
}
#endif
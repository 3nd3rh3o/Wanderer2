using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TelluricMajorCelestialBody : IMajorCellestialBody
{
    private TerrainAtlas atlas;
    private Material terrainMaterial;
    private float atmosphereRadius;
    private float planetAtmRadius;
    private float BiomeScale;
    private float BiomeMul;
    private Vector3 BiomeOffset;
    private Biome[] biomes;
    private ComputeShader geometryGen;
    private int mLOD;
    private Queue<ChunkTask> queue = new();
    private ChunkNHMapCSManager csMan;
    private bool hasAtmosphere;
    public AtmoData atmoData;
    private Material atmosphereMat;
    private Chunk[] chunks;
    private CombineInstance[] combines;
    private Transform ParentTransform;

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
            mpb.SetVector("_LightDirection", transform.parent.rotation * (transform.localPosition).normalized);
            mpb.SetVector("_LightColor", new Vector3(1, 1, 1));
            //TODO fix me?
            //mpb.SetTexture("_ParallaxMap", heightsTextures[i]);
            GetComponent<MeshRenderer>().SetPropertyBlock(mpb, i);
        }
        GetComponent<MeshFilter>().mesh = combinedMesh;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (chunks != null && queue.Count == 0)
            chunks.ToList().ForEach(c => c.Update(Quaternion.Inverse(transform.rotation) * transform.position * -1f, queue));
        else
        {
            ChunkTask t = queue.Dequeue();
            t.chunk.ConsumeChunkTask(t);
        }
        Build();
        if (atmoData != null)
        {
            atmoData._lightDirection = (ParentTransform.position-transform.position).normalized;
            atmoData._PlanetPosition = transform.position;
        }
        if (atmosphereMat != null)
        {
            atmosphereMat.SetVector("_PlanetPosition", this.transform.position);
            atmosphereMat.SetVector("_LightDirection", atmoData._lightDirection);
            Vector3 scattCoefs = new Vector3(Mathf.Pow(1/atmoData._ScatteringCoefficients.x, 4), Mathf.Pow(1/atmoData._ScatteringCoefficients.y, 4), Mathf.Pow(1/atmoData._ScatteringCoefficients.z, 4)) * atmoData._ScatteringStrenght;
            
            atmosphereMat.SetVector("_ScatteringCoefficients", scattCoefs);
            atmosphereMat.SetFloat("_PlanetRadius", atmoData._PlanetRadius);
            atmosphereMat.SetFloat("_AtmosphereRadius", (1 + atmoData._AtmosphereRadius * .1f) * atmoData._PlanetRadius);
            atmosphereMat.SetFloat("_DensityFallOff", atmoData._DensityFalloff);
        }
        
    }

    public override bool HasAtmo()
    {
        return hasAtmosphere;
    }

    public override Material GetAtmoMat()
    {
        return atmosphereMat;
    }

    public override void OnEnable()
    {

        base.OnEnable();
        if (geometryGen) csMan = new(geometryGen);
/*         chunks = new Chunk[]{
            new(new Vector3(0, radius, 0), radius * 2, 0, mLOD, radius, csMan, BiomeScale, BiomeMul, BiomeOffset, biomes, atlas.atlas, 0),
            new(new Vector3(0, -radius, 0), radius * 2, 1, mLOD, radius, csMan, BiomeScale, BiomeMul, BiomeOffset, biomes, atlas.atlas, 0),
            new(new Vector3(0, 0, radius), radius * 2, 2, mLOD, radius, csMan, BiomeScale, BiomeMul, BiomeOffset, biomes, atlas.atlas, 0),
            new(new Vector3(0, 0, -radius), radius * 2, 3, mLOD, radius, csMan, BiomeScale, BiomeMul, BiomeOffset, biomes, atlas.atlas, 0),
            new(new Vector3(radius, 0, 0), radius * 2, 4, mLOD, radius, csMan, BiomeScale, BiomeMul, BiomeOffset, biomes, atlas.atlas, 0),
            new(new Vector3(-radius, 0, 0), radius * 2, 5, mLOD, radius, csMan, BiomeScale, BiomeMul, BiomeOffset, biomes, atlas.atlas, 0)
        }; */
        Build();
    }

    public override void OnDisable()
    {
        chunks?.ToList().ForEach(c => c.Kill());
        chunks = null;
#if UNITY_EDITOR
        combines.ToList().ForEach(c => DestroyImmediate(c.mesh));
        DestroyImmediate(GetComponent<MeshFilter>().mesh);
#else
        combines.ToList().Foreach(c => Destroy(c.mesh));
        Destroy(GetComponent<MeshFilter>().mesh);
#endif
        GetComponent<MeshFilter>().mesh = null;
        GetComponent<MeshRenderer>().SetMaterials(new());
        Resources.UnloadUnusedAssets();
    }
    public override void Kill()
    {
        base.Kill();
        this.OnDisable();
        Destroy(gameObject);
    }

    public void Init(
        float radius, float mass,
        Vector3 initialVelocity, Vector3 initialPosition,
        Vector3 initialTorque, Vector3 initialOrientation,
        bool IsKynematic,
        TerrainAtlas atlas, Material terrainMaterial,
        AtmoData atmoData, Material atmoMat,
        float BiomeScale, float BiomeMul,
        Vector3 BiomeOffset, Biome[] biomes,
        ComputeShader geometryGen, int mLOD,
        Transform parentTransform
    )
    {
        base.Init(radius, mass, initialVelocity, initialPosition, initialTorque, initialOrientation, IsKynematic);
        this.atlas = atlas;
        this.terrainMaterial = terrainMaterial;
        if (atmoData == null)
        {
            hasAtmosphere = false;
        }
        else
        {
            hasAtmosphere = true;
            atmosphereMat = new Material(atmoMat);
            this.atmoData = atmoData;
            
        }
        this.BiomeScale = BiomeScale;
        this.BiomeMul = BiomeMul;
        this.BiomeOffset = BiomeOffset;
        this.biomes = biomes;
        this.geometryGen = geometryGen;
        this.mLOD = mLOD;
        this.ParentTransform = parentTransform;
        gameObject.SetActive(true);
    }
}
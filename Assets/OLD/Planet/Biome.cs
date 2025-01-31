using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class BiomeTextureData
{
    [Range(0.001f, 2f)] [Tooltip("Scale of the noise texture")] public float TextureScale;
    [Tooltip("offset of the noise texture")] public Vector3 offset;
    [Tooltip("Biome primary color")] public Color PrimaryColor;
    [Tooltip("Biome secondary color")] public Color SecondaryColor;
    [Tooltip("Scale of the height of the texture")] [Range(0f, 1f)] public float heightScale;
    [Range(0f, 1f)] [Tooltip("Multiplier of the height of the texture")] public float heightMul;
    [Tooltip("Detail pool to fetch")] public int surfaceDetailMaterial;
    [Tooltip("Chunk debug color(written in vertex color channel)")] public Color DebugColor;
}

[Serializable]
public class BiomeIntegrator
{
    [Tooltip("min gradients values to spawn")] public BiomePredicate PredicateMin;
    [Tooltip("max gradients values to spawn")] public BiomePredicate PredicateMax;
    [Tooltip("Tolerance, or how much this predicate can 'overshoot'")] [Range(0f, 1f)] public float blendingFactor = 0f;
}

[Serializable]
public class Biome
{
    [Tooltip("Name of the biome")] public string name;
    [SerializeField] public BiomeIntegrator IntegrationParams = new();

    [FormerlySerializedAs("Carver")]
    [SerializeField]
    private BiomeCarver carver;
    [Tooltip("How do we build the topology of this biome")]
    public BiomeCarver Carver
    {
        get
        {
            return carver;
        }
        set
        {
            carver = value;
        }
    }
    [Tooltip("arguments given to the driver")]
    public float[] carverArguments = new float[0];
    
    [SerializeField] public BiomeTextureData SurfaceTexture = new();

    internal float4 GetMinPreds()
    {
        return new float4(IntegrationParams.PredicateMin.altitude, IntegrationParams.PredicateMin.temperature, IntegrationParams.PredicateMin.humidity, IntegrationParams.PredicateMin.latitude);
    }
    internal float4 GetMaxPreds()
    {
        return new float4(IntegrationParams.PredicateMax.altitude, IntegrationParams.PredicateMax.temperature, IntegrationParams.PredicateMax.humidity, IntegrationParams.PredicateMax.latitude);
    }

    internal float3 GetColor()
    {
        return new float3(SurfaceTexture.DebugColor.r, SurfaceTexture.DebugColor.g, SurfaceTexture.DebugColor.b);
    }

    internal int GetTexIds()
    {
        return SurfaceTexture.surfaceDetailMaterial;
    }

    internal int GetGenToUse()
    {
        return carver switch
        {
            BiomeCarver.NONE => 0,
            BiomeCarver.FRAC_SIMPLEX => 1,
            _ => 0
        };
    }

    internal float4x4 GetGenParams()
    {
        float4x4 res = new();
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                if (4 * x + y == carverArguments.Length) return res;
                res[x][y] = carverArguments[4 * x + y];
            }
        }
        return res;
    }

    internal float3 GetMainCol()
    {
        return new(SurfaceTexture.PrimaryColor.r, SurfaceTexture.PrimaryColor.g, SurfaceTexture.PrimaryColor.b);
    }

    internal float3 GetSecCol()
    {
        return new(SurfaceTexture.SecondaryColor.r, SurfaceTexture.SecondaryColor.g, SurfaceTexture.SecondaryColor.b);
    }

    internal float GetTexScale()
    {
        return SurfaceTexture.TextureScale;
    }

    internal float GetTolerance()
    {
        return IntegrationParams.blendingFactor;
    }

    internal float3 GetBiomeTexOffset()
    {
        return new float3(SurfaceTexture.offset.x, SurfaceTexture.offset.y, SurfaceTexture.offset.z);
    }
}

[Serializable]
public class BiomePredicate
{
    [Range(-1f, 1f)]
    public float altitude;
    [Range(-1f, 1f)]
    public float temperature;
    [Range(-1f, 1f)]
    public float humidity;
    [Range(-1f, 1f)]
    public float latitude;

    public float4 ToFloat4()
    {
        return new float4(altitude, temperature, humidity, latitude);
    }
}

public enum BiomeCarver
{
    NONE,
    FRAC_SIMPLEX
}

public class BiomeMaterial
{

}
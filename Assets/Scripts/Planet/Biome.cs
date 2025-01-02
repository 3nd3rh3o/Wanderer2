using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
[Serializable]
public class Biome
{
    [Tooltip("for conveniance")]
    public string name;
    [Tooltip("min gradients values to spawn")]
    public BiomePredicate PredicateMin;
    [Tooltip("max gradients values to spawn")]
    public BiomePredicate PredicateMax;
    
    [Tooltip("What material to use, and when we use them in this biome(normals, color....)")]
    public BiomeMaterial[] biomeMaterials;
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
    [Range(0f, 1f)]
    public float blendingFactor = 0f;
    [Range(0f, 1f)]
    [Tooltip("Scale of the noise texture")]
    public float TextureScale;
    [Tooltip("Biome primary color")]
    public Color PrimaryColor;
    [Tooltip("Biome secondary color")]
    public Color SecondaryColor;
    [Tooltip("Surface texture to use")]
    public int surfaceMaterial;

    [Tooltip("Chunk debug color(written in vertex color channel)")]
    public Color DebugColor;
    

    internal float4 GetMinPreds()
    {
        return new float4(PredicateMin.altitude, PredicateMin.temperature, PredicateMin.humidity, PredicateMin.latitude);
    }
    internal float4 GetMaxPreds()
    {
        return new float4(PredicateMax.altitude, PredicateMax.temperature, PredicateMax.humidity, PredicateMax.latitude);
    }

    internal float3 GetColor()
    {
        return new float3(DebugColor.r, DebugColor.g, DebugColor.b);
    }

    internal int GetTexIds()
    {
        return surfaceMaterial;
    }

    internal int GetGenToUse()
    {
        return carver switch {
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
                if (4*x+y == carverArguments.Length) return res;
                res[x][y] = carverArguments[4*x+y];
            }
        }
        return res;
    }

    internal float3 GetMainCol()
    {
        return new(PrimaryColor.r, PrimaryColor.g, PrimaryColor.b);
    }

    internal float3 GetSecCol()
    {
        return new(SecondaryColor.r, SecondaryColor.g, SecondaryColor.b);
    }

    internal float GetTexScale()
    {
        return TextureScale;
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
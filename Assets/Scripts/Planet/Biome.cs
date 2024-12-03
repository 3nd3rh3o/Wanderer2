using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
[Serializable]
public class Biome
{
    [Tooltip("for conveniance")]
    public string name;
    [Tooltip("When do we are in the biome (each point on the sphere as 4 value, if they are higher than those of the predicate, => in this biome)")]
    public BiomePredicate predicate;
    
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

    

    internal float4 GetPreds()
    {
        return new float4(predicate.altitude, predicate.temperature, predicate.humidity, predicate.latitude);
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
using System;
using UnityEngine;

public class Biome
{
    public float altitude;
    public float temperature;
    public float humity;
    public float latitude;

    public BiomeMaterial[] biomeMaterials;

    public BiomeCarver carver
    {
        get
        {
            return carver;
        }
        set
        {
            carverArguments = value switch
            {
                BiomeCarver.NONE => new float[0],
                BiomeCarver.FRAC_SIMPLEX => new float[8]

            };
            carver = value;
        }
    }
    public float[] carverArguments;
}

public enum BiomeCarver
{
    NONE,
    FRAC_SIMPLEX,

}

public class BiomeMaterial
{
    
}
using System;
using UnityEngine;
[Serializable]
public class Biome
{
    [Tooltip("for conveniance")]
    public string name;
    [Tooltip("all the rules to match to be in this biome")]
    public BiomePredicate[] predicates;
    
    [Tooltip("What material to use, and when we use them in this biome(normals, color....)")]
    public BiomeMaterial[] biomeMaterials;
    [Tooltip("How do we build the topology of this biome")]
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
                BiomeCarver.FRAC_SIMPLEX => new float[8],
                _ => new float[0]
            };
            carver = value;
        }
    }
    [Tooltip("arguments given to the driver")]
    public float[] carverArguments;
}

[Serializable]
public class BiomePredicate
{
    //TODO add default => U(the 4 params, with their names)

    public enum InputValue
    {
        alitude,
        latitude,
        temperature,
        humidity
    }
    [Tooltip("Use or statement instead of AND on the PREVIOUS predicate(first predicate don't use this value)")]
    public bool OR;
    // more or equal than dom=> return true;
    [Tooltip("threshold")]
    [Range(-1f, 1f)]
    public float value;
    [Tooltip("What canal to read on the 4d noise map of the sphere")]
    public InputValue input;
    [Tooltip("If true, 'input>value' will be used instead of 'input<=value'")]
    public bool invert = false;
}

public enum BiomeCarver
{
    NONE,
    FRAC_SIMPLEX,

}

public class BiomeMaterial
{

}
using System;
using UnityEngine;

public class Biome
{
    public BiomePredicate predicate;

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

[Serializable]
public class BiomePredicate
{
    public PredicateOperation predicate;
}


[Serializable]
public class PredicateOperation
{
    public string name;
    [Range(-1, 1)]
    public float value;
    public PredicateOperation predicate;
    /*
    TODO bool op, to allow other ranges than everything above this value.
    one for each predicate. IT DOESN'T CHANGE APPLICATION ORDER.
    ex : 
        - pole biome => 1 biome.
        - poles different => 2 biomes.


    default => ALL(predicate_n(value_n))

    */
}

public enum BiomeCarver
{
    NONE,
    FRAC_SIMPLEX,

}

public class BiomeMaterial
{
    
}
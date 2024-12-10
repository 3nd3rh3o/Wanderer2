using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class TerrainAtlas
{

    [SerializeField]
    public TerrainType[] types;
    public Texture3D[] atlas;


    public void Cleanup()
    {
        if (atlas != null)
        {
            #if UNITY_EDITOR
            atlas.ToList().ForEach(t => UnityEngine.Object.DestroyImmediate(t));
            #else
            atlas.ToList().ForEach(t => UnityEngine.Object.Destroy(t));
            #endif
            atlas = null;
        }
    }
    public void Init()
    {
        //TODO mouve it in CS later
        if (atlas != null) Cleanup();
        int texSize = 256;
        //PUT THIS IN A LOOP, PER-LOD
        atlas = new Texture3D[6];
        Texture3D albedos = new Texture3D(texSize, texSize, types.Length, TextureFormat.RGBA32, false);
        Texture3D normalMaps = new Texture3D(texSize, texSize, types.Length, TextureFormat.RGBA32, false);
        Texture3D heights = new Texture3D(texSize, texSize, types.Length, TextureFormat.RGBA32, false);
        Texture3D metalics = new Texture3D(texSize, texSize, types.Length, TextureFormat.RGBA32, false);
        Texture3D roughnesss = new Texture3D(texSize, texSize, types.Length, TextureFormat.RGBA32, false);
        Texture3D occlusions = new Texture3D(texSize, texSize, types.Length, TextureFormat.RGBA32, false);

        for (int z = 0; z < types.Length; z++)
        {
            for (int y = 0; y < texSize; y++)
            {
                for (int x = 0; x < texSize; x++)
                {
                    albedos.SetPixel(x, y, z, types[z].albedo.GetPixel(x, y));
                    normalMaps.SetPixel(x, y, z, types[z].normalMap.GetPixel(x, y));
                    heights.SetPixel(x, y, z, types[z].height.GetPixel(x, y));
                    metalics.SetPixel(x, y, z, types[z].metalic.GetPixel(x, y));
                    roughnesss.SetPixel(x, y, z, types[z].roughness.GetPixel(x, y));
                    occlusions.SetPixel(x, y, z, types[z].occlusion.GetPixel(x, y));
                }
            }
        }
        
        atlas[0] = albedos;
        atlas[1] = normalMaps;
        atlas[2] = heights;
        atlas[3] = metalics;
        atlas[4] = roughnesss;
        atlas[5] = occlusions;

        
        albedos.Apply();
        normalMaps.Apply();
        heights.Apply();
        metalics.Apply();
        roughnesss.Apply();
        occlusions.Apply();
    }
}

[Serializable]
public class TerrainType
{
    public Texture2D albedo;
    public Texture2D normalMap;
    public Texture2D height;
    public Texture2D metalic;
    public Texture2D roughness;
    public Texture2D occlusion;
}
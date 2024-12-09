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
        atlas = new Texture3D[6];
        Texture3D albedos = new Texture3D(256, 256, types.Length, TextureFormat.RGBA32, false);
        for (int z = 0; z < types.Length; z++)
        {
            for (int y = 0; y < types[z].albedo.height; y++)
            {
                for (int x = 0; x < types[z].albedo.width; x++)
                {
                    albedos.SetPixel(x, y, z, types[z].albedo.GetPixel(x, y));
                }
            }
        }
        albedos.Apply();
        atlas[0] = albedos;
        Texture3D normalMaps = new Texture3D(256, 256, types.Length, TextureFormat.RGBA32, false);
        for (int z = 0; z < types.Length; z++)
        {
            for (int y = 0; y < types[z].normalMap.height; y++)
            {
                for (int x = 0; x < types[z].normalMap.width; x++)
                {
                    normalMaps.SetPixel(x, y, z, types[z].normalMap.GetPixel(x, y));
                }
            }
        }
        normalMaps.Apply();
        atlas[1] = normalMaps;
        Texture3D heights = new Texture3D(256, 256, types.Length, TextureFormat.RGBA32, false);
        for (int z = 0; z < types.Length; z++)
        {
            for (int y = 0; y < types[z].height.height; y++)
            {
                for (int x = 0; x < types[z].height.width; x++)
                {
                    heights.SetPixel(x, y, z, types[z].height.GetPixel(x, y));
                }
            }
        }
        heights.Apply();
        atlas[2] = heights;
        Texture3D metalics = new Texture3D(256, 256, types.Length, TextureFormat.RGBA32, false);
        for (int z = 0; z < types.Length; z++)
        {
            for (int y = 0; y < types[z].metalic.height; y++)
            {
                for (int x = 0; x < types[z].metalic.width; x++)
                {
                    metalics.SetPixel(x, y, z, types[z].metalic.GetPixel(x, y));
                }
            }
        }
        metalics.Apply();
        atlas[3] = metalics;
        Texture3D roughnesss = new Texture3D(256, 256, types.Length, TextureFormat.RGBA32, false);
        for (int z = 0; z < types.Length; z++)
        {
            for (int y = 0; y < types[z].roughness.height; y++)
            {
                for (int x = 0; x < types[z].roughness.width; x++)
                {
                    roughnesss.SetPixel(x, y, z, types[z].roughness.GetPixel(x, y));
                }
            }
        }
        roughnesss.Apply();
        atlas[4] = roughnesss;
        Texture3D occlusions = new Texture3D(256, 256, types.Length, TextureFormat.RGBA32, false);
        for (int z = 0; z < types.Length; z++)
        {
            for (int y = 0; y < types[z].occlusion.height; y++)
            {
                for (int x = 0; x < types[z].occlusion.width; x++)
                {
                    occlusions.SetPixel(x, y, z, types[z].occlusion.GetPixel(x, y));
                }
            }
        }
        occlusions.Apply();
        atlas[5] = occlusions;
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
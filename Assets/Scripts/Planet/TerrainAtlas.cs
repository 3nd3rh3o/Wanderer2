using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class TerrainAtlas
{

    [SerializeField]
    public TerrainType[] types;
    public Texture3D[][] atlas;


    public void Cleanup()
    {
        if (atlas != null)
        {
#if UNITY_EDITOR
            atlas.ToList().ForEach(l => l?.ToList().ForEach(t => UnityEngine.Object.DestroyImmediate(t)));
#else
            atlas.ToList().ForEach(l => l.ToList().ForEach(t => UnityEngine.Object.Destroy(t)));
#endif
            atlas = null;
        }
    }
    public void Init()
    {
        //TODO ENSURE TEXSIZE AND mLOD ARE COHERENT!!! (REDUCE TEX SIZE TO 256, increase mLOD to 8)
        if (atlas != null) Cleanup();
        int texSize = 256;
        int mLod = 3;

        atlas = new Texture3D[mLod][];
        for (int l = 0; l < mLod; l++)
        {
            atlas[l] = new Texture3D[6];

            //PUT THIS IN A LOOP, PER-LOD
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
                        albedos.SetPixel(x, y, z, types[z].albedo[l].GetPixel(x, y));
                        normalMaps.SetPixel(x, y, z, types[z].normalMap[l].GetPixel(x, y));
                        heights.SetPixel(x, y, z, types[z].height[l].GetPixel(x, y));
                        metalics.SetPixel(x, y, z, types[z].metalic[l].GetPixel(x, y));
                        roughnesss.SetPixel(x, y, z, types[z].roughness[l].GetPixel(x, y));
                        occlusions.SetPixel(x, y, z, types[z].occlusion[l].GetPixel(x, y));
                    }
                }
            }

            atlas[l][0] = albedos;
            atlas[l][1] = normalMaps;
            atlas[l][2] = heights;
            atlas[l][3] = metalics;
            atlas[l][4] = roughnesss;
            atlas[l][5] = occlusions;


            albedos.Apply();
            normalMaps.Apply();
            heights.Apply();
            metalics.Apply();
            roughnesss.Apply();
            occlusions.Apply();
        }

    }
}

[Serializable]
public class TerrainType
{
    public Texture2D[] albedo;
    public Texture2D[] normalMap;
    public Texture2D[] height;
    public Texture2D[] metalic;
    public Texture2D[] roughness;
    public Texture2D[] occlusion;
}
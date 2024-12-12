using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TerrainLOD
{
    public Texture2D albedo;
    public Texture2D normalMap;
    public Texture2D height;
    public Texture2D metalic;
    public Texture2D roughness;
    public Texture2D occlusion;
}

[Serializable]
public class TerrainType
{
    public string name;
    [SerializeField]
    public TerrainLOD[] terrainLOD = new TerrainLOD[1];
}



[Serializable]
public class TerrainAtlas
{

    [SerializeField]
    public TerrainType[] types = new TerrainType[1];
    public Texture3D[][] atlas;
    public bool bake = false;




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
        int mLod = 6;

        atlas = new Texture3D[6][];
        atlas[0] = new Texture3D[mLod];
        atlas[1] = new Texture3D[mLod];
        atlas[2] = new Texture3D[mLod];
        atlas[3] = new Texture3D[mLod];
        atlas[4] = new Texture3D[mLod];
        atlas[5] = new Texture3D[mLod];
        for (int l = 0; l < mLod; l++)
        {


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
                        albedos.SetPixel(x, y, z, types[z].terrainLOD[l].albedo.GetPixel(x, y));
                        normalMaps.SetPixel(x, y, z, types[z].terrainLOD[l].normalMap.GetPixel(x, y));
                        heights.SetPixel(x, y, z, types[z].terrainLOD[l].height.GetPixel(x, y));
                        metalics.SetPixel(x, y, z, types[z].terrainLOD[l].metalic.GetPixel(x, y));
                        roughnesss.SetPixel(x, y, z, types[z].terrainLOD[l].roughness.GetPixel(x, y));
                        occlusions.SetPixel(x, y, z, types[z].terrainLOD[l].occlusion.GetPixel(x, y));
                    }
                }
            }

            atlas[0][l] = albedos;
            atlas[1][l] = normalMaps;
            atlas[2][l] = heights;
            atlas[3][l] = metalics;
            atlas[4][l] = roughnesss;
            atlas[5][l] = occlusions;


            albedos.Apply();
            normalMaps.Apply();
            heights.Apply();
            metalics.Apply();
            roughnesss.Apply();
            occlusions.Apply();
        }

    }
}



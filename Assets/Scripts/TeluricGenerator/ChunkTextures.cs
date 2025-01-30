using UnityEngine;

namespace Wanderer
{
    public partial class TeluricGenerator
    {
        public class ChunkTextures
        {
            public RenderTexture albedo;

            public ChunkTextures()
            {
                const int MAX_TEX_SIZE = 256;

                albedo = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 32, RenderTextureFormat.ARGB32);
                albedo.enableRandomWrite = true;
                albedo.Create();
            }


            public void Clear()
            {
                albedo?.Release();
                albedo = null;
            }
        }
    }
}
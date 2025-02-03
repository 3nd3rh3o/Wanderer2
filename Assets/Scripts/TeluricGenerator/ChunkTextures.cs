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

                albedo = new(MAX_TEX_SIZE, MAX_TEX_SIZE, 0, RenderTextureFormat.ARGB32);
                albedo.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
                albedo.enableRandomWrite = true;
                albedo.Create();
            }


            public void Clear()
            {
                albedo?.Release();
#if UNITY_EDITOR
                MonoBehaviour.DestroyImmediate(albedo);
#else
                MonoBehaviour.Destroy(albedo);
#endif
                albedo = null;
            }
        }
    }
}
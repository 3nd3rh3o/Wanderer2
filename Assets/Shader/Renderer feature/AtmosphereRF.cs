using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using OpenCover.Framework.Model;
using Unity.VisualScripting;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class AtmosphereRendererPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Shader shader;
    }

    public Settings settings = new();

    class AtmosphereRenderPass : ScriptableRenderPass
    {
        private Material material;
        private RenderTextureDescriptor targetDescriptor;

        public AtmosphereRenderPass(Shader shader)
        {
            if (shader != null) material = new(shader);
            targetDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height,
            RenderTextureFormat.Default, 0);
        }

        private class PassData
        {
            public Material material;
            public TextureHandle sourceColTex;
            public TextureHandle sourceDepthTex;
            public TextureHandle destColTex;
            public Camera cam;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Atmosphere Rendering(fullscreen post proccessing effect)";

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                passData.material = material;

                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                passData.sourceColTex = resourceData.activeColorTexture;
                passData.sourceDepthTex = resourceData.activeDepthTexture;
                passData.destColTex = resourceData.cameraOpaqueTexture;
                builder.SetRenderAttachment(passData.destColTex, 0, AccessFlags.Write);
                builder.UseTexture(passData.sourceColTex, AccessFlags.Read);
                builder.UseTexture(resourceData.activeDepthTexture, AccessFlags.Read);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, new(1, 1, 1, 1), data.material, 0);
                });
            }

        }
        private Matrix4x4 CamFrustum(Camera cam)
        {
            Matrix4x4 frustum = Matrix4x4.identity;
            if (cam == null) return frustum;
            float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad);


            Vector3 goUp = Vector3.up * fov;
            Vector3 goRight = Vector3.right * fov * cam.aspect;



            Vector3 TL = (-Vector3.forward - goRight + goUp);
            Vector3 TR = (-Vector3.forward + goRight + goUp);
            Vector3 BL = (-Vector3.forward - goRight - goUp);
            Vector3 BR = (-Vector3.forward + goRight - goUp);

            frustum.SetRow(0, TL);
            frustum.SetRow(1, TR);
            frustum.SetRow(2, BR);
            frustum.SetRow(3, BL);
            return frustum;
        }
    }

    AtmosphereRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new AtmosphereRenderPass(settings.shader);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}

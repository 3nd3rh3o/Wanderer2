using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using OpenCover.Framework.Model;

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

        public AtmosphereRenderPass(Shader shader)
        {
            if (shader != null) material = new(shader);
        }

        private class PassData
        {
            public Material material;
            public TextureHandle depthTexture;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Atmosphere Rendering(fullscreen post proccessing effect)";
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                passData.material = material;

                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                passData.depthTexture = resourceData.cameraDepthTexture;

                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    data.material.SetTexture("_CameraDepthTexture", data.depthTexture);

                    context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3, 1);
                });
            }
        }
    }

    AtmosphereRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new AtmosphereRenderPass(settings.shader);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}

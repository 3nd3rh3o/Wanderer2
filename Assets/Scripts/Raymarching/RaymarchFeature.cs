using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RaymarchFeature : ScriptableRendererFeature
{
    public Shader raymarchShader;
    private RaymarchPass raymarchPass;

    public override void Create()
    {
        if (raymarchShader != null)
        {
            raymarchPass = new RaymarchPass(raymarchShader);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (raymarchPass != null && raymarchShader.isSupported)
        {
            renderer.EnqueuePass(raymarchPass);
        }
    }

    class RaymarchPass : ScriptableRenderPass
    {
        private Material raymarchMaterial;
        private RTHandle currentTarget;

        public RaymarchPass(Shader shader)
        {
            raymarchMaterial = new Material(shader);
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents; // Placement dans le pipeline
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Récupérer le RTHandle pour le cameraColorTarget
            currentTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (raymarchMaterial == null)
                return;

            Camera camera = renderingData.cameraData.camera;

            // Calcul des matrices nécessaires pour le raymarching
            raymarchMaterial.SetMatrix("_CamFrustum", CalculateFrustum(camera));
            raymarchMaterial.SetMatrix("_CamToWorld", camera.cameraToWorldMatrix);

            // Effectuer le rendu du raymarching
            CommandBuffer cmd = CommandBufferPool.Get("Raymarch Atmosphere");
            cmd.Blit(null, currentTarget, raymarchMaterial);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private Matrix4x4 CalculateFrustum(Camera cam)
        {
            float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad);
            Vector3 goUp = Vector3.up * fov;
            Vector3 goRight = fov * cam.aspect * Vector3.right;

            Vector3 TL = -Vector3.forward - goRight + goUp;
            Vector3 TR = -Vector3.forward + goRight + goUp;
            Vector3 BL = -Vector3.forward - goRight - goUp;
            Vector3 BR = -Vector3.forward + goRight - goUp;

            Matrix4x4 frustum = Matrix4x4.identity;
            frustum.SetRow(0, TL);
            frustum.SetRow(1, TR);
            frustum.SetRow(2, BR);
            frustum.SetRow(3, BL);
            return frustum;
        }
    }
}
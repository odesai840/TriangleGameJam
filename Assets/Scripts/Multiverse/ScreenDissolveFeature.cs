using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenDissolveFeature : ScriptableRendererFeature
{
    public static ScreenDissolveFeature Instance { get; private set; } // static reference

    class ScreenDissolvePass : ScriptableRenderPass
    {
        public Material dissolveMaterial = null;
        public float progress = 0f;
        private RenderTargetHandle temporaryTexture;
        public string profilerTag = "Screen Dissolve Pass";

        public ScreenDissolvePass()
        {
            temporaryTexture.Init("_TemporaryScreenDissolveTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (dissolveMaterial == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            // Get the camera's color target
            RenderTargetIdentifier cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            cmd.GetTemporaryRT(temporaryTexture.id, opaqueDesc, FilterMode.Bilinear);

            dissolveMaterial.SetFloat("_Progress", progress);

            // Blit from camera target to temp, then from temp back using dissolve material.
            cmd.Blit(cameraColorTarget, temporaryTexture.Identifier());
            cmd.Blit(temporaryTexture.Identifier(), cameraColorTarget, dissolveMaterial);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                return;
            cmd.ReleaseTemporaryRT(temporaryTexture.id);
        }
    }

    public Material dissolveMaterial = null;
    [Range(0, 1)]
    public float progress = 0f;
    ScreenDissolvePass screenDissolvePass;

    public override void Create()
    {
        Instance = this; // Set static reference
        screenDissolvePass = new ScreenDissolvePass();
        screenDissolvePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        screenDissolvePass.dissolveMaterial = dissolveMaterial;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (dissolveMaterial == null)
            return;

        screenDissolvePass.progress = progress;
        renderer.EnqueuePass(screenDissolvePass);
    }
}

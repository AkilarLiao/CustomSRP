using System;
using UnityEngine;

using UnityEngine.Rendering;

[CreateAssetMenu]
public class OutLineRenderFeature : UnityEngine.Rendering.Universal.ScriptableRendererFeature
{
    //public static bool IsSkip { get; set; }
    public static bool IsSkip { get { return ms_isSkip; } set { ms_isSkip = value; } }

    public OutLineRenderFeature()
    {
    }

    public override void Create()
    {   
    }
    
    public override void AddRenderPasses(UnityEngine.Rendering.Universal.ScriptableRenderer renderer,
        ref UnityEngine.Rendering.Universal.RenderingData renderingData)
    {
        if ((IsSkip)/* || (!_enable)*/)
            return;
        _drawOutLinePass.Setup();
        renderer.EnqueuePass(_drawOutLinePass);
    }

    class DrawOutLinePass : UnityEngine.Rendering.Universal.ScriptableRenderPass
    {
        public DrawOutLinePass()
        {
            //renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            renderPassEvent = UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRenderingOpaques;
            _filteringSettings = new FilteringSettings(RenderQueueRange.opaque, ~0);            
        }

        public void Setup()
        {
            //_shaderTagId = new ShaderTagId("LightweightForward");
            //_shaderTagId = new ShaderTagId("SRPDefaultUnlit");
            _shaderTagId = new ShaderTagId("OutLine");
        }

        public override void Execute(ScriptableRenderContext context,
            ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            CommandBuffer command = CommandBufferPool.Get(_profilerTag);
            using (new ProfilingSample(command, _profilerTag))
            {
                context.ExecuteCommandBuffer(command);
                command.Clear();
                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                var drawSettings = CreateDrawingSettings(_shaderTagId,
                    ref renderingData, sortFlags);
                context.DrawRenderers(renderingData.cullResults, ref drawSettings,
                    ref _filteringSettings);
            }
            context.ExecuteCommandBuffer(command);
            CommandBufferPool.Release(command);
        }
        FilteringSettings _filteringSettings;
        const string _profilerTag = "Render Opaque outLines";
        private ShaderTagId _shaderTagId = ShaderTagId.none;
    }

    //[SerializeField]
    //private bool _enable = true;
    private static bool ms_isSkip = true;
    private DrawOutLinePass _drawOutLinePass = new DrawOutLinePass();
}

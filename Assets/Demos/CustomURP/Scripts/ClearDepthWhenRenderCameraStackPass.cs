using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomURP
{
    public class ClearDepthWhenRenderCameraStackPass : ScriptableRenderPass
    {
        public ClearDepthWhenRenderCameraStackPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRendering;
            //m_profilingSampler = new ProfilingSampler(m_profilerTag);
            ConfigureClear(ClearFlag.Depth, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context,
            ref RenderingData renderingData)
        {
            //CommandBuffer command = CommandBufferPool.Get(m_profilerTag);
            //using (new ProfilingScope(command, m_profilingSampler))
            //    command.ClearRenderTarget(true, false, Color.clear);
            //context.ExecuteCommandBuffer(command);
            //CommandBufferPool.Release(command);
        }
        //private string m_profilerTag = "ClearDepthWhenRenderCameraStackPass";
        //private FilteringSettings m_filteringSettings;
        //private ShaderTagId m_shaderTagId = ShaderTagId.none;
        //private ProfilingSampler m_profilingSampler;
    }
}
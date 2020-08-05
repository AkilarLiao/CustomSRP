using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public interface DissolveSkyBoxInterface
{
    bool IsEnable();
}

public class DissolveSkyBoxPass : ScriptableRenderPass
{
    public DissolveSkyBoxPass(RenderPassEvent evt, DissolveSkyBoxInterface theInterface)
    {
        renderPassEvent = evt;
        m_copyBackgroundRT.Init("_CopyBackgroundRT");
        m_interface = theInterface;
    }

    public void Setup(RenderTargetIdentifier colorTargetIdenfitier, RenderTargetIdentifier depthTargetIdentifier)
    {
        m_colorTargetIdentifier = colorTargetIdenfitier;
        m_depthTargetIdentifier = depthTargetIdentifier;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        bool isEnable = (m_interface != null) && m_interface.IsEnable();

        Material skyBoxMaterial = RenderSettings.skybox;
        if (isEnable)
        {
            skyBoxMaterial.SetInt("_ZTest", 0);
            cmd.EnableShaderKeyword("_PROCESS_DISSOLVE");
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            //生成拷貝目前畫面的渲染貼圖
            cmd.GetTemporaryRT(m_copyBackgroundRT.id, descriptor, FilterMode.Bilinear);
            //將目前的結果Blit到渲染貼圖
            cmd.Blit(m_colorTargetIdentifier, m_copyBackgroundRT.Identifier());
            //只指定ColorBuffer，不指定DepthBuffer，這樣就可以直接使用深度貼圖而不用cpoy(Input不可以等於Output）
            CoreUtils.SetRenderTarget(cmd, m_colorTargetIdentifier, ClearFlag.None, Color.clear, 0, CubemapFace.Unknown, -1);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
        else
        {
            skyBoxMaterial.SetInt("_ZTest", 4);
        }
        
        //渲染天空盒
        context.DrawSkybox(renderingData.cameraData.camera);

        //指定原本的Color及Depth Buffer（還原預設指定的RenderTarget）
        if(isEnable)
            CoreUtils.SetRenderTarget(cmd, m_colorTargetIdentifier, m_depthTargetIdentifier, ClearFlag.None, Color.clear, 0, CubemapFace.Unknown, -1);        

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(m_copyBackgroundRT.id);
    }

    private RenderTargetIdentifier m_colorTargetIdentifier;
    private RenderTargetIdentifier m_depthTargetIdentifier;
    private const string m_ProfilerTag = "Dissolve SkyBox Pass";
    private RenderTargetHandle m_copyBackgroundRT;
    private DissolveSkyBoxInterface m_interface = null;
}
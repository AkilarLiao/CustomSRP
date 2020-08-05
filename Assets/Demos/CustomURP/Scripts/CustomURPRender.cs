using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace CustomURP
{
    public class CustomURPRender : UnityEngine.Rendering.Universal.ScriptableRenderer, DissolveSkyBoxInterface
    {
        public CustomURPRender(CustomURPRenderDataAsset data) : base(data)
        {
            m_targetCustomURPRenderDataAsset = data;
            m_cameraColorAttachment.Init("_CameraColorTexture");
            m_cameraDepthAttachment.Init("_CameraDepthAttachment");            

            Material blitMaterial = CoreUtils.CreateEngineMaterial(data.blitShader);

            m_opaqueObjectPass = new DrawObjectsPass("Render Opaques", true,
                UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRenderingOpaques,
                RenderQueueRange.opaque, -1,
                StencilState.defaultValue, 0);

            m_transparentObjectPass =new DrawObjectsPass("Render Transparents", false,
                RenderPassEvent.BeforeRenderingTransparents,
                RenderQueueRange.transparent, -1,
                StencilState.defaultValue, 0);

            m_finalBlitPass = new FinalBlitPass(RenderPassEvent.AfterRendering, blitMaterial);
            m_drawSkyBoxPass = new DissolveSkyBoxPass(RenderPassEvent.BeforeRenderingSkybox, this);

            m_clearDepthWhenRenderCameraStackPass = new ClearDepthWhenRenderCameraStackPass();

            //setup reference rendering feature for support camera stacking.
            supportedRenderingFeatures = new RenderingFeatures()
            {
                cameraStacking = true,
            };
        }
        public override void Setup(ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            if (renderingData.cameraData.renderType == CameraRenderType.Overlay)
            {
                //直接指定ViewPort為RenderTarget.
                ConfigureCameraTarget(BuiltinRenderTextureType.CameraTarget,
                    BuiltinRenderTextureType.CameraTarget);
                //預設URP不會自動清ViewPort depth buffer, 所以加了一個pass來做清除.
                EnqueuePass(m_clearDepthWhenRenderCameraStackPass);
                EnqueuePass(m_opaqueObjectPass);
                EnqueuePass(m_transparentObjectPass);
                //直接畫到view port，所以不用FinalBlitPass
                return;
            }

            CreateCameraRenderTarget(context, ref renderingData.cameraData);
            ConfigureCameraTarget(m_cameraColorAttachment.Identifier(), m_cameraDepthAttachment.Identifier());

            foreach (var feature in rendererFeatures)
                feature.AddRenderPasses(this, ref renderingData);
            
            //加入opaque object pass
            EnqueuePass(m_opaqueObjectPass);

            //加入transparency object pass
            EnqueuePass(m_transparentObjectPass);

            Camera camera = renderingData.cameraData.camera;
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                m_drawSkyBoxPass.Setup(m_cameraColorAttachment.Identifier(), m_cameraDepthAttachment.Identifier());
                //加入dissolve skybox pass
                EnqueuePass(m_drawSkyBoxPass);
            }

            RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            //加入final pass(將結果繪製到veiwport)
            m_finalBlitPass.Setup(cameraTargetDescriptor, m_cameraColorAttachment);
            EnqueuePass(m_finalBlitPass);
        }

        void CreateCameraRenderTarget(ScriptableRenderContext context, ref CameraData cameraData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(k_CreateCameraTextures);
            var descriptor = cameraData.cameraTargetDescriptor;
            int msaaSamples = descriptor.msaaSamples;

            var colorDescriptor = descriptor;
            colorDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(m_cameraColorAttachment.id, colorDescriptor, FilterMode.Bilinear);

            var depthDescriptor = descriptor;
            depthDescriptor.colorFormat = RenderTextureFormat.Depth;
            depthDescriptor.depthBufferBits = 32;
            depthDescriptor.bindMS = msaaSamples > 1 && !SystemInfo.supportsMultisampleAutoResolve && (SystemInfo.supportsMultisampledTextures != 0);
            cmd.GetTemporaryRT(m_cameraDepthAttachment.id, depthDescriptor, FilterMode.Point);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FinishRendering(CommandBuffer cmd)
        {   
            cmd.ReleaseTemporaryRT(m_cameraColorAttachment.id);
            cmd.ReleaseTemporaryRT(m_cameraDepthAttachment.id);
        }
        bool DissolveSkyBoxInterface.IsEnable()
        {
            return m_targetCustomURPRenderDataAsset.isEnableDissolveSky;
        }

        private RenderTargetHandle m_cameraColorAttachment;
        private RenderTargetHandle m_cameraDepthAttachment;        
        private const string k_CreateCameraTextures = "Create Camera Texture";

        private DrawObjectsPass m_opaqueObjectPass = null;
        private DrawObjectsPass m_transparentObjectPass = null;
        private FinalBlitPass m_finalBlitPass = null;
        private DissolveSkyBoxPass m_drawSkyBoxPass = null;
        private ClearDepthWhenRenderCameraStackPass m_clearDepthWhenRenderCameraStackPass = null;
        private CustomURPRenderDataAsset m_targetCustomURPRenderDataAsset = null;
    }
}

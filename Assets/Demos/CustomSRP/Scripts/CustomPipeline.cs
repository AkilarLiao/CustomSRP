using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class CustomPipeline : RenderPipeline
{
    CommandBuffer cameraBuffer = new CommandBuffer
    {
        name = "Render Camera"
    };
    CustomPipelineAsset _targetAsset = null;

    public CustomPipeline(CustomPipelineAsset targetAsset)
    {
        _targetAsset = targetAsset;
        _debugRTProcessor.Initialize();
    }

    protected override void Dispose(bool disposing)
    {
        _debugRTProcessor.Release();
        base.Dispose(disposing);
    }

    //ScriptableRenderContext，跟底層繪圖API溝通的橋梁
    protected override void Render(ScriptableRenderContext renderContext,
        Camera[] cameras)
    {
        BeginFrameRendering(renderContext, cameras);
        foreach (var camera in cameras)
        {
            Render(renderContext, camera);
        }
        EndFrameRendering(renderContext, cameras);
    }

    private void Render(ScriptableRenderContext context, Camera camera)
    {
        ScriptableCullingParameters cullingParameters;
        if (!camera.TryGetCullingParameters(false, out cullingParameters))
            return;

#if UNITY_EDITOR
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
#endif  
        cameraBuffer.Clear();

        CullingResults cullResults = context.Cull(ref cullingParameters);

        context.SetupCameraProperties(camera, false);

        //建立RenderTexture,並指定其為RenderTarget
        RenderTextureDescriptor descriptor = CreateRenderTextureDescriptor(camera);

        if (_cameraColorAttachment.id == -1)
            _cameraColorAttachment.Init("_CameraColorTexture");

        cameraBuffer.GetTemporaryRT(_cameraColorAttachment.id,
            descriptor, FilterMode.Bilinear);

        RenderTargetIdentifier colorIdentifiy = _cameraColorAttachment.Identifier();

        cameraBuffer.SetRenderTarget(colorIdentifiy,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

        cameraBuffer.ClearRenderTarget(true, true, Color.clear);
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();

        SortingSettings sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        var drawSettings = new DrawingSettings(
            new ShaderTagId("SRPDefaultUnlit"), sortingSettings
        );

        var filterSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(
            cullResults, ref drawSettings, ref filterSettings
        );
        _debugRTProcessor.Add(ref context, ref colorIdentifiy);

        context.DrawSkybox(camera);
        _debugRTProcessor.Add(ref context, ref colorIdentifiy);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(
            cullResults, ref drawSettings, ref filterSettings
        );
        _debugRTProcessor.Add(ref context, ref colorIdentifiy);

        //畫在後置特效之前的Gizmos
#if UNITY_EDITOR
        if (UnityEditor.Handles.ShouldRenderGizmos())
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
#endif
        //後置特效處理，在這裡做…

        //final blit to frame buffer（將目前的RenderTextures的結果
        //，用一個Quad畫回FrameBuffer
        if (camera.cameraType == CameraType.Game)
        {
            cameraBuffer.SetGlobalTexture("_BlitTex", colorIdentifiy);
            cameraBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget,
                RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store);
            cameraBuffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cameraBuffer.SetViewport(camera.pixelRect);
            cameraBuffer.DrawMesh(fullscreenMesh, Matrix4x4.identity, blitMaterial);            
        }
        else
        {
            //因為工具的Camera是指向一個RenderTexture，要直接blitting過去，
            //否則會導致，Gizmos畫不出來。
            cameraBuffer.Blit(colorIdentifiy, BuiltinRenderTextureType.CameraTarget);
        }
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();

        //釋放RenderTexture
        cameraBuffer.ReleaseTemporaryRT(_cameraColorAttachment.id);
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();        

        //畫在後置特效之後的Gizmos
#if UNITY_EDITOR
        if (UnityEditor.Handles.ShouldRenderGizmos())
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
#endif

        _debugRTProcessor.Render(ref context, camera, blitMaterial);
        context.Submit();
    }

    private RenderTextureDescriptor CreateRenderTextureDescriptor(Camera camera)
    {
        RenderTextureDescriptor desc;
        desc = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight);
        desc.width = (int)((float)desc.width * _targetAsset._renderScale);
        desc.height = (int)((float)desc.height * _targetAsset._renderScale);
        desc.colorFormat = RenderTextureFormat.Default;
        desc.depthBufferBits = 32;
        desc.enableRandomWrite = false;
        desc.sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear);
        //desc.msaaSamples = msaaSamples;
        desc.msaaSamples = 1;
        desc.bindMS = false;
        return desc;
    }

    private Material _blitMaterial = null;
    public Material blitMaterial
    {
        get
        {
            if (_blitMaterial != null)
                return _blitMaterial;
            if ((!_targetAsset) || (!_targetAsset._blitShader))
                return null;
            _blitMaterial = new Material(_targetAsset._blitShader);
            return _blitMaterial;
        }
    }

    static Mesh _fullscreenMesh = null;
    static public Mesh fullscreenMesh
    {
        get
        {
            if (_fullscreenMesh != null)
                return _fullscreenMesh;

            float topV = 1.0f;
            float bottomV = 0.0f;

            _fullscreenMesh = new Mesh { name = "Fullscreen Quad" };
            _fullscreenMesh.SetVertices(new List<Vector3>
                {
                    new Vector3(-1.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f,  1.0f, 0.0f),
                    new Vector3(1.0f, -1.0f, 0.0f),
                    new Vector3(1.0f,  1.0f, 0.0f)
                });

            _fullscreenMesh.SetUVs(0, new List<Vector2>
                {
                    new Vector2(0.0f, bottomV),
                    new Vector2(0.0f, topV),
                    new Vector2(1.0f, bottomV),
                    new Vector2(1.0f, topV)
                });

            _fullscreenMesh.SetIndices(new[] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0, false);
            _fullscreenMesh.UploadMeshData(true);
            return _fullscreenMesh;
        }
    }

    private RenderTargetHandle _cameraColorAttachment;
    private DebugRTProcessor _debugRTProcessor = new DebugRTProcessor();
    public struct RenderTargetHandle
    {
        public int id { set; get; }

        public static readonly RenderTargetHandle CameraTarget = new RenderTargetHandle { id = -1 };

        public void Init(string shaderProperty)
        {
            id = Shader.PropertyToID(shaderProperty);
        }

        public RenderTargetIdentifier Identifier()
        {
            if (id == -1)
            {
                return BuiltinRenderTextureType.CameraTarget;
            }
            return new RenderTargetIdentifier(id);
        }

        public bool Equals(RenderTargetHandle other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is RenderTargetHandle && Equals((RenderTargetHandle)obj);
        }

        public override int GetHashCode()
        {
            return id;
        }

        public static bool operator ==(RenderTargetHandle c1, RenderTargetHandle c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(RenderTargetHandle c1, RenderTargetHandle c2)
        {
            return !c1.Equals(c2);
        }
    }
}
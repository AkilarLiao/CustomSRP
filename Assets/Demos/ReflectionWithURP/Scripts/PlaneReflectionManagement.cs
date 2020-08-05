using UnityEngine;
using CustomURP;
using UnityEngine.Rendering;

[ExecuteAlways]
public class PlaneReflectionManagement : MonoBehaviour
{   
    private void OnEnable()
    {
        Initialize();
    }
    private void OnDisable()
    {        
        Release();
    }
    private void OnDestroy()
    {
        Release();
    }
    private void Update()
    {
#if UNITY_EDITOR
        ResetReflectionTransform();
        m_planeReflectionProcessor.SetResolutionType(m_resolutionType);
#endif
    }
    private void Initialize()
    {
        m_planeReflectionProcessor.Initialize();
        m_selfTransform = gameObject.transform;
        ResetReflectionTransform();
        m_planeReflectionProcessor.SetResolutionType(m_resolutionType);
        m_originalUniversalRenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
        GraphicsSettings.renderPipelineAsset = m_universalRenderPipelineAsset;
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        m_meshMaxHeight = meshFilter.sharedMesh.bounds.max.y;
    }
    private void Release()
    {
        GraphicsSettings.renderPipelineAsset = m_originalUniversalRenderPipelineAsset;
        m_planeReflectionProcessor.Release();
    }

    private void ResetReflectionTransform()
    {
        float range = Mathf.Cos(Time.time);
        m_selfTransform.eulerAngles = new Vector3(0.0f, 0.0f, range * 20.0f);
        Vector3 position = m_selfTransform.position;
        m_planeReflectionProcessor.SetTargetPosition(ref position);
        Vector3 upAxis = m_selfTransform.up;
        m_planeReflectionProcessor.SetTargetNormal(ref upAxis);
        float planeOffest = m_selfTransform.lossyScale.y * m_meshMaxHeight * m_extnedPlaneOffestRatio;
        m_planeReflectionProcessor.SetClipPlaneOffest(planeOffest);
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, 200, 200), m_planeReflectionProcessor.GetReflectionRenderTexture(), ScaleMode.ScaleToFit,
           false);
    }
#endif
    [SerializeField]
    private PlaneReflectionProcessor.RESOLUTION_TYPE m_resolutionType = PlaneReflectionProcessor.RESOLUTION_TYPE.HALF;
    [SerializeField]
    private UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset m_universalRenderPipelineAsset = null;
    [Range(1.0f, 5.0f)]
    [SerializeField]
    private float m_extnedPlaneOffestRatio = 1.0f;
    private Transform m_selfTransform = null;
    private PlaneReflectionProcessor m_planeReflectionProcessor = new PlaneReflectionProcessor();
    private UnityEngine.Rendering.RenderPipelineAsset m_originalUniversalRenderPipelineAsset = null;
    private float m_meshMaxHeight = 0.0f;
    

    
}

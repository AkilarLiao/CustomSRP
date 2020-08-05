using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class ManualPipelineManagement : MonoBehaviour
{
    // Start is called before the first frame update
    //private void Start()
    private void OnEnable()
    {
        _manualPipelineProcessor.Initialize(gameObject.GetComponent<Camera>(), _skyBoxMesh, _skyBoxMaterial);
        m_originalPipelineAsset = GraphicsSettings.renderPipelineAsset;
        GraphicsSettings.renderPipelineAsset = null;
    }
    //private void OnDestroy()
    private void OnDisable()
    {
        _manualPipelineProcessor.Release();
        GraphicsSettings.renderPipelineAsset = m_originalPipelineAsset;
    }
    private void OnPostRender()
    {
        _manualPipelineProcessor.Render(_renderScale);
    }
    private ManualPipelineProcessor _manualPipelineProcessor = new ManualPipelineProcessor();
    [SerializeField]
    private Material _skyBoxMaterial = null;
    [SerializeField]
    private Mesh _skyBoxMesh = null;
    [SerializeField]
    [Range(0.1f, 1.0f)]
    private float _renderScale = 1.0f;

    private RenderPipelineAsset m_originalPipelineAsset = null;
}

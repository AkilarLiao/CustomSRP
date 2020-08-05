using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class ExtendURPManagement : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnEnable()
    {
        //Graphics.setting
        GraphicsSettings.renderPipelineAsset = m_URPPipelineAsset;
        OutLineRenderFeature.IsSkip = false;
    }

    // Update is called once per frame
    private void OnDisable()
    {
        GraphicsSettings.renderPipelineAsset = null;
        OutLineRenderFeature.IsSkip = true;
    }
    [SerializeField]
    private UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset m_URPPipelineAsset = null;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CustomURPManagement : MonoBehaviour
{
    private void OnEnable()
    {
        m_originalUniversalRenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
        GraphicsSettings.renderPipelineAsset = m_universalRenderPipelineAsset;
        if (m_targetDisplayFPS)
            m_targetDisplayFPS.AppendExtendString += AppendExtendStringCB;
    }
    private void OnDisable()
    {
        if (m_targetDisplayFPS)
            m_targetDisplayFPS.AppendExtendString -= AppendExtendStringCB;
        GraphicsSettings.renderPipelineAsset = m_originalUniversalRenderPipelineAsset;
    }
    private void AppendExtendStringCB(out string text)
    {   
        if(m_targetCustomURPRenderDataAsset.isEnableDissolveSky)
            text = "\nEnalbe DissolveSky";
        else
            text = "\nDisable DissolveSky";
    }
    [SerializeField]
    private UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset m_universalRenderPipelineAsset = null;
    [SerializeField]
    private DisplayFPS m_targetDisplayFPS = null;
    [SerializeField]
    private CustomURP.CustomURPRenderDataAsset m_targetCustomURPRenderDataAsset = null;
    private UnityEngine.Rendering.RenderPipelineAsset m_originalUniversalRenderPipelineAsset = null;
}

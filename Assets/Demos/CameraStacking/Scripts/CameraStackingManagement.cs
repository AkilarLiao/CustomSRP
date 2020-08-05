using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CameraStackingManagement : MonoBehaviour
{
    private void OnEnable()
    {
        m_originalUniversalRenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
        RestPipelineAsset();
        if (m_targetDisplayFPS)
            m_targetDisplayFPS.AppendExtendString += AppendExtendStringCB;

        if (m_targetCustomURPRenderDataAsset)
            m_originalFlag = m_targetCustomURPRenderDataAsset.isEnableDissolveSky;

        if (m_defaultURPRenderPipelineAsset)
        {
            m_defaultURPRenderRenderScale = m_defaultURPRenderPipelineAsset.renderScale;
            m_defaultURPRenderPipelineAsset.renderScale = m_renderScale;
        }

        if (m_customURPRenderPipelineAsset)
        {
            m_customURPRenderRenderScale = m_customURPRenderPipelineAsset.renderScale;
            m_customURPRenderPipelineAsset.renderScale = m_renderScale;
        }

        m_targetCustomURPRenderDataAsset.isEnableDissolveSky = false;
    }
    private void OnDisable()
    {
        if (m_targetCustomURPRenderDataAsset)
            m_targetCustomURPRenderDataAsset.isEnableDissolveSky = m_originalFlag;

        if (m_defaultURPRenderPipelineAsset)
            m_defaultURPRenderPipelineAsset.renderScale = m_defaultURPRenderRenderScale;

        if (m_customURPRenderPipelineAsset)
            m_customURPRenderPipelineAsset.renderScale = m_customURPRenderRenderScale;        

        if (m_targetDisplayFPS)
            m_targetDisplayFPS.AppendExtendString -= AppendExtendStringCB;
        GraphicsSettings.renderPipelineAsset = m_originalUniversalRenderPipelineAsset;
    }
    private void AppendExtendStringCB(out string text)
    {
        //if(m_targetCustomURPRenderDataAsset.isEnableDissolveSky)
        //text = "\nEnalbe DissolveSky";
        //else
        //text = "\nDisable DissolveSky";
        text = m_isCustom ? "\nCustomURP" : "\nDefaultURP";
    }
    private void OnValidate()
    {
        RestPipelineAsset();

        if (m_defaultURPRenderPipelineAsset)
            m_defaultURPRenderPipelineAsset.renderScale = m_renderScale;

        if (m_customURPRenderPipelineAsset)            
            m_customURPRenderPipelineAsset.renderScale = m_renderScale;
    }

    private void RestPipelineAsset()
    {
        GraphicsSettings.renderPipelineAsset = m_isCustom ? m_customURPRenderPipelineAsset :
            m_defaultURPRenderPipelineAsset;
    }

    [SerializeField]
    private UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset
        m_defaultURPRenderPipelineAsset = null;
    [SerializeField]
    private UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset
        m_customURPRenderPipelineAsset = null;
    [SerializeField]
    private DisplayFPS m_targetDisplayFPS = null;
    [SerializeField]
    private CustomURP.CustomURPRenderDataAsset m_targetCustomURPRenderDataAsset = null;
    private UnityEngine.Rendering.RenderPipelineAsset
        m_originalUniversalRenderPipelineAsset = null;
    private bool m_originalFlag = false;
    [SerializeField]
    private bool m_isCustom = true;
    [Range (0.001f, 1.0f)]
    [SerializeField]
    private float m_renderScale = 1.0f;

    private float m_defaultURPRenderRenderScale = 1.0f;
    private float m_customURPRenderRenderScale = 1.0f;
}

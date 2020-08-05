using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CustomSRPManagement : MonoBehaviour
{
    private void OnEnable()
    {
        //Graphics.setting
        GraphicsSettings.renderPipelineAsset = m_customPipelineAsset;
    }
    private void OnDisable()
    {
        GraphicsSettings.renderPipelineAsset = null;
    }
    [SerializeField]
    private CustomPipelineAsset m_customPipelineAsset = null;
}

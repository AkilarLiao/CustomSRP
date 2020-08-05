using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomURP
{
    [CreateAssetMenu(fileName = "CustomURPRenderDataAsset", menuName = "Rendering/CustomURPRender/CustomURPRenderDataAsset", order = 1)]
    public class CustomURPRenderDataAsset : UnityEngine.Rendering.Universal.ScriptableRendererData
    {
        public Shader blitShader
        {
            get => m_blitShader;
        }

        public bool isEnableDissolveSky
        {
            get => m_enableDissolveSky;
            set { m_enableDissolveSky = value; }
        }

        protected override UnityEngine.Rendering.Universal.ScriptableRenderer Create()
        {
            return new CustomURPRender(this);
        }
        [SerializeField] Shader m_blitShader = null;
        //[SerializeField] Shader _sampleShader = null;
        [SerializeField] bool m_enableDissolveSky = true;
    }

}

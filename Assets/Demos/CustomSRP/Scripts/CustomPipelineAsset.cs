using UnityEngine;
//using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/CustomPipeline")]
public class CustomPipelineAsset : RenderPipelineAsset {
    [Range(0.1f, 1.0f)]
    public float _renderScale = 1.0f;
    public Shader _blitShader = null;
	protected override RenderPipeline CreatePipeline() {
		return new CustomPipeline(this);
	}
}
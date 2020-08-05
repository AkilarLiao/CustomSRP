using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DebugRTProcessor
{
    public bool Initialize()
    {
        return true;
    }
    public bool Release()
    {
        ClearRenderTextures();
        return true;
    }
    public bool Add(ref ScriptableRenderContext context,
        ref RenderTargetIdentifier destTarget)
    {
        //get render texture
        RenderTexture copyRT = RenderTexture.GetTemporary(m_RTSize.x, m_RTSize.y,
            0, RenderTextureFormat.ARGB32);

        CommandBuffer command = CommandBufferPool.Get("BltingToDebugRT");
        //process blit
        command.Blit(destTarget, copyRT);
        //restore original renderTarget
        command.SetRenderTarget(destTarget);
        context.ExecuteCommandBuffer(command);
        CommandBufferPool.Release(command);
        _debugRTs.Add(copyRT);

        return true;
    }
    public bool Render(ref ScriptableRenderContext context,
        Camera camera, Material blitingMaterial)
    {
        CommandBuffer command = CommandBufferPool.Get("RenderDebugRTs");

        command.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);

        Vector2 RectSize = new Vector2(camera.pixelRect.width * _displayRatio,
            camera.pixelRect.height * _displayRatio);

        Rect displayRect = new Rect(0.0f, 0.0f, RectSize.x, RectSize.y);

        int index = 0;
        int columnIndex, rowIndex;
        var element = _debugRTs.GetEnumerator();
        while (element.MoveNext())
        {
            rowIndex = index / _columnCount;
            columnIndex = index % _columnCount;

            displayRect.x = (RectSize.x + _stepSize) * columnIndex;
            displayRect.y = camera.pixelRect.height - ((RectSize.y + _stepSize) * rowIndex + RectSize.y);

            command.SetViewport(displayRect);
            command.SetGlobalTexture("_BlitTex", element.Current);

            command.DrawMesh(CustomPipeline.fullscreenMesh, Matrix4x4.identity,
                blitingMaterial);

            ++index;
        }
        element.Dispose();

        context.ExecuteCommandBuffer(command);

        CommandBufferPool.Release(command);

        ClearRenderTextures();
        return true;
    }
    private void ClearRenderTextures()
    {
        var element = _debugRTs.GetEnumerator();
        while (element.MoveNext())
            RenderTexture.ReleaseTemporary(element.Current);
        element.Dispose();
        _debugRTs.Clear();
    }

    private static Vector2Int m_RTSize = new Vector2Int(256, 256);
    private static List<RenderTexture> _debugRTs = new List<RenderTexture>();
    private const float _displayRatio = 0.15f;
    private const int _columnCount = 6;
    private const float _stepSize = 10.0f;
}

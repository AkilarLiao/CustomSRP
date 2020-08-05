using System.Collections.Generic;
using UnityEngine;

public class ManualPipelineProcessor
{
    public bool Initialize(Camera targetCamera, Mesh skyBoxMesh, Material skyBoxMaterial)
    {
        _skyBoxMesh = skyBoxMesh;
        _skyBoxMaterial = skyBoxMaterial;
        _targetCamera = targetCamera;

        MeshRenderer[] meshRenders = Resources.FindObjectsOfTypeAll<MeshRenderer>();
        _objectInfos = new ObjectInfo[meshRenders.Length];
        for (int index = 0; index < meshRenders.Length; ++index)
        {
            MeshFilter meshFilter = meshRenders[index].gameObject.GetComponent<MeshFilter>();
            _objectInfos[index] = new ObjectInfo(
                meshRenders[index].transform,
                meshRenders[index].sharedMaterial, meshFilter.sharedMesh);
        }
        return true;
    }
    public bool Release()
    {
        if (_canvas)
            RenderTexture.ReleaseTemporary(_canvas);
        return true;
    }
    public bool Render(float renderScale)
    {
        if((_objectInfos == null) || (_objectInfos.Length <= 0))
            return false;
        //1.建立一個可以縮放尺吋的畫布，如果縮放比有變畫的話，就重建這個畫布
        //Camera camera = Camera.current;

        int width = (int)(renderScale * _targetCamera.pixelWidth);
        int height = (int)(renderScale * _targetCamera.pixelHeight);

        if ((_width != width) || (_height != height))
        {
            if (_canvas)
            {
                RenderTexture.ReleaseTemporary(_canvas);
                _canvas = null;
            }
            _width = width;
            _height = height;
            _canvas = RenderTexture.GetTemporary(_width, _height, 32);
        }

        //2.culling處理
        _opaqueCullResults.Clear();
        _alphaCullResults.Clear();
        ////取得viewFrustum的六個Plane.
        _cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(_targetCamera);
        string shaderName;
        for (int index = 0; index < _objectInfos.Length; ++index)
        {
            ////如果物件的AABB跟ViewFrustum Plnes有交集，就表示可以看到這個物件
            if (GeometryUtility.TestPlanesAABB(_cameraFrustumPlanes,
                _objectInfos[index].bound))
            {
                shaderName = _objectInfos[index].material.shader.name;
                //這裡沒有front to back，目前無法減少over draw, to do.
                if (shaderName == "Unlit/OpaqueUnLight")
                    _opaqueCullResults.AddLast(_objectInfos[index]);
                //這裡沒有做alpha排序，當重疊時會造成顯示不正確, to do.
                else if (shaderName == "Unlit/Transparent")
                    _alphaCullResults.AddLast(_objectInfos[index]);
            }
        }

        //預設正在作用的RenderTexture，就是指向Framebuffer
        RenderTexture current = RenderTexture.active;
        //2.設定畫布
        Graphics.SetRenderTarget(_canvas);
        //3.清除畫布，如果有畫sky box，底色就會填滿，可以不用清，
        //但是depth還是要清
        GL.Clear(true, false, Color.clear);

        //4.畫圖（畫物件們）
        ////畫所有的opaque.
        var element = _opaqueCullResults.GetEnumerator();
        while (element.MoveNext())
        {
            element.Current.material.SetPass(0);
            Graphics.DrawMeshNow(element.Current.mesh,
                element.Current.transform.localToWorldMatrix);
        }
        element.Dispose();

        ////畫SkyBox，在opaque之後畫可以避免over drawing.
        _skyBoxMaterial.SetPass(0);
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one * 500.0f);
        Graphics.DrawMeshNow(_skyBoxMesh, matrix);

        ////畫所有的alpha.
        element = _alphaCullResults.GetEnumerator();
        while (element.MoveNext())
        {
            element.Current.material.SetPass(0);
            Graphics.DrawMeshNow(element.Current.mesh,
                element.Current.transform.localToWorldMatrix);
        }
        element.Dispose();        

        //5.將畫布的內容拷貝Framebuufer.
        Graphics.Blit(_canvas, current);
        return true;
    }
    public void OnGUI()
    {
        if (_canvas)
            GUI.DrawTexture(new Rect(0, 0, 100, 100), _canvas);
    }    

    private RenderTexture _canvas;
    private int _width, _height;

    private ObjectInfo[] _objectInfos = null;

    private LinkedList<ObjectInfo> _opaqueCullResults = new LinkedList<ObjectInfo>();
    private LinkedList<ObjectInfo> _alphaCullResults = new LinkedList<ObjectInfo>();

    private struct ObjectInfo
    {
        public Transform transform;
        public Material material;
        public Mesh mesh;
        public Bounds bound;
        public ObjectInfo(Transform transform, Material material,
            Mesh mesh)
        {
            this.transform = transform;
            this.material = material;
            this.mesh = mesh;

            Vector3 position = transform.position;
            bound = new Bounds();
            Vector3 halfVector = Vector3.one * 0.5f;
            bound.min = position - halfVector;
            bound.max = position + halfVector;
        }
    }
    private Plane[] _cameraFrustumPlanes = null;
    private Material _skyBoxMaterial = null;
    private Mesh _skyBoxMesh = null;
    private Camera _targetCamera = null;
}

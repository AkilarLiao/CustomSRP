using UnityEngine;
using System.Collections;

//改成用draw3DText，OnGUI會造成嚴重的GC.
public class DisplayFPS : MonoBehaviour
{
    public bool OnlyShowFPS { set { _onlyShowFPS = value; } }
    private float _deltaTime = 0.0f;
    [SerializeField]
    private Color _textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
    private GUIStyle _style = null;
    private float _printFPSTime = 1.0f;
    private float _restPrintFPSTime = 0.0f;
    private static string _tempFPSText = null;
    private static Rect _TempPrintRect;
    [SerializeField]
    private bool _ShowDebugInfo = true;
    [SerializeField]
    private bool _onlyShowFPS = false;
    [SerializeField]
    private bool _leftSide = false;
    [SerializeField]
    private bool _displayFPS = true;

    public delegate void AppendExtendStringCB(out string text);
    public AppendExtendStringCB AppendExtendString { get; set; }

    private void Start()
    {
        _style = new GUIStyle();
        _style.alignment = _leftSide ? TextAnchor.UpperLeft : TextAnchor.UpperRight;
        _style.normal.textColor = _textColor;
        _restPrintFPSTime = _printFPSTime;
        //AppendExtendString += TimePropertyBlockDrawerManager.Instance.AppendExtendStringCB;
    }
    private void Update()
    {
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        _restPrintFPSTime -= Time.deltaTime;
        if (_restPrintFPSTime < 0.0f)
            _restPrintFPSTime = 0.0f;
    }

    private void OnGUI()
    {
        if (!_ShowDebugInfo)
            return;
        if (_restPrintFPSTime <= 0.0)
        {
            _restPrintFPSTime = _printFPSTime;
            int width = Screen.width, height = Screen.height;
            //Rect rect = new Rect(w-230, 0, w, h * 2 / 100);
            //Rect rect = new Rect(w - 400, 0, w, h * 2 / 100);
            //Rect rect = new Rect(w - 230, h-(int)(0.1f*(float)h), w, h * 2 / 100);
            //_TempPrintRect = new Rect(0, height - (int)(0.1f * (float)height), width, height * 2 / 100);
            _TempPrintRect = new Rect(0, height - (int)(1.0f * (float)height), width, height * 2 / 100);
            _style.fontSize = height * 5 / 100;


            if (_displayFPS)
            {
                float msec = _deltaTime * 1000.0f;
                float fPS = 1.0f / _deltaTime;
                _tempFPSText = string.Format("  {0:0.0} ms ({1:0.} fps)", msec, fPS);
            }
            else
            {
                _tempFPSText = "";
            }

            //_tempFPSText += "\nRTCacheCount:" + RenderSystem.GetRTCacheCount();

            if (!_onlyShowFPS)
            {
                //_tempFPSText += "\nQualityLevel:" + QualitySettings.names[QualitySettings.GetQualityLevel()];
                //_tempFPSText += "\nglobalMaximumLOD:" + Shader.globalMaximumLOD;
                //_tempFPSText += "\ngraphicsDeviceID:" + SystemInfo.graphicsDeviceID;
                //_tempFPSText += "\ngraphicsDeviceName:" + SystemInfo.graphicsDeviceName;
                //_tempFPSText += "\ngraphicsDeviceType:" + SystemInfo.graphicsDeviceType;
                //_tempFPSText += "\ngraphicsDeviceVersion:" + SystemInfo.graphicsDeviceVersion;
                //_tempFPSText += "\ngraphicsDeviceVendor:" + SystemInfo.graphicsDeviceVendor;
                //_tempFPSText += "\ngraphicsDeviceVendorID:" + SystemInfo.graphicsDeviceVendorID;
                //_tempFPSText += "\ngraphicsShaderLevel:" + SystemInfo.graphicsShaderLevel;
                //_tempFPSText += "\nmaxTextureSize:" + SystemInfo.maxTextureSize;
                //_tempFPSText += "\nsystemMemorySize:" + SystemInfo.systemMemorySize;
                //_tempFPSText += "\ngraphicMemorySize:" + SystemInfo.graphicsMemorySize;
                _tempFPSText += "\nSupportMultiThreaded:" + SystemInfo.graphicsMultiThreaded;
                _tempFPSText += "\nSupportsInstancing:" + SystemInfo.supportsInstancing;
                _tempFPSText += "\nsupportsComputeShaders:" + SystemInfo.supportsComputeShaders;
            }

            if (AppendExtendString != null)
            {
                string text;
                AppendExtendString(out text);
                _tempFPSText += text;
            }
        }
        if (_tempFPSText == null)
            return;

        
        GUI.Label(_TempPrintRect, _tempFPSText, _style);
    }
}
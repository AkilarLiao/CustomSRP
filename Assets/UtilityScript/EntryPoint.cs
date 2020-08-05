using System.Collections;
using UnityEngine;

public static class EntryPoint
{   
    [RuntimeInitializeOnLoadMethod]
    static void Main()
    {
#if !UNITY_EDITOR_WIN
            Application.targetFrameRate = 60;
#endif
    }
}

using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Scripting.APIUpdating;

namespace CustomURP
{
    [CustomEditor(typeof(CustomURPRenderDataAsset), true)]
    public class CustomURPRenderDataEditor : ScriptableRendererDataEditor
    {
        private static class Styles
        {
            public static readonly GUIContent RendererTitle = new GUIContent("CustomURP Renderer", "Custom URP Renderer for Custom Universal RP.");
            public static readonly GUIContent EnableDissolveSky = new GUIContent("Enable Dissolve Sky", "Controls need process dissolve sky.");
        }
        
        SerializedProperty m_enableDissolveSky;

        private void OnEnable()
        {         
            m_enableDissolveSky = serializedObject.FindProperty("m_enableDissolveSky");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Styles.RendererTitle, EditorStyles.boldLabel);         
            
            EditorGUILayout.PropertyField(m_enableDissolveSky, Styles.EnableDissolveSky);
            EditorGUILayout.Space();            

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NorthLab.Effects
{

    /// <summary>
    /// Custom editor for the <see cref="RetroScreen"/>.
    /// </summary>
    [CustomEditor(typeof(RetroScreen))]
    public class RetroScreenEditor : Editor
    {

        private SerializedProperty quad;
        private SerializedProperty targetHeight;
        private SerializedProperty overrideFPS;
        private SerializedProperty fps;
        private SerializedProperty filterMode;
        private SerializedProperty shaderName;
        private SerializedProperty mainTextureName;

        private void OnEnable()
        {
            quad = serializedObject.FindProperty(nameof(quad));
            targetHeight = serializedObject.FindProperty(nameof(targetHeight));
            overrideFPS = serializedObject.FindProperty(nameof(overrideFPS));
            fps = serializedObject.FindProperty(nameof(fps));
            filterMode = serializedObject.FindProperty(nameof(filterMode));
            shaderName = serializedObject.FindProperty(nameof(shaderName));
            mainTextureName = serializedObject.FindProperty(nameof(mainTextureName));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(quad);
            EditorGUILayout.PropertyField(targetHeight);
            EditorGUILayout.PropertyField(overrideFPS);
            if (overrideFPS.boolValue)
            {
                EditorGUILayout.PropertyField(fps);
                EditorGUILayout.HelpBox("Keep in mind that this feature is very inconsistent. It is highly sensitive to the framerate and can produce up to 15 frames difference, especially with the Vsync enabled. Consider using the Application.targetFrameRate and Vsync to achieve your goals.\nLook at the FPSDemo scene for more details.", MessageType.Warning);
            }
            EditorGUILayout.PropertyField(filterMode);
            EditorGUILayout.PropertyField(shaderName);
            EditorGUILayout.PropertyField(mainTextureName);

            serializedObject.ApplyModifiedProperties();
        }

    }

}
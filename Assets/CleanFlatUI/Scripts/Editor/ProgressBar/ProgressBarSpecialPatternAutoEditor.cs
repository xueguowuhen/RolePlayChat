﻿using UnityEditor;


namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(ProgressBarSpecialPatternAuto))]
    public class ProgressBarSpecialPatternAutoEditor : Editor
    {
        SerializedProperty minValue;
        SerializedProperty maxValue;
        SerializedProperty loadSpeed;
        SerializedProperty forward;
        SerializedProperty loop;
        SerializedProperty foreground;
        SerializedProperty foregroundArea;
        SerializedProperty patternImage;
        SerializedProperty patternRect;
        SerializedProperty patternPlay;
        SerializedProperty patternSpeed;
        SerializedProperty patternForward;
        SerializedProperty patternScale;
        SerializedProperty hasText;
        SerializedProperty text;

        protected virtual void OnEnable()
        {
            minValue = serializedObject.FindProperty("minValue");
            maxValue = serializedObject.FindProperty("maxValue");
            loadSpeed = serializedObject.FindProperty("loadSpeed");
            forward = serializedObject.FindProperty("forward");
            loop = serializedObject.FindProperty("loop");
            foreground = serializedObject.FindProperty("foreground");
            foregroundArea = serializedObject.FindProperty("foregroundArea");
            patternImage = serializedObject.FindProperty("patternImage");
            patternRect = serializedObject.FindProperty("patternRect");
            patternPlay = serializedObject.FindProperty("patternPlay");
            patternSpeed = serializedObject.FindProperty("patternSpeed");
            patternForward = serializedObject.FindProperty("patternForward");
            patternScale = serializedObject.FindProperty("patternScale");
            hasText = serializedObject.FindProperty("hasText");
            text = serializedObject.FindProperty("text");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(minValue);
            EditorGUILayout.PropertyField(maxValue);
            EditorGUILayout.PropertyField(loadSpeed);
            EditorGUILayout.PropertyField(forward);
            EditorGUILayout.PropertyField(loop);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(foregroundArea);
            EditorGUILayout.PropertyField(foreground);
            EditorGUILayout.PropertyField(patternImage);
            EditorGUILayout.PropertyField(patternRect);
            EditorGUILayout.PropertyField(patternPlay);
            if (patternPlay.boolValue == true)
            {
                EditorGUILayout.PropertyField(patternSpeed);
                EditorGUILayout.PropertyField(patternForward);
                EditorGUILayout.PropertyField(patternScale);
            }
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(hasText);
            if (hasText.boolValue == true)
            {
                EditorGUILayout.PropertyField(text);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}

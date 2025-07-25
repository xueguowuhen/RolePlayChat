﻿using UnityEditor;

namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(ContextMenuLeftClick))]
    public class ContextMenuLeftClickEditor : Editor
    {
        SerializedProperty contextMenu;
        SerializedProperty areaScope;

        protected virtual void OnEnable()
        {
            contextMenu = serializedObject.FindProperty("contextMenu");
            areaScope = serializedObject.FindProperty("areaScope");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(contextMenu);
            EditorGUILayout.PropertyField(areaScope);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

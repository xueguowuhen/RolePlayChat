﻿using UnityEditor;

namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(NotificationContentFitterWithButtonUI))]
    public class NotificationContentFitterWithButtonUIEditor : Editor
    {
        SerializedProperty button;
        SerializedProperty notification;

        protected virtual void OnEnable()
        {
            button = serializedObject.FindProperty("button");
            notification = serializedObject.FindProperty("notification");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(button);
            EditorGUILayout.PropertyField(notification);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

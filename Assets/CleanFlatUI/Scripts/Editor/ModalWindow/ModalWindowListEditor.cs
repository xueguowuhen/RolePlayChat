﻿using UnityEditor;

namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(ModalWindowList))]
    public class ModalWindowListEditor : Editor
    {
        SerializedProperty iconTitle;
        SerializedProperty title;
        SerializedProperty buttonClose;
        SerializedProperty buttonConfirm;
        SerializedProperty buttonCancel;
        SerializedProperty animator;
        SerializedProperty onConfirmClick;
        SerializedProperty onCancelClick;
        SerializedProperty contentRect;
        SerializedProperty itemTemplate;
        SerializedProperty itemText;
        SerializedProperty itemImage;
        SerializedProperty itemSelect;
        SerializedProperty padding;
        SerializedProperty spacing;
        SerializedProperty options;
        SerializedProperty onConfirm;
        SerializedProperty onCancel;

        protected virtual void OnEnable()
        {
            iconTitle = serializedObject.FindProperty("iconTitle");
            title = serializedObject.FindProperty("title");
            buttonClose = serializedObject.FindProperty("buttonClose");
            buttonConfirm = serializedObject.FindProperty("buttonConfirm");
            buttonCancel = serializedObject.FindProperty("buttonCancel");
            animator = serializedObject.FindProperty("animator");
            contentRect = serializedObject.FindProperty("contentRect");
            itemTemplate = serializedObject.FindProperty("itemTemplate");
            itemText = serializedObject.FindProperty("itemText");
            itemImage = serializedObject.FindProperty("itemImage");
            itemSelect = serializedObject.FindProperty("itemSelect");
            padding = serializedObject.FindProperty("padding");
            spacing = serializedObject.FindProperty("spacing");
            options = serializedObject.FindProperty("options");
            onConfirm = serializedObject.FindProperty("onConfirm");
            onCancel = serializedObject.FindProperty("onCancel");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(iconTitle);
            EditorGUILayout.PropertyField(title);
            EditorGUILayout.PropertyField(buttonClose);
            EditorGUILayout.PropertyField(buttonConfirm);
            EditorGUILayout.PropertyField(buttonCancel);
            EditorGUILayout.PropertyField(animator);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(contentRect);
            EditorGUILayout.PropertyField(itemTemplate);
            EditorGUILayout.PropertyField(itemText);
            EditorGUILayout.PropertyField(itemImage);
            EditorGUILayout.PropertyField(itemSelect);
            EditorGUILayout.PropertyField(padding);
            EditorGUILayout.PropertyField(spacing);
            EditorGUILayout.PropertyField(options);
            if ((buttonClose.objectReferenceValue != null) || (buttonCancel.objectReferenceValue != null) || (buttonConfirm.objectReferenceValue != null))
            {
                EditorGUILayout.Space();
            }
            if (buttonConfirm.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(onConfirm);
            }
            if ((buttonClose.objectReferenceValue != null) || (buttonCancel.objectReferenceValue != null))
            {
                EditorGUILayout.PropertyField(onCancel);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}

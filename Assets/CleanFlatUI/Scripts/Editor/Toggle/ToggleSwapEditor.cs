using UnityEditor;

namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(ToggleSwap))]
    public class ToggleSwapEditor : Editor
    {
        SerializedProperty toggle;
        SerializedProperty background;
        SerializedProperty foreground;

        protected virtual void OnEnable()
        {
            toggle = serializedObject.FindProperty("toggle");
            background = serializedObject.FindProperty("background");
            foreground = serializedObject.FindProperty("foreground");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(toggle);
            EditorGUILayout.PropertyField(background);
            EditorGUILayout.PropertyField(foreground);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

using UnityEditor;

namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(ToggleCheck))]
    public class ToggleCheckEditor : Editor
    {
        SerializedProperty toggle;
        SerializedProperty animator;

        protected virtual void OnEnable()
        {
            toggle = serializedObject.FindProperty("toggle");
            animator = serializedObject.FindProperty("animator");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(toggle);
            EditorGUILayout.PropertyField(animator);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

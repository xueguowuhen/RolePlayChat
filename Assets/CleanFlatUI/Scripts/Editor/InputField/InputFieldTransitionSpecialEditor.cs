using UnityEditor;

namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(InputFieldTransitionSpecial))]
    public class InputFieldTransitionSpecialEditor : Editor
    {
        SerializedProperty inputField;
        SerializedProperty animator;

        protected virtual void OnEnable()
        {
            inputField = serializedObject.FindProperty("inputField");
            animator = serializedObject.FindProperty("animator");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(inputField);
            EditorGUILayout.PropertyField(animator);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

using UnityEditor;

namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(TransitionTwo))]
    public class TransitionTwoEditor : Editor
    {
        SerializedProperty animator;

        protected virtual void OnEnable()
        {
            animator = serializedObject.FindProperty("animator");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(animator);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

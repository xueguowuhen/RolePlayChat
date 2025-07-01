using UnityEditor;

namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(TransitionDown))]
    public class TransitionDownEditor : Editor
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

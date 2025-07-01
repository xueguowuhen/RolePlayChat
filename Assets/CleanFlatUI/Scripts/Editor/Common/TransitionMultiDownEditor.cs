using UnityEditor;

namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(TransitionMultiDown))]
    public class TransitionMultiDownEditor : Editor
    {
        SerializedProperty animators;

        protected virtual void OnEnable()
        {
            animators = serializedObject.FindProperty("animators");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(animators);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

using UnityEditor;

namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(ButtonMultiTransition))]
    public class ButtonMultiTransitionEditor : Editor
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

using UnityEditor;

namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(ModalWindowProgressBarUI))]
    public class ModalWindowProgressBarUIEditor : Editor
    {
        SerializedProperty button;
        SerializedProperty modalWindow;

        protected virtual void OnEnable()
        {
            button = serializedObject.FindProperty("button");
            modalWindow = serializedObject.FindProperty("modalWindow");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(button);
            EditorGUILayout.PropertyField(modalWindow);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

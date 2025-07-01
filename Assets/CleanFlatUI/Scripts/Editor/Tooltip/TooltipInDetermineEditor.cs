using UnityEditor;

namespace RainbowArt.CleanFlatUI
{
    [CustomEditor(typeof(TooltipInDetermine))]
    public class TooltipInDetermineEditor : Editor
    {
        SerializedProperty tooltip;

        protected virtual void OnEnable()
        {
            tooltip = serializedObject.FindProperty("tooltip");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(tooltip);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

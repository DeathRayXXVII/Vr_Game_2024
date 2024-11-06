using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class InspectorReadOnlyAttribute : PropertyAttribute { }


[CustomPropertyDrawer(typeof(InspectorReadOnlyAttribute))]
public class InspectorReadOnlyDrawer : PropertyDrawer
{
#if UNITY_EDITOR
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
#endif
}
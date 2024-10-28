#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

[CustomPropertyDrawer(typeof(SteppedRangeAttribute))]
public class StepDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SteppedRangeAttribute stepAttribute = attribute as SteppedRangeAttribute;

        if (property.propertyType == SerializedPropertyType.Float)
        {
            EditorGUI.Slider(position, property, stepAttribute.min, stepAttribute.max, label);
            property.floatValue = Mathf.Round(property.floatValue / stepAttribute.step) * stepAttribute.step;
        }
        else if (property.propertyType == SerializedPropertyType.Integer)
        {
            EditorGUI.IntSlider(position, property, (int)stepAttribute.min, (int)stepAttribute.max, label);
            property.intValue = Mathf.RoundToInt(property.intValue / stepAttribute.step) * (int)stepAttribute.step;
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Use Step with float.");
        }
    }
}
#endif
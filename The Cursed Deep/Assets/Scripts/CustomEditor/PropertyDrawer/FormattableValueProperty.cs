using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZPTools.Utility
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FormattableValue))]
    public class FormattableValueProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate the index of the element from its property path
            int index = GetElementIndex(property);

            // Set the label to {#} where # is the index, if we are inside an array
            if (index >= 0)
            {
                label = new GUIContent($"{{{index}}}");
            }

            // Total width available in the inspector
            float totalWidth = position.width;
            float padding = 5; // Padding between fields

            // Allocate width for each field dynamically based on totalWidth
            float minLabelWidth = 30;
            float labelWidth = Mathf.Min(minLabelWidth, totalWidth * 0.15f); // Allocate up to 15% of totalWidth
            float typeWidth = Mathf.Clamp(totalWidth * 0.5f, 0, 80); // Allocate 50% of totalWidth, clamped between 0 and 85 units
            float valueWidth = totalWidth - (labelWidth + typeWidth + 2 * padding); // Remaining width for the value field

            // Ensure valueWidth is never negative
            valueWidth = Mathf.Max(0, valueWidth);

            // Define rects for the property fields
            Rect labelRect = new Rect(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight);
            Rect typeRect = new Rect(position.x + labelWidth + padding, position.y, typeWidth, EditorGUIUtility.singleLineHeight);
            Rect valueRect = new Rect(position.x + labelWidth + typeWidth + 2 * padding, position.y, valueWidth, EditorGUIUtility.singleLineHeight);

            // Draw the label prefix
            EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), label);

            // Save the original indent level
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Draw the valueType enum
            SerializedProperty valueTypeProperty = property.FindPropertyRelative("valueType");
            EditorGUI.PropertyField(typeRect, valueTypeProperty, GUIContent.none);

            // Draw the appropriate value field based on the selected enum
            FormattableValue.ValueType valueType = (FormattableValue.ValueType)valueTypeProperty.enumValueIndex;
            switch (valueType)
            {
                case FormattableValue.ValueType.String:
                    SerializedProperty stringValue = property.FindPropertyRelative("stringValue");
                    EditorGUI.PropertyField(valueRect, stringValue, GUIContent.none);
                    break;

                case FormattableValue.ValueType.Int:
                    SerializedProperty intValue = property.FindPropertyRelative("intValue");
                    EditorGUI.PropertyField(valueRect, intValue, GUIContent.none);
                    break;

                case FormattableValue.ValueType.IntData:
                    SerializedProperty intDataValue = property.FindPropertyRelative("intDataValue");
                    EditorGUI.PropertyField(valueRect, intDataValue, GUIContent.none);
                    break;

                case FormattableValue.ValueType.Float:
                    SerializedProperty floatValue = property.FindPropertyRelative("floatValue");
                    EditorGUI.PropertyField(valueRect, floatValue, GUIContent.none);
                    break;

                case FormattableValue.ValueType.FloatData:
                    SerializedProperty floatDataValue = property.FindPropertyRelative("floatDataValue");
                    EditorGUI.PropertyField(valueRect, floatDataValue, GUIContent.none);
                    break;

                case FormattableValue.ValueType.Bool:
                    SerializedProperty boolValue = property.FindPropertyRelative("boolValue");
                    EditorGUI.PropertyField(valueRect, boolValue, GUIContent.none);
                    break;
            }

            // Restore the original indent level
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        
        private int GetElementIndex(SerializedProperty property)
        {
            string path = property.propertyPath;
            string[] parts = path.Split('.');

            // Iterate from the end to the start of the path
            for (int i = parts.Length - 1; i >= 0; i--)
            {
                // Look for "Array.data[#]" pattern to get the array index
                // Start from the last part and move upwards in the path due to if the array is nested it will catch the
                // last index of the pattern which will be the correct index instead of working from the start and
                // increasing in complexity the deeper the element is in the hierarchy
                // Example: "parentArray.Array.data[0].childArray.Array.data[3]" will return 3
                // It will stop at the first "data[#]" pattern found
                if (parts[i].StartsWith("data["))
                {
                    // Extract the index value from the "data[#]" part
                    string indexString = parts[i].Substring(5, parts[i].Length - 6);
                    if (int.TryParse(indexString, out int index))
                    {
                        return index; // Return immediately when the first "data[#]" is found
                    }
                }
            }

            return -1; // Default if no index is found
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight * 1.25f;
    }
#endif
}

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZPTools.Utility
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(StringFactory))]
    public class StringFactoryProperty : PropertyDrawer
    {
        // Static variable to track the root foldout state
        private static bool _isExpanded = true;
        private static bool _isHelpBoxExpanded;

        private static float lineHeight => EditorGUIUtility.singleLineHeight;

        private const float HelpBoxSizeScalar = 4f;
        private const float FormatStringSizeScalar = 4f;

        private static float helpBoxHeight => lineHeight * HelpBoxSizeScalar;
        private static float formatStringHeight => lineHeight * FormatStringSizeScalar;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin property drawing
            EditorGUI.BeginProperty(position, label, property);

            // Calculate the root foldout rect
            Rect rootFoldoutRect = new Rect(position.x, position.y, position.width, lineHeight);

            // Draw the foldout toggle for all content
            _isExpanded = EditorGUI.Foldout(rootFoldoutRect, _isExpanded, label, true);

            float currentY = position.y + lineHeight;
            
            // If the foldout is expanded, draw all child elements
            if (_isExpanded)
            {
                EditorGUI.indentLevel++;
                // Retrieve child properties
                SerializedProperty formatStringProp = property.FindPropertyRelative("formatString");
                SerializedProperty formatValuesProp = property.FindPropertyRelative("formatValues");
                
                Rect helpFoldoutRect = new Rect(position.x, currentY, position.width, lineHeight);
                
                currentY += lineHeight;

                // Draw the foldout toggle for the help box
                _isHelpBoxExpanded = EditorGUI.Foldout(helpFoldoutRect, _isHelpBoxExpanded, "[INFO] Format Modifiers:",
                    true);

                // If expanded, draw the help box
                if (_isHelpBoxExpanded)
                {
                    EditorGUI.indentLevel++;
                    Rect helpBoxRect = new Rect(position.x + EditorGUI.indentLevel * 10, currentY, position.width, helpBoxHeight);
                    EditorGUI.HelpBox(helpBoxRect,
                        "{:singular} - Singular form of the value (1 Apple)\n" +
                        "{:plural} - Plural form of the value (3 Apples)\n" +
                        "{:currency} - Currency format (100.00)\n" +
                        "{:wholeCurrency} - Whole number currency format (100)\n" +
                        "{:decimalCurrency} - Decimal currency format (100.00)",
                        MessageType.Info);
                    
                    EditorGUI.indentLevel--;
                    currentY += helpBoxHeight + 6;
                }

                // Draw the format string text box
                Rect formatStringRect = new Rect(position.x, currentY, position.width, formatStringHeight);
                EditorGUI.PropertyField(formatStringRect, formatStringProp);
                currentY += formatStringHeight + 4;

                // Draw the list/array of format values
                Rect valuesRect = new Rect(position.x, currentY, position.width, position.height - (currentY - position.y));
                EditorGUI.PropertyField(valuesRect, formatValuesProp, true);
                
                EditorGUI.indentLevel--;
            }

            // End property drawing
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Base height for the foldout itself
            float totalHeight = lineHeight;

            // If expanded, add heights for all child elements
            if (_isExpanded)
            {
                totalHeight += lineHeight;
                
                // Add help box height
                if (_isHelpBoxExpanded)
                {
                    totalHeight += helpBoxHeight + 6;
                }

                // Add height for format string
                totalHeight += formatStringHeight + 4;

                // Add height for format values
                SerializedProperty formatValuesProp = property.FindPropertyRelative("formatValues");
                totalHeight += EditorGUI.GetPropertyHeight(formatValuesProp, true);
            }

            return totalHeight;
        }
    }
#endif
}

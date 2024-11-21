using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

public class BitMaskAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BitMaskAttribute))]
public class BitMaskEnumPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get the enum type via reflection
        System.Type enumType = fieldInfo.FieldType;

        // Ensure it's an enum type with [System.Flags] attribute
        if (!enumType.IsEnum || !System.Attribute.IsDefined(enumType, typeof(System.FlagsAttribute)))
        {
            EditorGUI.LabelField(position, label.text, "Use [BitMask] with an enum marked with [System.Flags].");
            return;
        }

        // Split the position rect into two parts: one for the label, one for the dropdown
        float labelWidth = EditorGUIUtility.labelWidth; // Use Unity's default label width
        Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
        Rect dropdownRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

        // Draw the label
        EditorGUI.LabelField(labelRect, label);

        // Get the current mask value and the enum names
        int currentMaskValue = property.intValue;
        string[] enumNames = System.Enum.GetNames(enumType);
        int[] enumValues = (int[])System.Enum.GetValues(enumType);

        // Create the display value for the popup (shows selected items), skipping the first item
        string display = GetDisplayString(currentMaskValue, enumNames, enumValues);

        // Draw the dropdown button in the allocated dropdownRect
        if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(display), FocusType.Keyboard))
        {
            // Display the improved popup window for the selection
            BitMaskPopup improvedPopup = new BitMaskPopup(currentMaskValue, enumType, (newValue) =>
            {
                property.intValue = newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            PopupWindow.Show(dropdownRect, improvedPopup);
        }
    }

    // Helper method to create a display string for the selected flags
    private string GetDisplayString(int maskValue, string[] enumNames, int[] enumValues)
    {
        List<string> selectedNames = new List<string>();

        // Start from index 1 to skip "None" (index 0)
        for (int i = 1; i < enumValues.Length; i++)
        {
            if ((maskValue & enumValues[i]) == enumValues[i] && enumValues[i] != 0)
            {
                selectedNames.Add(enumNames[i]);
            }
        }

        return selectedNames.Count > 0 ? string.Join(",  ", selectedNames) : "None";
    }
}

// The improved custom popup for multi-selection with a better UI layout
public class BitMaskPopup : PopupWindowContent
{
    private int maskValue;
    private System.Type enumType;
    private System.Action<int> onValueChanged;
    private string[] enumNames;
    private int[] enumValues;

    private Vector2 scrollPosition;

    public BitMaskPopup(int currentMaskValue, System.Type enumType, System.Action<int> onValueChanged)
    {
        this.maskValue = currentMaskValue;
        this.enumType = enumType;
        this.onValueChanged = onValueChanged;
        this.enumNames = System.Enum.GetNames(enumType);
        this.enumValues = (int[])System.Enum.GetValues(enumType);
    }

    public override Vector2 GetWindowSize()
    {
        // Adjust the height calculation to account for skipping the first item
        float height = Mathf.Min((enumNames.Length - 1) * 22, 200); // Limit the maximum height
        return new Vector2(250, height);
    }

    public override void OnGUI(Rect rect)
    {
        EditorGUIUtility.labelWidth = 100;

        // Start from index 1 to skip "None" (index 0)
        for (int i = 1; i < enumNames.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            bool isSet = (maskValue & enumValues[i]) == enumValues[i];
            bool toggle = EditorGUILayout.Toggle(isSet, GUILayout.Width(20));
            EditorGUILayout.LabelField(enumNames[i], GUILayout.Width(200));

            if (toggle != isSet)
            {
                if (toggle)
                {
                    maskValue |= enumValues[i];  // Set the bit
                }
                else
                {
                    maskValue &= ~enumValues[i];  // Clear the bit
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        onValueChanged?.Invoke(maskValue);
    }
}
#endif

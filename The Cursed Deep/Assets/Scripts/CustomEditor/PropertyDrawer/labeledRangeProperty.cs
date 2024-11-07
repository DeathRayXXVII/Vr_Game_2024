#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class LabeledRangeAttribute : PropertyAttribute
{
    public float min;
    public float max;
    public string leftLabel;
    public string rightLabel;
    
    private string ConfirmString(string label) => string.IsNullOrEmpty(label) ?
        " " : label.Length <= 1 ?
            " " + label : label;

    public LabeledRangeAttribute(float min, float max, string leftLabel, string rightLabel)
    {
        if (min > max)
        {
            this.min = max;
            this.max = min;
        }
        else
        {
            this.min = min;
            this.max = max;
        }
        
        this.leftLabel = ConfirmString(leftLabel);
        this.rightLabel = ConfirmString(rightLabel);
    }
}

[CustomPropertyDrawer(typeof(LabeledRangeAttribute))]
public class LabeledRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Color originalColor = GUI.color;
        
        var labeledRangeAttribute = (LabeledRangeAttribute)attribute;
        var range = new Vector2(labeledRangeAttribute.min, labeledRangeAttribute.max);
        var leftLabel = labeledRangeAttribute.leftLabel;
        var rightLabel = labeledRangeAttribute.rightLabel;

        float yOffset = position.y + 2f;
        
        Rect labelRect = new Rect(
            position.x,
            yOffset,
            position.width,
            EditorGUIUtility.singleLineHeight
            );
        
        float sliderOffset = EditorGUIUtility.labelWidth - 12f;
        
        Rect sliderRect = new Rect(
            position.x + sliderOffset,
            yOffset, 
            position.width - sliderOffset,
            EditorGUIUtility.singleLineHeight
            );

        EditorGUI.LabelField(labelRect, label);

        property.floatValue = EditorGUI.Slider(sliderRect, property.floatValue, range.x, range.y);

        const float charSize = 15f;
        var leftCharSize = charSize * leftLabel.Length;
        var rightCharSize = charSize * rightLabel.Length;
        yOffset = sliderRect.yMax - 3;
        float height = EditorGUIUtility.singleLineHeight / 1.5f;
        
        const int pixels = 87;
        const float step = 6f;
        var rightOffset = rightLabel.Length > 1 ? (rightLabel.Length - 2) * step + pixels : pixels;
        
        Rect leftSliderLabelRect = new Rect(
            sliderRect.xMin,
            yOffset,
            leftCharSize + 2,
            height
            );
        Rect rightSliderLabelRect = new Rect(
            sliderRect.xMax - rightOffset,
            yOffset,
            rightCharSize + 2,
            height
            );
        
        GUI.color = originalColor / 1.25f;
        EditorGUI.LabelField(leftSliderLabelRect, leftLabel, EditorStyles.miniLabel);
        EditorGUI.LabelField(rightSliderLabelRect, rightLabel, EditorStyles.miniLabel);
        GUI.color = originalColor;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight * 1.7f;
}
#endif
using UnityEngine;

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
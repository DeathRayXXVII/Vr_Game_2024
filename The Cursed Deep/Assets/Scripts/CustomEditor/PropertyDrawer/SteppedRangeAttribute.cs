using UnityEngine;

public class SteppedRangeAttribute : PropertyAttribute
{
    public float min;
    public float max;
    public float step;

    public SteppedRangeAttribute(float rangeMin, float rangeMax, float step)
    {
        min = rangeMin;
        max = rangeMax;
        this.step = step;
    }
}
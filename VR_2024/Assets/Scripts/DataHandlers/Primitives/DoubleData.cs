using System;
using UnityEngine;

[CreateAssetMenu (fileName = "DoubleData", menuName = "Data/Primitive/DoubleData")]
public class DoubleData : ScriptableObject
{
    private string _saveKey;
    
    [SerializeField] private bool zeroOnEnable;
    [SerializeField] private double objectValue;

    public double value
    {
        get => objectValue;
        set => objectValue = value;
    }

    private void Awake()
    {
        _saveKey = name;
    }

    private void OnEnable()
    {
        objectValue = (zeroOnEnable) ? 0 : objectValue;
    }

    public void SetValue(int num) { objectValue = num; }
    
    public void IncrementValue() { ++objectValue; }
    
    public void DecrementValue() { --objectValue; }
    
    public void UpdateValue(int num) { objectValue += num; }

    public double GetSavedValue()
    {
        var key = name;
        value = (PlayerPrefs.HasKey(key)) ? PlayerPrefs.GetFloat(key) : 0;
        return value;
    }
    
    public void SaveCurrentValue()
    {
        var saveValue = (float) value;
        PlayerPrefs.SetFloat(_saveKey, saveValue);
        PlayerPrefs.Save();
    }
    
    public static implicit operator double(DoubleData data)
    {
        return data.value;
    }

    public static DoubleData operator --(DoubleData data)
    {
        data.value--;
        return data;
    }

    public static DoubleData operator ++(DoubleData data)
    {
        data.value--;
        return data;
    }
    
    public static bool operator ==(DoubleData data, int other)
    {
        return data != null && data.value == other;
    }

    public static bool operator !=(DoubleData data, int other)
    {
        return data != null && data.value != other;
    }

    public static bool operator >(DoubleData data, int other)
    {
        return data.value > other;
    }

    public static bool operator <(DoubleData data, int other)
    {
        return data.value < other;
    }

    public static bool operator >=(DoubleData data, int other)
    {
        return data.value >= other;
    }

    public static bool operator <=(DoubleData data, int other)
    {
        return data.value <= other;
    }

    public override bool Equals(object obj)
    {
        const double tolerance = 0.001f;
        switch (obj)
        {
            case DoubleData otherData:
                return Math.Abs(value - otherData.value) < tolerance;
            case int otherValue:
                return Math.Abs(value - otherValue) < tolerance;
            default:
                return false;
        }
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}
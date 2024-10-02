using UnityEngine;

[CreateAssetMenu (fileName = "FloatData", menuName = "Data/Primitive/FloatData")]
public class FloatData : ScriptableObject
{
    private string _saveKey;
    
    [SerializeField] private bool zeroOnEnable;
    [SerializeField] private float objectValue;

    public float value
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

    public void Set(float num) => objectValue = num;

    public void IncrementValue() => ++objectValue;
    
    public void DecrementValue() => --objectValue;
    
    public void AdjustValue(int num) => objectValue += num;

    public float GetSavedValue()
    {
        var key = name;
        value = (PlayerPrefs.HasKey(key)) ? PlayerPrefs.GetInt(key) : 0;
        return value;
    }
    
    public void SaveCurrentValue()
    {
        PlayerPrefs.SetFloat(_saveKey, objectValue);
        PlayerPrefs.Save();
    }
    
    public static implicit operator float(FloatData data)
    {
        return data.value;
    }

    public static FloatData operator --(FloatData data)
    {
        data.value--;
        return data;
    }

    public static FloatData operator ++(FloatData data)
    {
        data.value++;
        return data;
    }
    
    public static FloatData operator +(FloatData data, int other)
    {
        data.value += other;
        return data;
    }

    public static FloatData operator -(FloatData data, int other)
    {
        data.value -= other;
        return data;
    }

    public static FloatData operator *(FloatData data, int scalar)
    {
        data.value *= scalar;
        return data;
    }

    public static FloatData operator /(FloatData data, int scalar)
    {
        data.value /= scalar;
        return data;
    }
    
    public static bool operator ==(FloatData data, float other)
    {
        return data != null && Mathf.Approximately(data.value, other);
    }

    public static bool operator !=(FloatData data, float other)
    {
        return data != null && !Mathf.Approximately(data.value, other);
    }

    public static bool operator >(FloatData data, float other)
    {
        return data.value > other;
    }

    public static bool operator <(FloatData data, float other)
    {
        return data.value < other;
    }

    public static bool operator >=(FloatData data, float other)
    {
        return data.value >= other;
    }

    public static bool operator <=(FloatData data, float other)
    {
        return data.value <= other;
    }

    public override bool Equals(object obj)
    {
        switch (obj)
        {
            case FloatData otherData:
                return Mathf.Approximately(value, otherData.value);
            case float otherValue:
                return Mathf.Approximately(value, otherValue);
            default:
                return false;
        }
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}
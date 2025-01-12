using UnityEngine;

[CreateAssetMenu (fileName = "IntData", menuName = "Data/Primitive/IntData")]
public class IntData : ScriptableObject
{
    private string _saveKey;
    
    [SerializeField] private bool zeroOnEnable;
    [SerializeField] private int objectValue;
    
    public int value
    {
        get => objectValue;
        set => objectValue = value;
    }
    
    private void Awake()
    {
        value = zeroOnEnable ? 0 : value;
        _saveKey = name;
    }

    public void Set(int num) => value = num;
    public void Set(IntData otherDataObj) => value = otherDataObj.value;
    
    public void Increment() => ++value;
    
    public void Decrement() => --value;
    
    public void AdjustValue(int num) => value += num;

    public int GetSavedValue()
    {
        var key = name;
        value = (PlayerPrefs.HasKey(key)) ? PlayerPrefs.GetInt(key) : 0;
        return value;
    }
    
    public void SaveCurrentValue()
    {
        PlayerPrefs.SetInt(_saveKey, value);
        PlayerPrefs.Save();
    }
    
    public override string ToString()
    {
        return base.ToString() + $": {value}";
    }
    
    public static implicit operator int(IntData data)
    {
        return data.value;
    }
    
    public static implicit operator float(IntData data)
    {
        return data.value;
    }

    public static IntData operator --(IntData data)
    {
        data.value--;
        return data;
    }

    public static IntData operator ++(IntData data)
    {
        data.value++;
        return data;
    }
    
    public static IntData operator +(IntData data, int other)
    {
        data.value += other;
        return data;
    }

    public static IntData operator -(IntData data, int other)
    {
        data.value -= other;
        return data;
    }

    public static IntData operator *(IntData data, int scalar)
    {
        data.value *= scalar;
        return data;
    }

    public static IntData operator /(IntData data, int scalar)
    {
        data.value /= scalar;
        return data;
    } 
    
    public static bool operator ==(IntData data, int other)
    {
        return data != null && data.value == other;
    }

    public static bool operator !=(IntData data, int other)
    {
        return data != null && data.value != other;
    }

    public static bool operator >(IntData data, int other)
    {
        return data.value > other;
    }

    public static bool operator <(IntData data, int other)
    {
        return data.value < other;
    }

    public static bool operator >=(IntData data, int other)
    {
        return data.value >= other;
    }

    public static bool operator <=(IntData data, int other)
    {
        return data.value <= other;
    }

    public override bool Equals(object obj)
    {
        switch (obj)
        {
            case IntData otherData:
                return value == otherData.value;
            case int otherValue:
                return value == otherValue;
            default:
                return false;
        }
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}

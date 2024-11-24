using UnityEngine;

[CreateAssetMenu (fileName = "BoolData", menuName = "Data/Primitive/BoolData")]
public class BoolData : ScriptableObject
{
    [SerializeField] private bool objectValue;

    public bool value
    {
        get => objectValue;
        set => objectValue = value;
    }
    
    public void Set(bool newValue) => value = newValue;
    public void Set(int bitWiseBool) => value = bitWiseBool != 0;
    public void Set(float bitWiseBool) => value = bitWiseBool != 0;
    public bool Get() => value;
    
    
    public static implicit operator bool(BoolData data) => data.value;
    public static implicit operator int(BoolData data) => data.value ? 1 : 0;
}

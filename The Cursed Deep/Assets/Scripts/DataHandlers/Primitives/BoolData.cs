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
    
    public static implicit operator bool(BoolData data)
    {
        return data.value;
    }
}

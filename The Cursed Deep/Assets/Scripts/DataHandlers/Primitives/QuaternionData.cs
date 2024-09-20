using UnityEngine;

[CreateAssetMenu(fileName = "QuaternionData", menuName = "Data/Primitive/QuaternionData")]
public class QuaternionData : ScriptableObject
{
    [SerializeField] private Quaternion objectValue;

    private float x
    { get => value.x; set => objectValue.x = value; }

    private float y
    { get => value.y; set => objectValue.y = value; }

    private float z
    { get => value.z; set => objectValue.z = value; }

    private float w
    {
        get => value.w; set => objectValue.w = value; }

    public Quaternion value
    {
        get => objectValue;
        set
        {
            objectValue = value;
            x = value.x;
            y = value.y;
            z = value.z;
            w = value.w;
        }
    }
    
    public static implicit operator Quaternion(QuaternionData data)
    {
        return data.value;
    }
    
    public static Quaternion operator *(QuaternionData data, Quaternion other)
    {
        return data.value * other;
    }

    public static Vector3 operator *(QuaternionData data, Vector3 other)
    {
        return data.value * other; 
    }

    public static Quaternion operator *(QuaternionData data, float scalar)
    {
        return new Quaternion(data.x * scalar, data.y * scalar, data.z * scalar, data.w * scalar);
    }

    public static Quaternion operator /(QuaternionData data, float scalar)
    {
        return new Quaternion(data.x / scalar, data.y / scalar, data.z / scalar, data.w / scalar);
    }
}

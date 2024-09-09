using UnityEngine;

[CreateAssetMenu(fileName = "Vector3Data", menuName = "Data/Primitive/Vector3Data")]
public class Vector3Data : ScriptableObject
{
    [SerializeField] private Vector3 objectValue;

    public float x
    { get => value.x; set => objectValue.x = value; }

    public float y
    { get => value.y; set => objectValue.y = value; }

    public float z
    { get => value.z; set => objectValue.z = value; }

    public Vector3 value
    {
        get => objectValue;
        set
        {
            objectValue = value;
            x = value.x;
            y = value.y;
            z = value.z;
        }
    }
    
    public static implicit operator Vector3(Vector3Data data)
    {
        return data.value;
    }

    public static Vector3Data operator +(Vector3Data data, Vector3 other)
    {
        data.objectValue += other;
        return data;
    }

    public static Vector3Data operator -(Vector3Data data, Vector3 other)
    {
        data.objectValue -= other;
        return data;
    }

    public static Vector3Data operator *(Vector3Data data, float scalar)
    {
        data.value *= scalar;
        return data;
    }

    public static Vector3Data operator /(Vector3Data data, float scalar)
    {
        data.value /= scalar;
        return data;
    }
}
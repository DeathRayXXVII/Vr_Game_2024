using System;
using UnityEngine;

[CreateAssetMenu (fileName = "Vector2Data", menuName = "Data/Primitive/Vector2Data")]
public class Vector2Data : ScriptableObject
{
    [SerializeField] private Vector2 objectValue;

    public float x
    { get => value.x; set => objectValue.x = value; }

    public float y
    { get => value.y; set => objectValue.y = value; }

    public Vector2 value
    {
        get => objectValue;
        set
        {
            objectValue = value;
            x = value.x;
            y = value.y;
        }
    }
    
    public static implicit operator Vector2(Vector2Data data)
    {
        return data.value;
    }

    public static Vector2Data operator +(Vector2Data data, Vector2 other)
    {
        data.objectValue += other;
        return data;
    }

    public static Vector2Data operator -(Vector2Data data, Vector2 other)
    {
        data.objectValue -= other;
        return data;
    }

    public static Vector2Data operator *(Vector2Data data, float scalar)
    {
        data.value *= scalar;
        return data;
    }

    public static Vector2Data operator /(Vector2Data data, float scalar)
    {
        data.value /= scalar;
        return data;
    }
}

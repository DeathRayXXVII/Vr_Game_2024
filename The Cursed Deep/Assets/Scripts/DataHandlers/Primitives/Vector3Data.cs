using UnityEngine;

[CreateAssetMenu(fileName = "Vector3Data", menuName = "Data/Primitive/Vector3Data")]
public class Vector3Data : ScriptableObject
{
    // Serialized private Vector3 objectValue that is the protected value of the Vector3Data
    [SerializeField] private Vector3 objectValue;

    // Property x that accesses the private objectValue.x, allowing for direct access to the x value of the Vector3 instead of the need to use value.x
    public float x
    { get => value.x; set => objectValue.x = value; }

    // Property y that accesses the private objectValue.y, allowing for direct access to the y value of the Vector3 instead of the need to use value.y
    public float y
    { get => value.y; set => objectValue.y = value; }

    // Property z that accesses the private objectValue.z, allowing for direct access to the z value of the Vector3 instead of the need to use value.z
    public float z
    { get => value.z; set => objectValue.z = value; }

    // Property value that accesses the private objectValue, allowing for direct access to the Vector3
    public Vector3 value
    {
        get => objectValue; 
        set => objectValue = value;
    }
    
    // Implicit conversion when getting a Vector3 from a Vector3Data, e.g. Vector3 position = vector3Data; instead of Vector3 position = vector3Data.value;
    // This will also apply to the following overloads, making the return of Vector3Data 'data' interchangeable as a Vector3 or Vector3Data 
    public static implicit operator Vector3(Vector3Data data)
    {
        return data.value;
    }

    // Operator overload of + to add a Vector3 to a Vector3Data, e.g. vector3Data += new Vector3(1, 0, 0); instead of vector3Data.value += new Vector3(1, 0, 0);
    // This overload will also handler the addition of two Vector3Data objects because the right-hand side Vector3Data will be implicitly converted to a Vector3
    public static Vector3Data operator +(Vector3Data data, Vector3 other)
    {
        data.value += other;
        return data;
    }
    
    // Operator overload of - to subtract a Vector3 from a Vector3Data, e.g. vector3Data -= new Vector3(1, 0, 0); instead of vector3Data.value -= new Vector3(1, 0, 0);
    // This overload will also handler the subtraction of two Vector3Data objects because the right-hand side Vector3Data will be implicitly converted to a Vector3
    public static Vector3Data operator -(Vector3Data data, Vector3 other)
    {
        data.value -= other;
        return data;
    }
    
    // Operator overload of * to multiply a Vector3Data by a scalar, e.g. vector3Data *= 2; instead of vector3Data.value *= 2;
    public static Vector3Data operator *(Vector3Data data, float scalar)
    {
        data.value *= scalar;
        return data;
    }
    
    // Operator overload of / to divide a Vector3Data by a scalar, e.g. vector3Data /= 2; instead of vector3Data.value /= 2;
    public static Vector3Data operator /(Vector3Data data, float scalar)
    {
        data.value /= scalar;
        return data;
    }
}
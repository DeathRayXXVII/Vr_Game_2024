namespace ZPTools
{
    
     public struct DataType
    {
        public enum EnumDataTypes
        {
            Int,
            Float,
            // String,
            // Bool,
            // Vector2,
            // Vector3,
            // Vector4,
            // Quaternion,
            // Color,
            // Rect,
            // Bounds,
            // AnimationCurve,
            // Gradient,
            // ObjectReference,
            // LayerMask,
            // Enum,
            // Array,
            // List,
            // Dictionary,
        }
        
        public static readonly System.Collections.Generic.Dictionary<System.Enum, System.Type> TypeMappings = new()
        {
            { EnumDataTypes.Int, typeof(int) },
            { EnumDataTypes.Float, typeof(float) },
            // { EnumDataTypes.String, typeof(string) },
            // { EnumDataTypes.Bool, typeof(bool) },
            // { EnumDataTypes.Vector2, typeof(UnityEngine.Vector2) },
            // { EnumDataTypes.Vector3, typeof(UnityEngine.Vector3) },
            // { EnumDataTypes.Vector4, typeof(UnityEngine.Vector4) },
            // { EnumDataTypes.Quaternion, typeof(UnityEngine.Quaternion) },
            // { EnumDataTypes.Color, typeof(UnityEngine.Color) },
            // { EnumDataTypes.Rect, typeof(UnityEngine.Rect) },
            // { EnumDataTypes.Bounds, typeof(UnityEngine.Bounds) },
            // { EnumDataTypes.AnimationCurve, typeof(UnityEngine.AnimationCurve) },
            // { EnumDataTypes.Gradient, typeof(UnityEngine.Gradient) },
            // { EnumDataTypes.ObjectReference, typeof(UnityEngine.Object) },
            // { EnumDataTypes.LayerMask, typeof(UnityEngine.LayerMask) },
            // { EnumDataTypes.Enum, typeof(System.Enum) },
            // { EnumDataTypes.Array, typeof(System.Array) },
            // { EnumDataTypes.List, typeof(System.Collections.Generic.List<object>) },
            // { EnumDataTypes.Dictionary, typeof(System.Collections.Generic.Dictionary<object, object>) },
        };

        public static System.Type GetTypeForDataType(EnumDataTypes enumDataType)
        {
            if (TypeMappings.TryGetValue(enumDataType, out System.Type type))
            {
                return type;
            }
            throw new System.ArgumentException($"Unsupported data type: {enumDataType}");
        }

        public static object CastToType(EnumDataTypes enumDataType, object obj)
        {
            System.Type targetType = GetTypeForDataType(enumDataType);

            // Handle cases where the type might be a primitive type or require a specific cast
            try
            {
                return System.Convert.ChangeType(obj, targetType);
            }
            catch (System.InvalidCastException)
            {
                // Handle more complex types that need specific casting, e.g., Unity types
                if (targetType.IsAssignableFrom(obj.GetType()))
                {
                    return obj;
                }

                throw new System.InvalidCastException($"Cannot cast value of type {obj.GetType()} to {targetType}");
            }
        }
    }
}

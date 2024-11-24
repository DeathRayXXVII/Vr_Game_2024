using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("TransformBehavior")]
[CreateAssetMenu(fileName = "TransformData", menuName = "Data/Primitive/TransformData")]
public class TransformData : ScriptableObject
{
    [SerializeField] private Vector3 _position;
    [SerializeField] private Quaternion _rotation;
    [SerializeField] private Vector3 _scale = Vector3.one;
    internal Vector3Data PositionHandler, ScaleHandler;
    internal QuaternionData RotationHandler;
    
    private void OnValidate()
    {
        PositionHandler = CreateInstance<Vector3Data>();
        ScaleHandler = CreateInstance<Vector3Data>();
        RotationHandler = CreateInstance<QuaternionData>();
    }
    
    public Vector3 position
    {
        get => _position;
        set
        {
            _position = value;
            PositionHandler.value = value;
        }
    }
    
    public Vector3 scale
    {
        get => _scale;
        set
        {
            _scale = value;
            ScaleHandler.value = value;
        }
    }
    
    public Quaternion rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            RotationHandler.value = _rotation;
        }
    }
    
    public void Set(TransformData data)
    {
        position = data.position;
        rotation = data.rotation;
        scale = data.scale;
    }

    public static implicit operator QuaternionData(TransformData data)
    {
        return data.RotationHandler;
    }
    
    public static implicit operator Vector3Data(TransformData data)
    {
        return data.PositionHandler;
    }
}
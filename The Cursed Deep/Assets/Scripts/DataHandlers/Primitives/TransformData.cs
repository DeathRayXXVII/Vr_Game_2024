using UnityEngine;

[CreateAssetMenu(fileName = "TransformData", menuName = "Data/Primitive/TransformData")]
public class TransformData : ScriptableObject
{
    [SerializeField] private Vector3 _position;
    [SerializeField] private Quaternion _rotation;
    [SerializeField] private Vector3 _scale = Vector3.one;
    [SerializeField, HideInInspector] private Vector3Data _positionHandler;
    [SerializeField, HideInInspector] private Vector3Data _scaleHandler;
    [SerializeField, HideInInspector] private QuaternionData _rotationHandler;
    
    public Vector3Data positionHandler
    {
        get
        {
            if (_positionHandler == null) _positionHandler = CreateInstance<Vector3Data>();
            return _positionHandler;
        }
        private set
        {
            if (_positionHandler == null) _positionHandler = CreateInstance<Vector3Data>();
            _positionHandler = value;
        }
    }
    
    public Vector3Data scaleHandler
    {
        get
        {
            if (_scaleHandler == null) _scaleHandler = CreateInstance<Vector3Data>();
            return _scaleHandler;
        }
        private set
        {
            if (_scaleHandler == null) _scaleHandler = CreateInstance<Vector3Data>();
            _scaleHandler = value;
        }
    }
    public QuaternionData rotationHandler
    {
        get
        {
            if (_rotationHandler == null) _rotationHandler = CreateInstance<QuaternionData>();
            return _rotationHandler;
        }
        private set
        {
            if (_rotationHandler == null) _rotationHandler = CreateInstance<QuaternionData>();
            _rotationHandler = value;
        }
    }
    
    private void OnEnable()
    {
        positionHandler ??= CreateInstance<Vector3Data>();
        scaleHandler ??= CreateInstance<Vector3Data>();
        rotationHandler  ??= CreateInstance<QuaternionData>();
    }
    
    public Vector3 position
    {
        get => _position;
        set
        {
            _position = value;
            positionHandler.value = value;
        }
    }
    
    public Vector3 scale
    {
        get => _scale;
        set
        {
            _scale = value;
            scaleHandler.value = value;
        }
    }
    
    public Quaternion rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            rotationHandler.value = _rotation;
        }
    }
    
    public void Set(TransformData data)
    {
        position = data.position;
        rotation = data.rotation;
        scale = data.scale;
    }
    
    public override string ToString() => $"{name}:\nPosition: {position}, Rotation: {rotation}, Scale: {scale}";

    public static implicit operator QuaternionData(TransformData data)
    {
        return data.rotationHandler;
    }
    
    public static implicit operator Vector3Data(TransformData data)
    {
        return data.positionHandler;
    }
}
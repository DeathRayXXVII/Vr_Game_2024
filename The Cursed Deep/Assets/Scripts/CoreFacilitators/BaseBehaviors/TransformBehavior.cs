using UnityEngine;

public class TransformBehavior : MonoBehaviour
{
    [SerializeField] private bool _setTransformOnAwake;
    
    private Vector3 _startTransformPosition;
    private Quaternion _startTransformRotation;
    
    private void Awake()
    {
        if (_setTransformOnAwake) SetStartTransform(transform);
    }
    
    public void SetToTransform(Transform newTransform)
    {
        transform.position = newTransform.position;
        transform.rotation = newTransform.rotation;
    }
    
    public void SetToTransform(TransformData newTransform)
    {
        transform.position = newTransform.position;
        transform.rotation = newTransform.rotation;
    }

    public void SetStartTransform(Transform newPosition)
    {
        _startTransformPosition = newPosition.position;
        _startTransformRotation = newPosition.rotation;
    }

    public void SetStartTransform(TransformData newPosition)
    {
        _startTransformPosition = newPosition.position;
        _startTransformRotation = newPosition.rotation;
        Debug.Log($"Setting start transform to {_startTransformPosition} and {_startTransformRotation}", this);
    }
    public void SetToStartPosition() { transform.position = _startTransformPosition; }
    
    public void SetPosition(Vector3 newPosition) { transform.position = newPosition; }
    public void SetPosition(Vector3Data newPosition) => SetPosition(newPosition.value);
    public void SetPosition(CharacterData data) { transform.position = data.spawnPosition; }
    public void SetPosition(Transform newPosition) { transform.position = newPosition.position; }
    
    public void SetToStartRotation() { transform.rotation = _startTransformRotation; }
    public void SetRotation(Vector3 newRotation) { transform.position = newRotation; }
    public void SetRotation(Vector3Data newRotation) => SetRotation(newRotation.value);
    public void SetRotation(Transform newRotation) { transform.rotation = newRotation.rotation; }


    public void ResetToStartTransform(TransformData newPosition)
    {
        SetStartTransform(newPosition);
        ResetToStartTransform();
    }

    public void ResetToStartTransform()
    {
        Debug.Log($"Resetting to start transform, {_startTransformPosition} and {_startTransformRotation}");
        SetToStartPosition();
        SetToStartRotation();
    }

    public Vector3 GetPosition() { return transform.position; }
    private Vector3 GetStartPosition() { return _startTransformPosition; }
    private Quaternion GetStartRotation() { return _startTransformRotation; }
}

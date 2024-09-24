using UnityEngine;

public class TransformBehavior : MonoBehaviour
{
    private Vector3 _startTransformPosition;
    private Quaternion _startTransformRotation;
    
    private void Awake()
    {
        _startTransformPosition = transform.position;
        _startTransformRotation = transform.rotation;
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
    
    public void SetToStartPosition() { transform.position = _startTransformPosition; }
    public void SetPosition(Vector3 newPosition) { transform.position = newPosition; }
    public void SetPosition(Vector3Data newPosition) => SetPosition(newPosition.value);
    public void SetPosition(CharacterData data) { transform.position = data.spawnPosition; }
    public void SetPosition(Transform newPosition) { transform.position = newPosition.position; }
    
    public void SetToStartRotation() { transform.rotation = _startTransformRotation; }
    public void SetRotation(Vector3 newRotation) { transform.position = newRotation; }
    public void SetRotation(Vector3Data newRotation) => SetRotation(newRotation.value);
    public void SetRotation(Transform newRotation) { transform.rotation = newRotation.rotation; }
    
    
    public void ResetToStartTransform()
    {
        SetToStartPosition();
        SetToStartRotation();
    }

    public Vector3 GetPosition() { return transform.position; }
    private Vector3 GetStartPosition() { return _startTransformPosition; }
    private Quaternion GetStartRotation() { return _startTransformRotation; }
}

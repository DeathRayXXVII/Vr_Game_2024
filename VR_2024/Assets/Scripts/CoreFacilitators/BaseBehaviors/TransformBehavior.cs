using System.Collections;
using UnityEngine;

public class TransformBehavior : MonoBehaviour
{
    [SerializeField] private TransformData transformTrackerSO;
    private Vector3Data _activePositionTracker;
    private QuaternionData _activeRotationTracker;
    
    [SerializeField] private bool singleTrackOnStart;
    [SerializeField] private bool continuousTrackOnStart;
    
    private Coroutine _trackPostionCoroutine;
    private Coroutine _trackRotationCoroutine;
    private WaitForFixedUpdate _wffu = new();
    private bool _isTrackingPosition, _isTrackingRotation;
    
    private Transform _transform;
    private Vector3 _startTransformPosition;
    private Quaternion _startTransformRotation;
    
    private void Start()
    {
        _transform = transform;
        _startTransformPosition = _transform.position;
        _startTransformRotation = _transform.rotation;
        if (singleTrackOnStart)
        {
            TrackCurrentPosition(transformTrackerSO);
            TrackCurrentRotation(transformTrackerSO);
        }
        else
        {
            if (!continuousTrackOnStart) return;
            StartContinuousTrackingPosition(transformTrackerSO);
            StartContinuousTrackingRotation(transformTrackerSO);
        }
    }
    
    private void TrackCurrentPosition(Vector3Data tracker)
    {
        if (tracker == transformTrackerSO.PositionHandler && tracker != transformTrackerSO.ScaleHandler)
        {
            transformTrackerSO.position = _transform.position;
        }
        else
        {
            tracker.value = _transform.position;
        }
    }
    
    private void TrackCurrentRotation(QuaternionData tracker)
    {
        if (tracker == transformTrackerSO.RotationHandler)
        {
            transformTrackerSO.rotation = transform.rotation;
        }
        else
        {
            tracker.value = transform.rotation;
        }
    }
    
    private IEnumerator TrackPosition(Vector3Data tracker)
    {
        while (_isTrackingPosition)
        {
            tracker.value = _transform.position;
            yield return _wffu;
        }
    }
    
    private IEnumerator TrackRotation(QuaternionData tracker)
    {
        while (_isTrackingRotation)
        {
            tracker.value = _transform.rotation;
            yield return _wffu;
        }
    }

    public void StartContinuousTrackingPosition(Vector3Data positionTracker)
    {
        _activePositionTracker = positionTracker;
        StartTracking(ref _isTrackingPosition, ref _trackPostionCoroutine, TrackPosition(_activePositionTracker));
    }

    public void StopContinuousTrackingPosition()
    {
        StopTracking(ref _isTrackingPosition, ref _trackPostionCoroutine);
    }
    
    public void StartContinuousTrackingRotation(QuaternionData rotationTracker)
    {
        _activeRotationTracker = rotationTracker;
        StartTracking(ref _isTrackingRotation, ref _trackRotationCoroutine, TrackRotation(_activeRotationTracker));
    }
    
    public void StopContinuousTrackingRotation()
    {
        StopTracking(ref _isTrackingRotation, ref _trackRotationCoroutine);
    }
    
    private void StartTracking(ref bool isRunning, ref Coroutine internalCoroutineObject, IEnumerator coroutine)
    {
        if (isRunning) return;
        isRunning = true;
        internalCoroutineObject = StartCoroutine(coroutine);
    }

    private void StopTracking(ref bool isRunning, ref Coroutine internalCoroutineObject)
    {
        if (!isRunning) return;
        isRunning = false;
        if (internalCoroutineObject != null) StopCoroutine(internalCoroutineObject);
        internalCoroutineObject = null;
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

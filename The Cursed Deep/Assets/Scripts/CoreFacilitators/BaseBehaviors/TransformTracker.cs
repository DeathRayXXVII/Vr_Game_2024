using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZPTools.Interface;

public class TransformTracker : MonoBehaviour, INeedButton
{
    public bool allowDebug;
    
    [SerializeField] private TransformData transformTrackerSO;
    private Vector3Data _activePositionTracker;
    private QuaternionData _activeRotationTracker;
    
    [SerializeField] private bool singleTrackOnStart;
    [SerializeField] private bool continuousTrackOnStart;
    
    private Coroutine _trackPostionCoroutine;
    private Coroutine _trackRotationCoroutine;
    private WaitForFixedUpdate _wffu = new();
    private bool _isTrackingPosition, _isTrackRotation;

    private bool _initialized;
    
    public void Initialize()
    {
        if (_initialized) return;
        
        if (transformTrackerSO == null) 
        { 
            Debug.LogWarning("Tracker TransformData is missing.", this);
            return;
        }

        TrackCurrentTransform();
        
// #if UNITY_EDITOR
        if (allowDebug)
        {
            Debug.Log($"Initializing TransformTracker: {name}", this);
            Debug.Log($"[{name}] Tracked: {transformTrackerSO}", transformTrackerSO);
        }
// #endif
        
        _initialized = true;
    }
    
    private void Awake()
    {
        if (transformTrackerSO == null) 
        { 
            Debug.LogWarning("Tracker TransformData is missing.", this);
            return;
        }
        
        if (singleTrackOnStart)
        {
            Initialize();
        }
        else
        {
            if (!continuousTrackOnStart) return;
            StartContinuousTrackPosition(transformTrackerSO);
            StartContinuousTrackRotation(transformTrackerSO);
        }
    }
    
    public void TrackCurrentTransform(TransformData tracker)
    {
        TrackCurrentPosition(tracker);
        TrackCurrentRotation(tracker);
    }
    
    public void TrackCurrentTransform()
    {
        TrackCurrentPosition();
        TrackCurrentRotation();
    }
    
    public void TrackCurrentPosition()
    {
        if (transformTrackerSO != null) { TrackCurrentPosition(transformTrackerSO); }
    }
    
    public void TrackCurrentPosition(TransformData tracker)
    {
        TrackCurrentPosition(tracker.positionHandler);
    }
    
    public void TrackCurrentPosition(Vector3Data tracker)
    {
        if (tracker == transformTrackerSO.positionHandler && tracker != transformTrackerSO.scaleHandler)
        {
            transformTrackerSO.position = transform.position;
        }
        else
        {
            tracker.value = transform.position;
        }
    }
    
    public void TrackCurrentRotation()
    {
        if (transformTrackerSO != null) { TrackCurrentRotation(transformTrackerSO); }
    }
    
    public void TrackCurrentRotation(TransformData tracker)
    {
        TrackCurrentRotation(tracker.rotationHandler);
    }
    
    public void TrackCurrentRotation(QuaternionData tracker)
    {
        if (tracker == transformTrackerSO.rotationHandler)
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
            tracker.value = transform.position;
            yield return _wffu;
        }
    }
    
    private IEnumerator TrackRotation(QuaternionData tracker)
    {
        while (_isTrackRotation)
        {
            tracker.value = transform.rotation;
            yield return _wffu;
        }
    }

    public void StartContinuousTrackPosition(Vector3Data positionTracker)
    {
        _activePositionTracker = positionTracker;
        StartTracking(ref _isTrackingPosition, ref _trackPostionCoroutine, TrackPosition(_activePositionTracker));
    }

    public void StopContinuousTrackPosition()
    {
        StopTracking(ref _isTrackingPosition, ref _trackPostionCoroutine);
    }
    
    public void StartContinuousTrackRotation(QuaternionData rotationTracker)
    {
        _activeRotationTracker = rotationTracker;
        StartTracking(ref _isTrackRotation, ref _trackRotationCoroutine, TrackRotation(_activeRotationTracker));
    }
    
    public void StopContinuousTrackRotation()
    {
        StopTracking(ref _isTrackRotation, ref _trackRotationCoroutine);
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
    
    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)>
        {
#if UNITY_EDITOR
            (TrackCurrentPosition, "Track Current Position"),
            (TrackCurrentRotation, "Track Current Rotation")
#endif
        };
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    private List<TransformTracker> _transformTrackers;
    
    private bool PopulateTrackers()
    {
        var trackers = FindObjectsOfType<TransformTracker>();
        _transformTrackers = trackers != null && trackers.Length != 0 ? new List<TransformTracker>(trackers) : null;
        return _transformTrackers != null;
    }
    
    public UnityEvent beforeInitialization;
    
    // Editor Triggered Custom Events
    public UnityEvent onGameStart;

    private readonly WaitForFixedUpdate _waitFixed = new();
    protected Coroutine _initCoroutine;
    public bool initialized { get; protected set; }
    
    protected virtual void Awake()
    {
        initialized = false;
        Debug.Log("GameManager Awake, Initializing trackers.", this);
        PopulateTrackers();
    }
    
    protected virtual void Start()
    {
        _initCoroutine ??= StartCoroutine(Initialize());
    }

    protected virtual IEnumerator Initialize()
    {
        yield return _waitFixed;
        
        // Initialize Trackers and then trigger beforeInitialization event
        yield return InitializeTrackers();
        
        initialized = true;
        _initCoroutine = null;
    }

    protected IEnumerator InitializeTrackers()
    {
        if (_transformTrackers == null && !PopulateTrackers())
        {
            Debug.LogError("No TransformTrackers found in scene.", this);
            beforeInitialization.Invoke();
            yield return _waitFixed;
            
            yield break;
        }
        
        System.Diagnostics.Debug.Assert(_transformTrackers != null, "No TransformTrackers found in scene.");
        foreach (var tracker in _transformTrackers)
        {
            Debug.LogWarning($"Initializing {tracker.name}", tracker);
            tracker.Initialize();
            yield return null;
        }
        yield return _waitFixed;
        
        beforeInitialization.Invoke();
    }
    
    public void StartGame()
    {
        onGameStart.Invoke();
    }
}
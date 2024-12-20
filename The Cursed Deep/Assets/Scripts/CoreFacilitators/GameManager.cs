using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    [SerializeField] private BoolData _tutorialIsActive;
    
    [SerializeField] private SceneBehavior _sceneBehavior;
    [SerializeField] private bool startGameAfterSceneBehaviorTransition = true;
    private bool runTutorial => _tutorialIsActive ?? false;
    
    private List<TransformTracker> _transformTrackers;
    
    private bool PopulateTrackers()
    {
        var trackers = FindObjectsOfType<TransformTracker>();
        _transformTrackers = trackers != null && trackers.Length != 0 ? new List<TransformTracker>(trackers) : null;
        return _transformTrackers != null;
    }
    
    public UnityEvent beforeInitialization;
    public UnityEvent tutorialInitialization;
    public UnityEvent onGameStart;
    public UnityEvent onTutorialGameStart;

    private readonly WaitForFixedUpdate _waitFixed = new();
    protected Coroutine _initCoroutine;
    public bool initialized { get; protected set; }
    
    protected virtual void Awake()
    {
        initialized = false;
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

        yield return _waitFixed;
        
        if (_sceneBehavior == null)
        {
            Debug.LogError("[ERROR] SceneBehavior is null, cannot initialize GameManager.", this);
        }
        else
        {
            yield return StartCoroutine(_sceneBehavior.Initialize());
        }
        
        if (startGameAfterSceneBehaviorTransition)
        {
            StartGame();
        }
        
        initialized = true;
        _initCoroutine = null;
    }

    protected IEnumerator InitializeTrackers()
    {
        if (runTutorial)
        {
            tutorialInitialization.Invoke();
        }
        
        if (_transformTrackers == null && !PopulateTrackers())
        {
            beforeInitialization.Invoke();
            yield return _waitFixed;
            
            yield break;
        }
        
        System.Diagnostics.Debug.Assert(_transformTrackers != null, "[ERROR] No TransformTrackers found in scene.");
        foreach (var tracker in _transformTrackers)
        {
            tracker.Initialize();
            yield return null;
        }
        yield return _waitFixed;
        
        beforeInitialization.Invoke();
    }
    
    public void StartGame()
    {
        if (runTutorial)
        {
            onTutorialGameStart.Invoke();
            return;
        }
        onGameStart.Invoke();
    }
}
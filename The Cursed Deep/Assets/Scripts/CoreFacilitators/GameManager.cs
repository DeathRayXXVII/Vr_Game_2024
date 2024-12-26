using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    [SerializeField] private SceneBehavior _sceneBehavior;
    [SerializeField] private bool startGameAfterSceneBehaviorTransition = true;
    private bool runTutorial => _tutorialData != null && _tutorialData.Count != 0 && _currentTutorialIndex != -1;
    
    private List<TransformTracker> _transformTrackers;
    
    private bool PopulateTrackers()
    {
        var trackers = FindObjectsOfType<TransformTracker>();
        _transformTrackers = trackers != null && trackers.Length != 0 ? new List<TransformTracker>(trackers) : null;
        return _transformTrackers != null;
    }

    [System.Serializable]
    private struct TutorialData
    {
        public BoolData _tutorialIsActive;
        public UnityEvent initializationEvent;
        public UnityEvent StartEvent;
    }
    
    private int _currentTutorialIndex = -1;
    
    [SerializeField] private List<TutorialData> _tutorialData;
    private UnityEvent tutorialInitialization => runTutorial ? 
        _tutorialData[_currentTutorialIndex].initializationEvent : null;
    private UnityEvent onTutorialGameStart => runTutorial ?
        _tutorialData[_currentTutorialIndex].StartEvent : null;
    
    public UnityEvent beforeInitialization;
    public UnityEvent onGameStart;

    private readonly WaitForFixedUpdate _waitFixed = new();
    protected Coroutine _initCoroutine;
    public bool initialized { get; protected set; }
    
    private void SetupTutorial()
    {
        if (_tutorialData == null || _tutorialData.Count == 0)
        {
            return;
        }
        
        for (var i = 0; i < _tutorialData.Count; i++)
        {
            if (!_tutorialData[i]._tutorialIsActive) continue;
            _currentTutorialIndex = i;
            return;
        }
        
        _currentTutorialIndex = -1;
    }
    
    protected virtual void Awake()
    {
        initialized = false;
        PopulateTrackers();
        SetupTutorial();
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
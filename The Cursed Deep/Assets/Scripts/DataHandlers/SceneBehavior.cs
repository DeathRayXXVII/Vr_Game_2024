using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using ZPTools;

public class SceneBehavior : MonoBehaviour
{
    [SerializeField] private bool allowDebug;
    
    [Tooltip("TransformBehavior that will be used to set the player's position at the start of the scene.")]
    [SerializeField] private TransformBehavior playerTransform;
    
    [Tooltip("TransformData that will be used to set the player's position at the start of the scene.")]
    [SerializeField] private TransformData initialPlayerTransform;

    [Tooltip("Animator that will be used to transition between scenes.")]
    [SerializeField] private ScreenManager screenManager;
    
    [Tooltip("Should the transition animation be played when loading a scene?")]
    [SerializeField] private bool transitionOnLoad = true;
    
    [Tooltip("Additive time in seconds to wait before loading the scene.")]
    [SerializeField, SteppedRange(0, 10, 0.1f)] private float loadBuffer = 1f;
    
    [Tooltip("UnityEvent that will be invoked before the scene is loaded.")]
    [SerializeField] private UnityEvent beforeLoadIn;
    [Tooltip("UnityEvent that will be invoked after the scene is loaded.")]
    [SerializeField] private UnityEvent onLoadInComplete;
    
    private readonly WaitForFixedUpdate _waitFixed = new();
    private Coroutine _loadCoroutine;
    private Coroutine _initializeCoroutine;
    
    private void OnDisable()
    {
        if (_loadCoroutine != null)
        {
            StopCoroutine(_loadCoroutine);
            _loadCoroutine = null;
        }
        if (_initializeCoroutine != null)
        {
            StopCoroutine(_initializeCoroutine);
            _initializeCoroutine = null;
        }
    }
    
    private bool _isInitialized;
    public bool isInitialized { get => _isInitialized; private set => _isInitialized = value; }
    
    public IEnumerator Initialize()
    {
        _initializeCoroutine ??= StartCoroutine(InitializeAndTransitionIn());
        yield return new WaitUntil(() => isInitialized || _initializeCoroutine == null);
    }

    private IEnumerator InitializeAndTransitionIn()
    {
        // Wait until all trackers are initialized.
        if (isInitialized)
        {
            yield break;
        }
        
        // Wait until the player is positioned.
        yield return PositionPlayer();
        yield return _waitFixed;
        
        // Proceed with the transition.
// #if UNITY_EDITOR
        if (allowDebug) 
            Debug.Log("[DEBUG] Scene Initialization Complete. Performing before load in actions.", this);
// #endif
        
        // Invoke the beforeLoadIn event.
        beforeLoadIn.Invoke();
        yield return _waitFixed;
// #if UNITY_EDITOR
        if (allowDebug && transitionOnLoad) 
            Debug.Log("[DEBUG] Before load in Complete. Attempting transition in.", this);
// #endif
        
        // Perform Buffer before transitioning calling transition in.
        yield return FixedUpdateBuffer(loadBuffer);

        if (transitionOnLoad)
        {
            // Wait for the transition to complete.
            yield return screenManager.TransitionIn();
            yield return _waitFixed;
        }

// #if UNITY_EDITOR
        if (allowDebug) 
            Debug.Log($"[DEBUG] {(transitionOnLoad? "Transition in complete" : "Before load in complete")}. " +
                      $"Performing after load in actions.", this);
// #endif

        // Invoke the onLoadInComplete event.
        onLoadInComplete.Invoke();
        yield return _waitFixed;

        // Start the game if the initialization manager is set and the game should start after transition.
        isInitialized = true;
        _initializeCoroutine = null;
    }
    
    private bool PositionPlayer()
    {
        if (playerTransform == null)
        {
            Debug.LogError("[ERROR] Player Transform is null, cannot complete positioning player.", this);
            return false;
        }
        if (initialPlayerTransform == null)
        {
            Debug.LogError("[ERROR] Initial Player Transform is null, cannot complete positioning player.", this);
            return false;
        }
        
        playerTransform.SetTransform(initialPlayerTransform);
        playerTransform.SetStartTransform(initialPlayerTransform);
        
        return true;
    }
    
    public void RestartActiveScene() => LoadScene(SceneManager.GetActiveScene().name);
    
    private bool _sceneLoaded;
    public void LoadScene(string scene)
    {
        var sceneIndex = SceneUtility.GetBuildIndexByScenePath(scene);
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
// #if UNITY_EDITOR
            if (allowDebug) Debug.Log($"[DEBUG] Attempting to load Scene: {scene}", this);
// #endif
            LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError($"[ERROR] Scene: '{scene}' not found.", this);
        }
    }
    
    public void LoadScene(int sceneIndex)
    {
        if (_loadCoroutine != null)
        {
            Debug.LogWarning("[WARNING] Scene loading already in progress.\nPotentially working on --\n" +
                             $"Transition Out: {screenManager.isTransitioning}\nBuffering: {_buffering}\n" +
                             $"Loading: {!_sceneLoaded}", this);
            return;
        }
// #if UNITY_EDITOR
        var scene = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
        if (!string.IsNullOrEmpty(scene) && allowDebug)
            Debug.Log($"[DEBUG] Loading Scene: {scene}, Index: {sceneIndex}", this);
// #endif
        var asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        try
        {
            asyncLoad!.allowSceneActivation = false;
        }
        catch (NullReferenceException)
        {
            Debug.LogError($"[ERROR] Scene at Index: '{sceneIndex}' not found.", this);
            return;
        }
        
        _sceneLoaded = false;
        _loadCoroutine ??= StartCoroutine(LoadAndTransitionOut(asyncLoad));
    }
    
    private IEnumerator LoadAndTransitionOut(AsyncOperation loadOperation)
    {
        Debug.Log($"[DEBUG] Scene Loading in progress. Performing Transition Out.", this);
        yield return StartCoroutine(screenManager.TransitionOut());
        
        Debug.Log($"[DEBUG] Transition Out complete. Performing Background Load.", this);
        yield return StartCoroutine(BackgroundLoad(loadOperation));
        
        Debug.Log($"[DEBUG] Load and Transition Out complete. Performing Buffer.", this);
        StartCoroutine(FixedUpdateBuffer(loadBuffer));
        yield return new WaitUntil(() => !_buffering);
        
        _loadCoroutine = null;
        loadOperation.allowSceneActivation = true;
    }
    
    private IEnumerator BackgroundLoad(AsyncOperation loadOperation)
    {
        Debug.Log($"[DEBUG] Loading Scene in background, Progress: {loadOperation.progress}", this);
        while (!loadOperation.isDone && loadOperation.progress < 0.9f)
        {
            Debug.Log($"[DEBUG] Loading Scene in background, Progress: {loadOperation.progress}", this);
            yield return _waitFixed;
        }
        _sceneLoaded = true;
        yield return _waitFixed;
    }
    
    private bool _buffering;
    private IEnumerator FixedUpdateBuffer(float waitTime = 5f)
    {
        _buffering = true;
        var time = Time.time;
        float elapsedTime = 0;
        
#if UNITY_EDITOR
        var debugSpacer = 0;
        const int mod = 20;
#endif

        while (elapsedTime <= waitTime)
        {
#if UNITY_EDITOR
            if (allowDebug && debugSpacer++ % mod == 0)
            {
                Debug.Log($"[DEBUG] Running Load Buffer, Time: {Time.time}, Elapsed Time: {elapsedTime} / {waitTime} " +
                          $"Complete: {elapsedTime - time < waitTime}", this);
            }
#endif   
            yield return _waitFixed;
            elapsedTime = Time.time - time;
        }
        
        if (allowDebug) 
            Debug.Log($"[DEBUG] Buffer completed at game time: {Time.time}, Time Elapsed: {elapsedTime}", this);
        
        yield return null;
        _buffering = false;
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using ZPTools;

public class SceneBehavior : MonoBehaviour
{
    [SerializeField] private bool allowDebug;
    
    [Tooltip("TransformBehavior that will be used to set the player's position at the start of the scene.")]
    [SerializeField] private TransformBehavior playerTransform;
    
    [Tooltip("TransformData that will be used to set the player's position at the start of the scene.")]
    [SerializeField] private TransformData initialPlayerTransform;
    
    [Tooltip("Y Offset to add to the player's position.")]
    [SerializeField, Range(0, 10)] private float cameraYOffset = 1;

    [Tooltip("Animator that will be used to transition between scenes.")]
    [SerializeField] private ScreenManager screenManager;
    
    [Tooltip("Should the transition animation be played when loading a scene?")]
    [SerializeField] private bool transitionOnLoad = true;
    [SerializeField] private bool skipAutomaticOrientation;
    
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
            Debug.Log("[INFO] Scene Initialization Complete. Performing before load in actions.", this);
// #endif
        
        // Invoke the beforeLoadIn event.
        beforeLoadIn.Invoke();
        yield return _waitFixed;
// #if UNITY_EDITOR
        if (allowDebug && transitionOnLoad) 
            Debug.Log("[INFO] Before load in Complete. Attempting transition in.", this);
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
            Debug.Log($"[INFO] {(transitionOnLoad? "Transition in complete" : "Before load in complete")}. " +
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
        
        if (skipAutomaticOrientation)
        {
            return true;
        }
        
        var headSetCamera = Camera.main?.gameObject;
        var headSetPositionParent = headSetCamera?.transform.parent.gameObject;
        var headSetRotationParent = headSetPositionParent?.transform.parent.gameObject;
        
        if (headSetCamera == null || headSetPositionParent == null || headSetRotationParent == null)
        {
            Debug.LogError("[ERROR] Head Set Camera, Position Parent or Rotation Parent is null, cannot complete positioning player corrections.", this);
            return false;
        }
        
        headSetPositionParent.transform.localScale = Vector3.one;
        
        var expectedCameraPosition = new Vector3(
            initialPlayerTransform.position.x,
            initialPlayerTransform.position.y + cameraYOffset,
            initialPlayerTransform.position.z
            );
        
        float expectedYaw = initialPlayerTransform.rotation.eulerAngles.y;
        
        var currentCameraPosition = headSetCamera.transform.position;
        float currentYaw = headSetCamera.transform.rotation.eulerAngles.y;
        
        var posDiff = Vector3.Distance(expectedCameraPosition, currentCameraPosition);
        var yawDiff = Mathf.DeltaAngle(currentYaw, expectedYaw);
        
        var withinPosThreshold = posDiff < 0.1f;
        var withinRotThreshold = Mathf.Abs(yawDiff) < 15f;
        
        if (withinPosThreshold && withinRotThreshold)
        {
            return true;
        }

        if (!withinPosThreshold)
        {
            headSetPositionParent.transform.position += expectedCameraPosition - currentCameraPosition;
        }
        
        if (!withinRotThreshold)
        {
            headSetRotationParent.transform.rotation = Quaternion.Euler(
                headSetRotationParent.transform.rotation.eulerAngles + new Vector3(0, expectedYaw - currentYaw, 0)
                );
        }
        
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
            if (allowDebug) Debug.Log($"[INFO] Attempting to load Scene: {scene}", this);
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
            Debug.Log($"[INFO] Loading Scene: {scene}, Index: {sceneIndex}", this);
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
        if (allowDebug) 
            Debug.Log($"[INFO] Scene Loading in progress. Performing Transition Out.", this);
        yield return StartCoroutine(screenManager.TransitionOut());
        
        if (allowDebug) 
            Debug.Log($"[INFO] Transition Out complete. Performing Background Load.", this);
        yield return StartCoroutine(BackgroundLoad(loadOperation));
        
        if (allowDebug) 
            Debug.Log($"[INFO] Load and Transition Out complete. Performing Buffer.", this);
        StartCoroutine(FixedUpdateBuffer(loadBuffer));
        yield return new WaitUntil(() => !_buffering);
        
        _loadCoroutine = null;
        loadOperation.allowSceneActivation = true;
    }
    
    private IEnumerator BackgroundLoad(AsyncOperation loadOperation)
    {
        if (allowDebug) 
            Debug.Log($"[INFO] Loading Scene in background, Progress: {loadOperation.progress}", this);
        while (!loadOperation.isDone && loadOperation.progress < 0.9f)
        {
            if (allowDebug) 
                Debug.Log($"[INFO] Loading Scene in background, Progress: {loadOperation.progress}", this);
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
                Debug.Log($"[INFO] Running Load Buffer, Time: {Time.time}, Elapsed Time: {elapsedTime} / {waitTime} " +
                          $"Complete: {elapsedTime - time < waitTime}", this);
            }
#endif   
            yield return _waitFixed;
            elapsedTime = Time.time - time;
        }
        
        if (allowDebug) 
            Debug.Log($"[INFO] Buffer completed at game time: {Time.time}, Time Elapsed: {elapsedTime}", this);
        
        yield return null;
        _buffering = false;
    }
}
